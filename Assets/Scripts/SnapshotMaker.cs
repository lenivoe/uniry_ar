using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

// делает снимки объектов на слое ArModels
public class SnapshotMaker : MonoBehaviour {
    public enum PhotoFmt { JPG, PNG };
    private string PhotoFmtStr {
        get {
            switch (photoFmt) {
                case PhotoFmt.JPG: return ".jpg";
                case PhotoFmt.PNG: return ".png";
                default: return "";
            }
        }
    }

    public delegate void TextureMadeDelegate(Texture2D tex);
    public event TextureMadeDelegate PhotoTexMade;
    public event TextureMadeDelegate ScreenTexMade;

    public delegate void TextureSavedDelegate(string name);
    public event TextureSavedDelegate PhotoTexSaved;

    
    public PhotoFmt photoFmt = PhotoFmt.JPG;
    public Camera mainCam;
    public Transform realityPlane;
    public Text logText;


    private Camera photoCam = null;
    private Transform photoPlane = null;

    private RenderTexture nextPhotoRenderTex = null;
    private RenderTexture screenshotRenderTex = null;
    private SnapshotMakerHelper photoCamHelper = null;
    private SnapshotMakerHelper screenshotCamHelper = null;

    private Logger logger = null;
    
    private bool needSave = false;
    private Vector2Int specTexSize = Vector2Int.zero;


    // создает камеру для снимков
    void Start() {
        logger = new Logger(logText);

        transform.parent = realityPlane.parent;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        const int photoPlaneSize = 500;
        photoCam = gameObject.AddComponent<Camera>();
        photoCam.cullingMask = (1 << 9) | (1 << 10); // layers ArModels and PhotoPlane
        photoCam.enabled = false;
        photoCam.nearClipPlane = 0.1f;
        photoCam.farClipPlane = photoPlaneSize + 1;
        photoCam.clearFlags = CameraClearFlags.SolidColor;

        photoCamHelper = photoCam.gameObject.AddComponent<SnapshotMakerHelper>();
        photoCamHelper.PostRender += OnPhotoCamPostRender;

        photoPlane = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
        photoPlane.parent = transform;
        photoPlane.transform.localPosition = new Vector3(0, 0, photoPlaneSize);
        photoPlane.transform.localScale = new Vector3(photoPlaneSize, photoPlaneSize, 1);
        photoPlane.transform.localRotation = Quaternion.Euler(0, 180, 180);
        photoPlane.gameObject.layer = 10; // "PhotoPlane" layer
        photoPlane.name = "PhotoPlane";

        int rendTexSize = 2;
        StartCoroutine(Init(rendTexSize));

        screenshotCamHelper = mainCam.gameObject.AddComponent<SnapshotMakerHelper>();
        RefreshScreenshotRenderTex();
    }

    void OnDestroy() {
        OrientationBehaviour.OrientationChanged -= OnOrientationChanged;
        if (photoCamHelper != null)
            photoCamHelper.PostRender -= OnPhotoCamPostRender;
        if(screenshotCamHelper != null)
            screenshotCamHelper.PostRender -= OnScreenshotCamPostRender;
    }
    
    // real size = max(Screen.width, Screen.height) * size
    // init cam aspect and FOV after background picture plane (target) init
    private IEnumerator Init(float size) {
        for (;;) {
            Vector2 scale = new Vector2(realityPlane.localScale.x, realityPlane.localScale.z);
            if (scale == Vector2.one) {
                yield return null;
                continue;
            }

            photoPlane.GetComponent<Renderer>().material = realityPlane.GetComponent<Renderer>().material;
            Vector3 locScale = photoPlane.localScale;
            photoPlane.localScale = new Vector3(locScale.x, locScale.y * scale.y / scale.x, locScale.z);
            scale = new Vector2(photoPlane.localScale.x, photoPlane.localScale.y);

            // set aspect the same one of picture panel
            photoCam.aspect = scale.x / scale.y;
            // set fieldOfView by: 1) distance between cam and picture plane, 2) height of picture plane
            photoCam.fieldOfView = Mathf.Atan2(scale.y / 2, photoPlane.localPosition.z) * 2 * Mathf.Rad2Deg;

            Vector2Int texSize = new Vector2Int();
            if (Screen.orientation == ScreenOrientation.Portrait) {
                texSize.x = (int)(Screen.height * size * photoCam.aspect);
                texSize.y = (int)(Screen.height * size);
            } else {
                texSize.x = (int)(Screen.width * size);
                texSize.y = (int)(Screen.width * size / photoCam.aspect);
            }
            photoCam.targetTexture = new RenderTexture(texSize.x, texSize.y, 24);
            nextPhotoRenderTex = new RenderTexture(texSize.y, texSize.x, 24);

            OrientationBehaviour.OrientationChanged += OnOrientationChanged;
            break;
        }
    }

    private void OnOrientationChanged(ScreenOrientation newOrientation) {
        RenderTexture tmp = photoCam.targetTexture;
        photoCam.targetTexture = nextPhotoRenderTex;
        nextPhotoRenderTex = tmp;

        Vector3 scale = photoPlane.localScale;
        photoPlane.localScale = scale = new Vector3(scale.y, scale.x, scale.z);

        photoCam.aspect = 1 / photoCam.aspect;
        photoCam.fieldOfView = Mathf.Atan2(scale.y / 2, photoPlane.localPosition.z) * 2 * Mathf.Rad2Deg;
    }

    // enable camera for one render
    public void Snapshoot(bool needSave = true) {
        this.needSave = needSave;
        photoCam.enabled = true;
    }
    public void Snapshoot(Vector2Int size) {
        needSave = false;
        specTexSize = size;
        photoCam.enabled = true;
    }

    public void FlipPhotoPlane() {
        Vector3 euler = photoPlane.localRotation.eulerAngles;
        euler.x = (euler.x + 180) % 360;
        photoPlane.localRotation = Quaternion.Euler(euler);
    }

    // save rendered texture once and disable camera
    private void OnPhotoCamPostRender(Camera cam) {
        cam.enabled = false;
        RenderTexture rendTex = (specTexSize == Vector2Int.zero) ? cam.targetTexture :
            new RenderTexture(specTexSize.x, specTexSize.y, 24);
        Texture2D tex = MakeTexture2D(cam, rendTex);

        if (PhotoTexMade != null)
            PhotoTexMade(tex);
        if (needSave) {
            needSave = false;
            SavePhoto(tex);
        }

        specTexSize = Vector2Int.zero;
    }

    //void 

    // сохранение текстуры в файл в галерее
    private void SavePhoto(Texture2D snapshotTex) {
        string filename = DateTime.Now.ToString("ddMMyyyy_HHmmssfff") + PhotoFmtStr;
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        NativeGallery.SaveImageToGallery(snapshotTex, Application.productName, filename, strErr => logger += strErr);
#else
        SaveTexture(snapshotTex, filename);
#endif
        logger += "photo saved: " + filename;
        if (PhotoTexSaved != null)
            PhotoTexSaved(filename);
    }

    // launch making screenshot
    public void Screenshoot() {
        screenshotCamHelper.PostRender += OnScreenshotCamPostRender;
    }

    // use after change screen orientation
    public void RefreshScreenshotRenderTex() {
        screenshotRenderTex = new RenderTexture(Screen.width, Screen.height,
            24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
    }

    // making screenshot
    private void OnScreenshotCamPostRender(Camera cam) {
        Texture2D screenshot = MakeTexture2D(cam, screenshotRenderTex);
        if (ScreenTexMade != null)
            ScreenTexMade(screenshot);
        screenshotCamHelper.PostRender -= OnScreenshotCamPostRender;
    }

    
    // save current RenderTexture to Texture2D
    private Texture2D MakeTexture2D(Camera cam, RenderTexture renderTex) {
        RenderTexture camRenderTex = cam.targetTexture;
        cam.targetTexture = renderTex;
        Texture2D tex2D = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGB24, false);
        tex2D.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex2D.Apply();
        cam.targetTexture = camRenderTex;

        return tex2D;
    }

    // save Texture2D to file with name like <date_time.format>
    private void SaveTexture(Texture2D tex2D, string filename) {
        // setting path for pictures
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        string dirPath = Application.dataPath + "/../_imgs/";
#else
        string dirPath = Application.persistentDataPath + "/";
#endif
        string filePath = dirPath + filename;

        // checking path and writing
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        if (Directory.Exists(dirPath) && !File.Exists(filePath)) {
            using (BinaryWriter binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create))) {
                byte[] bytes = filename.EndsWith(".png") ? tex2D.EncodeToPNG() : tex2D.EncodeToJPG();
                binWriter.Write(bytes);
            }
        } else {
            Debug.LogError("Can't save picture.");
        }
    }

    
}






















/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class SnapshotMaker : MonoBehaviour {
    public enum SnapshotFmt { JPG, PNG };
    public SnapshotFmt snapshotFmt = SnapshotFmt.JPG;
    public Transform target;

    public Text logText;
    private Logger logger = null;

    private Camera snapshotCam;

    void Start() {
        logger = new Logger(logText);

        snapshotCam = GetComponent<Camera>();
        snapshotCam.enabled = false;
        StartCoroutine(InitSnapshotCam(Screen.height * 2)); // tex size = Screen.height * 2

#if UNITY_ANDROID && !UNITY_EDITOR
        Android.PreloadAndroidClasses();
#endif
    }

    // init cam aspect and FOV after background picture plane (target) init
    IEnumerator InitSnapshotCam(int renderTexSize) {
        for(; ;) {
            yield return null;
            Vector3 scale = target.localScale;
            bool TargetIsReady = scale.x != 1 || scale.y != 1 || scale.z != 1;
            if (TargetIsReady) {
                // set aspect the same one of picture panel
                snapshotCam.aspect = scale.x / scale.z;
                // set fieldOfView by: 1) distance between cam and picture plane, 2) height of picture plane
                snapshotCam.fieldOfView = Mathf.Atan2(scale.z / 2, target.localPosition.z) * 2 * Mathf.Rad2Deg;
                snapshotCam.targetTexture = new RenderTexture((int)(renderTexSize * snapshotCam.aspect), renderTexSize, 24);
                break;
            }
        }
    }
    

    // save rendered texture once and disable camera
    void OnPostRender() {
        Texture2D snapshot = MakeSnapshot();

        // save snapshot
#if UNITY_ANDROID && !UNITY_EDITOR
        string curDateTime = DateTime.Now.ToString("MM.dd.yyyy_HH.mm.ss.fff");
        SaveImageToGallery(snapshot, "AR " + curDateTime, "time: " + curDateTime);
#else
        SaveTexture(snapshot, snapshotFmt);
#endif

        snapshotCam.enabled = false;
    }

    // enable camera for one render
    public void Snapshoot() {
        print("boom!");
        snapshotCam.enabled = true;
    }

    // save current RenderTexture to Texture2D
    private Texture2D MakeSnapshot() {
        RenderTexture renderTex = snapshotCam.targetTexture;
        Texture2D tex2D = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGB24, false);
        tex2D.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        tex2D.Apply();

        return tex2D;
    }
    
    // save Texture2D to file with name like <date_time.format>
    private void SaveTexture(Texture2D tex2D, SnapshotFmt snapshotFmt) {
        // setting path for pictures
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        string dirPath = Application.dataPath + "/../_imgs/";
#else
        string dirPath = Application.persistentDataPath + "/";
#endif
        string filePath = dirPath + System.DateTime.Now.ToString("MMddyyyy_HHmmssfff");
        switch (snapshotFmt) {
            case SnapshotFmt.JPG: filePath += ".jpg"; break;
            case SnapshotFmt.PNG: filePath += ".png"; break;
        }

        // checking path and writing
        if (!Directory.Exists(dirPath))
            Directory.CreateDirectory(dirPath);
        if (Directory.Exists(dirPath) && !File.Exists(filePath)) {
            using (BinaryWriter binWriter = new BinaryWriter(File.Open(filePath, FileMode.Create))) {
                byte[] bytes = null;
                switch (snapshotFmt) {
                    case SnapshotFmt.JPG: bytes = tex2D.EncodeToJPG(); break;
                    case SnapshotFmt.PNG: bytes = tex2D.EncodeToPNG(); break;
                }
                binWriter.Write(bytes);
            }
        } else {
            Debug.LogError("Can't save picture.");
        }
    }


    // (unused)
    private Texture2D ConvertTexToTex2D(Texture tex) {
        RenderTexture renderTex = new RenderTexture(tex.width, tex.height, 32);
        Graphics.Blit(tex, renderTex);

        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTex;

        Texture2D tex2D = new Texture2D(tex.width, tex.height, TextureFormat.RGB24, false);
        for (int y = 0; y < tex.height; y++)
            tex2D.ReadPixels(new Rect(0, y, renderTex.width, 1), 0, y);
        tex2D.Apply();

        RenderTexture.active = currentRT;

        return tex2D;
    }




    //
    // for android
    //

    public class Android {
        public static void PreloadAndroidClasses() {
            var tmpUnityPlayer = UnityPlayer;
            var tmpMediaClass = MediaClass;
            var tmpBmpFactory = BmpFactory;

            var tmpActivity = Activity;
            var tmpContentResolver = ContentResolver;
        }


        public static AndroidJavaClass UnityPlayer {
            get {
                if (m_UnityPlayer == null)
                    m_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                return m_UnityPlayer;
            }
        }
        public static AndroidJavaClass MediaClass {
            get {
                if (m_MediaClass == null)
                    m_MediaClass = new AndroidJavaClass("android.provider.MediaStore$Images$Media");
                return m_MediaClass;
            }
        }
        public static AndroidJavaClass BmpFactory {
            get {
                if (m_BmpFactory == null)
                    m_BmpFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
                return m_BmpFactory;
            }
        }

        public static AndroidJavaObject Activity {
            get {
                if (m_Activity == null)
                    m_Activity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                return m_Activity;
            }
        }
        public static AndroidJavaObject ContentResolver {
            get {
                if (m_ContentResolver == null)
                    m_ContentResolver = Activity.Call<AndroidJavaObject>("getContentResolver");
                return m_ContentResolver;
            }
        }


        private static AndroidJavaClass m_UnityPlayer = null;
        private static AndroidJavaClass m_MediaClass = null;
        private static AndroidJavaClass m_BmpFactory = null;

        private static AndroidJavaObject m_Activity = null;
        private static AndroidJavaObject m_ContentResolver = null;
    }



    public static string SaveImageToGallery(Texture2D texture2D, string title, string description) {
        AndroidJavaObject image = Texture2DToAndroidBitmap(texture2D);
        var imageUrl = Android.MediaClass.CallStatic<string>("insertImage", Android.ContentResolver, image, title, description);
        return imageUrl;
    }

    public static AndroidJavaObject Texture2DToAndroidBitmap(Texture2D texture2D) {
        byte[] encoded = texture2D.EncodeToJPG();
        return Android.BmpFactory.CallStatic<AndroidJavaObject>("decodeByteArray", encoded, 0, encoded.Length);
    }
}
*/

