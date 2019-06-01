using UnityEngine;

public class ToastMessage {
    public static ToastMessage Inst { get { return inst; } }
    private static ToastMessage inst = new ToastMessage();

    private string toastMsg;

    private AndroidJavaObject UnityPlayer { get {
            if (unityPlayer == null)
                unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return unityPlayer;
        } }
    private AndroidJavaObject UnityActivity {
        get {
            if (unityActivity == null)
                unityActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            return unityActivity;
        }
    }
    private AndroidJavaObject Context {
        get {
            if (context == null)
                context = UnityActivity.Call<AndroidJavaObject>("getApplicationContext");
            return context;
        }
    }

    private AndroidJavaClass unityPlayer = null;
    private AndroidJavaObject unityActivity = null;
    private AndroidJavaObject context = null;


    public void Show(string msg) { showToastOnUiThread(msg); }


    private ToastMessage() { }
    
    private void showToastOnUiThread(string msg) {
        toastMsg = msg;
        UnityActivity.Call("runOnUiThread", new AndroidJavaRunnable(showToast));
    }

    private void showToast() {
        Debug.Log(this + ": Running on UI thread");

        AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
        AndroidJavaObject toast = Toast.CallStatic<AndroidJavaObject>("makeText", Context, toastMsg, Toast.GetStatic<int>("LENGTH_SHORT"));
        toast.Call("show");
    }
}