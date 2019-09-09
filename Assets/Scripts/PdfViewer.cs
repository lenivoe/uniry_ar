using UnityEngine;
using AndroidWrap;

public class PdfViewer {
    public const string ACTIVITY_CLASS_NAME = "com.plugin58.levis.pdfplugin.PdfViewActivity";


    public static readonly PdfViewer Inst = new PdfViewer();


    private string pdfName = null;
    private bool needStartActivity => pdfName != null;

    
    public void StartActivityAsync(string pdfFilename) {
        if (!needStartActivity) {
            pdfName = pdfFilename;
        }
    }

    // нужно циклично вызывать в главном потоке (в основном из Update какого-нибудь потомка MonoBehaviour)
    public void MainThreadLoop() {
        if(needStartActivity) {
            StartActivity(pdfName);
            pdfName = null;
        }
    }


    public void StartActivity(string pdfFilename) {
        if (string.IsNullOrEmpty(pdfFilename)) {
            ToastMessage.Inst.Show("pdf file name error: " + pdfFilename);
            return;
        }

        var activityClass = new AndroidJavaClass(ACTIVITY_CLASS_NAME);

        var intent = Obj.CreateIntent(Obj.UnityActivity, activityClass);
        string extraMsgName = activityClass.GetStatic<string>("EXTRA_FILENAME");
        intent.Call<AndroidJavaObject>("putExtra", extraMsgName, pdfFilename);

        Obj.UnityActivity.Call("startActivity", intent);

        Debug.Log("activity started with file: " + pdfFilename);
    }


    private PdfViewer() { }
}
