/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
using System.Timers;
using VPlayer = EasyAR.VideoPlayerBaseBehaviour;

public class TestButton : MonoBehaviour {
    Button button = null;


    public string yandexLink;
    public float timeout = 1;

    private YandexDownloader downloader = new YandexDownloader();
    private MessagerBehaviour messager = null;
    private string cachePath = null;

    protected void Start() {
        downloader.DownloadComplete += OnDownloadComplete;
        downloader.DownloadProgressChaged += OnDownloadProgressChaged;

        messager = FindObjectOfType<MessagerBehaviour>();

        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }
    protected void OnDestroy() { }

    public void OnButtonClick() {
        messager.SetMessege("test", float.PositiveInfinity);
        downloader.GetDirectLinkAsync(getLinkCallback, yandexLink, (long)(timeout * 1000));
    }

    private void getLinkCallback(string directLink, bool isTimeout) {
        if (isTimeout) {
            Debug.LogWarning("downloading aborted");
            
            messager.SetMessege("Проблема с интернет-соединением...");
            return;
        }

        string filename = cachePath + downloader.GetFilenameFromLink(directLink);
        FileInfo fileInfo = new FileInfo(filename);

        Debug.Log((fileInfo.Exists ? "exists" : "doesn't exist") + ": " + fileInfo.FullName);

        if (fileInfo.Exists) { // файл выкачан
            if (fileInfo.Length == downloader.GetFileSizeFromLink(directLink)) { // файл цел, открываем
                messager.Clear();
                Debug.Log("start activity"); //PdfViewer.StartActivityAsync(filename);
                return;
            }

            // файл выкачан частично, удаляем, качаем заного
            Debug.Log("file corrupted, removing: " + filename);
            fileInfo.Delete();
        }
        
        if (YandexDownloader.DownloadsCount == 0) { // файла нет и ничего не качается => качаем
            messager.SetMessege("Начало загрузки...", float.PositiveInfinity);
            downloader.DirectDownloadAsync(directLink, filename);
        }
    }

    private void OnDownloadComplete(string filename, bool isCancelled) {
        Debug.LogFormat("{0}: download {1}", filename, (isCancelled ? "cancelled" : "completed"));
        
        if (isCancelled)
            messager.SetMessege("Загрузка прервана");
        else {
            messager.Clear();
            Debug.Log("start activity"); //PdfViewer.StartActivityAsync(filename);
        }
    }

    private void OnDownloadProgressChaged(string filename, long bytesReceived, long totalBytes) {
        const double toMb = 1 / (1024.0 * 1024.0);
        string progress = string.Format("Загрузка: {0}% ({1:F2} / {2:F2} mb)", 
            (int)(bytesReceived * 100.0 / totalBytes), bytesReceived * toMb, totalBytes * toMb);
        messager.SetMessege(progress, float.PositiveInfinity);
    }
    
}
/**/

using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour {
    Button button = null;


    public string yandexLink;
    public float timeout = 1;

    private YDownloader downloader = new YDownloader();
    private MessagerBehaviour messager = null;

    protected void Start() {
        messager = FindObjectOfType<MessagerBehaviour>();

        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }
    protected void OnDestroy() { }

    public void OnButtonClick() {
        messager.SetMessege("testing...", float.PositiveInfinity);
        downloader.DownloadFileAsync(OnDownloadComplete, OnDownloadProgressChanged, yandexLink, (int)(timeout * 1000));
    }

    private void OnDownloadComplete(string filename, YDownloader.DownloadResult result) {
        Debug.LogFormat("{0}: {1}", filename, result);
        
        if (result == YDownloader.DownloadResult.SUCCESS || result == YDownloader.DownloadResult.FILE_EXIST) {
            messager.Clear();
            Debug.Log("start activity"); //PdfViewer.StartActivityAsync(filename);
        } else {
            messager.SetMessege("Загрузка не удалась");
        }
    }

    private void OnDownloadProgressChanged(string filename, long bytesReceived, long totalBytes) {
        const double toMb = 1 / (1024.0 * 1024.0);
        string progress = string.Format("Загрузка: {0}% ({1:F2} / {2:F2} mb)", 
            (int)(bytesReceived * 100.0 / totalBytes), bytesReceived * toMb, totalBytes * toMb);
        messager.SetMessege(progress, float.PositiveInfinity);
    }
    
}

 /**/