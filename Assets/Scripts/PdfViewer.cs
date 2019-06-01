using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PdfViewer {
    public const string ActivityClassName = "com.plugin58.levis.pdfplugin.PdfViewActivity";

    private static bool needHandleActivity = false;
    private static string filename = null;

    
    public static void StartActivityAsync(string pdfFilename) {
        if (!needHandleActivity) {
            filename = pdfFilename;
            needHandleActivity = true;
        }
    }

    // нужно вызывать циклично в главном потоке (вставить в Update какого-нибудь потомка MonoBehaviour)
    public static void MainThreadLoop() {
        if(needHandleActivity) {
            StartActivity(filename);
            needHandleActivity = false;
        }
    }


    public static void StartActivity(string pdfFilename) {
        if (string.IsNullOrEmpty(ActivityClassName)) {
            ToastMessage.Inst.Show("activity class name error: " + ActivityClassName);
            return;
        }
        if (string.IsNullOrEmpty(pdfFilename)) {
            ToastMessage.Inst.Show("pdf file name error: " + pdfFilename);
            return;
        }

        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass activityClass = new AndroidJavaClass(ActivityClassName);
        AndroidJavaObject intent = new AndroidJavaObject("android.content.Intent", unityActivity, activityClass);

        string extraMsgName = activityClass.GetStatic<string>("EXTRA_FILENAME");
        string filename = pdfFilename[0] == '/' ? pdfFilename : Application.persistentDataPath + "/" + pdfFilename;
        intent.Call<AndroidJavaObject>("putExtra", extraMsgName, filename);
        unityActivity.Call("startActivity", intent);

        Debug.Log("activity started with file: " + filename);
    }
}
