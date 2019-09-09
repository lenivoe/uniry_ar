using UnityEngine;
using VPlayer = EasyAR.VideoPlayerBaseBehaviour;

public class ImageTargetBehaviour_YandexVideo : ImageTargetBehaviour {
    public enum VideoScaleType { Fill, FitWidth, FitHeight, Fit }
    
    public VideoScaleType videoScaleType = VideoScaleType.Fill;
    public string yandexLink;

    private readonly YandexDownloader downloader = new YandexDownloader();
    private MessagerBehaviour messager = null;
    private VPlayer player = null;
    private bool needOpenPlayer = false;
    private bool isGettingLink = false;

    protected override void Start() {
        base.Start();
        
        messager = FindObjectOfType<MessagerBehaviour>();
        TargetFound += OnTargetFound;
        player = AttachPlayer(transform);
        player.VideoErrorEvent += OnVideoError;
        player.VideoReadyEvent += OnVideoReady;
    }

    protected override void Update() {
        base.Update();
        if(needOpenPlayer) {
            needOpenPlayer = false;
            player.Open();
            TargetFound -= OnTargetFound;
        }
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        TargetFound -= OnTargetFound;
        if (player != null) {
            player.VideoErrorEvent -= OnVideoError;
            player.VideoReadyEvent -= OnVideoReady;
        }
    }

    private static VPlayer AttachPlayer(Transform playerParent) {
        GameObject playerObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
        playerObj.name = "StreamPlayer";
        Destroy(playerObj.GetComponent<Collider>());

        Transform playerTransform = playerObj.transform;
        playerTransform.parent = playerParent;
        playerTransform.localPosition = Vector3.zero;
        playerTransform.localRotation = Quaternion.Euler(90, 180, 0);
        playerTransform.localScale = Vector3.one;

        VPlayer curPlayer = playerObj.AddComponent<VPlayer>();
        curPlayer.VideoScaleMode = VPlayer.ScaleMode.None; // others don't work
        curPlayer.EnableLoop = true;
        curPlayer.DisplayTextMessage = false;
        curPlayer.Storage = EasyAR.StorageType.Absolute;

        return curPlayer;
    }

    private void OnTargetFound(EasyAR.TargetAbstractBehaviour obj) {
        if (isGettingLink)
            return;

        if (!AppController.Inst.HaveInetConnection) {
            messager.SetMessege("Проблема с интернет-соединением...");
            return;
        }
        messager.SetMessege("Загрузка видео...", float.PositiveInfinity);

        isGettingLink = true;
        downloader.GetDirectLinkAsync((string directLink, bool isTimeout) => {
            isGettingLink = false;
            if (isTimeout) {
                messager.SetMessege("Проблема с интернет-соединением...");
                return;
            }
            player.Path = directLink;
            needOpenPlayer = true;
        }, yandexLink);
    }

    private void OnVideoError(object sender, System.EventArgs e) {
        Debug.Log("video loading error: " + player.Path);
    }

    private void OnVideoReady(object sender, System.EventArgs e) {
        Debug.Log("video size: " + player.Width() + " x " + player.Height());
        messager.Clear();
        
        float videoAspect = ((float)player.Width()) / player.Height();
        float targetAspect = Size.x / Size.y;
        Vector2 playerScale = Vector2.one;
        if ((videoScaleType == VideoScaleType.FitHeight) || (videoScaleType == VideoScaleType.Fit && videoAspect < 1)) {
            playerScale.x = videoAspect;
            if (targetAspect > 1)
                playerScale /= targetAspect;
        } else if ((videoScaleType == VideoScaleType.FitWidth) || (videoScaleType == VideoScaleType.Fit && videoAspect > 1)) {
            playerScale.y = 1 / videoAspect;
            if (targetAspect < 1)
                playerScale *= targetAspect;
        }
        player.transform.localScale = new Vector3(playerScale.x, playerScale.y, 1);

        player.Play();
    }
}
