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
    private YandexDownloader downloader = null;
    private MessagerBehaviour messager = null;
    private string cachePath = null;

    protected void Start() {
        downloader = new YandexDownloader();
        messager = FindObjectOfType<MessagerBehaviour>();

        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClick);
    }
    protected void OnDestroy() { }

    public void OnButtonClick() {
        messager.ShowMessege("test", float.PositiveInfinity);
        Action<string, bool> callback = (string directLink, bool isTimeout) => {
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
                    print("start activity"); //PdfViewer.StartActivityAsync(filename);
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
        };

        downloader.GetDirectLinkAsync(callback, yandexLink, 3000);
    }
}
