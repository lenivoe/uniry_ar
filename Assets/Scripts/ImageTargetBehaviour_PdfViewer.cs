using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ImageTargetBehaviour_PdfViewer : ImageTargetBehaviour {
    public string yandexLink;
    private YandexDownloader downloader = null;
    private MessagerBehaviour messager = null;
    private bool isGettingLink = false;
    private bool isDownloading = false;
    private string cachePath = null;

    protected override void Start () {
        base.Start();
        cachePath = Application.persistentDataPath + "/";
        downloader = new YandexDownloader();
        downloader.DownloadComplete += OnDownloadComplete;
        downloader.DownloadProgressChaged += OnDownloadProgressChaged;
        TargetFound += OnTargetFound;
        messager = FindObjectOfType<MessagerBehaviour>();
    }

    protected override void OnDestroy() {
        base.OnDestroy();
        TargetFound -= OnTargetFound;
        downloader.DownloadComplete -= OnDownloadComplete;
        downloader.DownloadProgressChaged -= OnDownloadProgressChaged;
    }

    void OnTargetFound(EasyAR.TargetAbstractBehaviour obj) {
        if (isGettingLink || isDownloading)
            return;

        if (!AppController.HaveInetConnection) {
            messager.ShowMessege("Проблема с интернет-соединением...");
            return;
        }
        messager.ShowMessege("Проверка файла...", float.PositiveInfinity);

        isGettingLink = true;
        downloader.GetDirectLinkAsync((string directLink, bool isTimeout) => {
            isGettingLink = false;
            if (isTimeout) {
                messager.ShowMessege("Проблема с интернет-соединением...");
                return;
            }

            messager.Clear();
            string filename = cachePath + downloader.GetFilenameFromLink(directLink);
            FileInfo fileInfo = new FileInfo(filename);
            Debug.Log("file " + (fileInfo.Exists ? "exists" : "doesn't exist") + ": " + filename);
            if (fileInfo.Exists) { // файл выкачан
                if (fileInfo.Length == downloader.GetFileSizeFromLink(directLink)) { // файл цел, открываем
                    PdfViewer.StartActivityAsync(filename);
                    return;
                } else { // файл выкачан частично, удаляем, качаем заного
                    Debug.Log("file corrupted, removing: " + filename);
                    fileInfo.Delete();
                }
            }
            if (YandexDownloader.IsSomethingDownloading) { // файла нет, что-то уже качается, выходим
                return;
            }

            messager.ShowMessege("Начало загрузки...", float.PositiveInfinity);
            downloader.DirectDownloadAsync(directLink, filename);
            isDownloading = true;
        }, yandexLink);
    }

    private void OnDownloadComplete(string filename, bool isCancelled) {
        Debug.Log(filename + (isCancelled ? ": download cancelled" : ": download completed"));
        messager.Clear();
        isDownloading = false;
        if (isCancelled)
            messager.ShowMessege("Загрузка прервана");
        else {
            PdfViewer.StartActivityAsync(filename);
        }
    }

    private void OnDownloadProgressChaged(string filename, long bytesReceived, long totalBytes) {
        string msg = filename + string.Format(@": {0:F2}mb / {1:F2}mb",
            bytesReceived / (1024.0 * 1024.0), totalBytes / (1024.0 * 1024.0));
        Debug.Log(msg);
        messager.ShowMessege("Загрузка: " + (int)(bytesReceived * 100.0 / totalBytes) + "%", float.PositiveInfinity);
    }
}
