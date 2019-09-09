using UnityEngine;
using AndroidWrap;

public class ToastMessage {
    public static readonly ToastMessage Inst = new ToastMessage();

    private string toastMsg;


    public void Show(string msg) { showToastOnUiThread(msg); }


    private ToastMessage() { }
    
    private void showToastOnUiThread(string msg) {
        toastMsg = msg;
        Obj.UnityActivity.Call("runOnUiThread", new AndroidJavaRunnable(showToast));
    }

    private void showToast() {
        Debug.Log(this + ": Running on UI thread");

        var toastLength = Cls.Toast.GetStatic<int>("LENGTH_SHORT");
        var toast = Cls.Toast.CallStatic<AndroidJavaObject>("makeText", Obj.Context, toastMsg, toastLength);
        toast.Call("show");
    }
}