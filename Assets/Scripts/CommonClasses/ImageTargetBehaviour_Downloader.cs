using UnityEngine;

public abstract class ImageTargetBehaviour_Downloader : ImageTargetBehaviour {
    public string yandexLink;

    private readonly YDownloader downloader = new YDownloader();
    private MessagerBehaviour messager = null;



    protected abstract void OnResourceReady(string name);


    protected override void Start () {
        base.Start();

        TargetFound += OnTargetFound;
        messager = FindObjectOfType<MessagerBehaviour>();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        TargetFound -= OnTargetFound;
    }

    void OnTargetFound(EasyAR.TargetAbstractBehaviour obj) {
        if (downloader.IsDownloading)
            return;

        if (!AppController.Inst.HaveInetConnection) {
            messager.SetMessege("Проблемы с соединением...");
            return;
        }

        messager.SetMessege("Загрузка...", float.PositiveInfinity);
        downloader.DownloadFileAsync(OnDownloadComplete, OnDownloadProgressChanged, yandexLink);
    }


    private void OnDownloadComplete(string filename, YDownloader.DownloadResult result) {
        Debug.LogFormat("{0}: {1}", filename, result);
        
        if (result == YDownloader.DownloadResult.SUCCESS || result == YDownloader.DownloadResult.FILE_EXIST) {
            messager.Clear();
            OnResourceReady(filename);
        } else {
            messager.SetMessege("Загрузка не удалась");
        }
    }

    private void OnDownloadProgressChanged(string filename, long bytesReceived, long totalBytes) {
        const double toMb = 1 / (1024.0 * 1024.0);
        string progress = string.Format("Загрузка: {0}%" /*+ " ({1:F2} / {2:F2} mb)"*/, 
            (int)(bytesReceived * 100.0 / totalBytes), bytesReceived * toMb, totalBytes * toMb);
        messager.SetMessege(progress, float.PositiveInfinity);
    }
}
