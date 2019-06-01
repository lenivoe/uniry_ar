using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System;
using System.Timers;

public class YandexDownloader {
    public static bool IsSomethingDownloading { get { return downloadingCount != 0; } }
    private static uint downloadingCount = 0;

    public event Action<string, bool> DownloadComplete;
    public event Action<string, long, long> DownloadProgressChaged;

    private const string mainUri = @"https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key=";
    private WebClient webClient = new WebClient();
    private WebRequest curAsyncRequest = null;
    private Action<string, bool> curAsyncRequestCallback = null;
 
    private Timer requestTimeoutTimer = new Timer();
    private Timer downloadTimeoutTimer = new Timer();
    private const long baseTimeout = 60000;

    private long curBytesReceived = 0;
    private long curTotalBytes = 0;
    private string curFileName = "";



    public YandexDownloader() {
        ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
        webClient.DownloadFileCompleted += DownloadFileCompleted;
        webClient.DownloadProgressChanged += DownloadProgressChanged;

        requestTimeoutTimer.AutoReset = false;
        requestTimeoutTimer.Elapsed += (object sender, ElapsedEventArgs e) => {
            if (curAsyncRequest != null) {
                curAsyncRequest.Abort();
                curAsyncRequest = null;
                curAsyncRequestCallback(null, true);
                curAsyncRequestCallback = null;
                Debug.Log("async request aborted: timeout");
            }
        };

        downloadTimeoutTimer.AutoReset = false;
        downloadTimeoutTimer.Elapsed += (object sender, ElapsedEventArgs e) => {
            if (webClient.IsBusy) {
                webClient.CancelAsync();
                Debug.Log("download cancelled: timeout");
            }
        };
    }

    public void GetDirectLinkAsync(Action<string, bool> getLinkCallback, string redirectLink, long timeout = baseTimeout) {
        curAsyncRequestCallback = getLinkCallback;
        requestTimeoutTimer.Interval = timeout;
        requestTimeoutTimer.Start();
        
        curAsyncRequest = WebRequest.Create(mainUri + redirectLink);
        Debug.Log("request created " + (curAsyncRequest != null) + ": " + mainUri + redirectLink);
        AsyncCallback requestCallback = (IAsyncResult asyncResult) => {
            Debug.Log("request asyncResult: " + asyncResult.IsCompleted);
            WebRequest request = curAsyncRequest;
            curAsyncRequest = null; // защита от лишнего вызова таймера
            requestTimeoutTimer.Stop();

            string directLink = null;
            bool isTimeout = true;
            try {
                WebResponse response = request.EndGetResponse(asyncResult);
                Debug.Log("response content length: " + response.ContentLength);
                if (response.ContentLength <= 0) { // ответ без данных
                    request.Abort();
                    return;
                }
                string responseStr = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Debug.Log("async response got: " + responseStr);
                directLink = GetSubstring(responseStr, "\"href\":\"", "\"");
                isTimeout = false;
            } catch(Exception) {
                request.Abort();
            } finally {
                curAsyncRequestCallback(directLink, isTimeout);
                curAsyncRequestCallback = null;
            }
        };
        curAsyncRequest.BeginGetResponse(requestCallback, null);
        Debug.Log("async request sent");
    }

    public string GetDirectLink(string redirectLink) {
        Debug.Log("create request: " + mainUri + redirectLink);
        WebRequest request = WebRequest.Create(mainUri + redirectLink);
        Stream responseStream = request.GetResponse().GetResponseStream();
        Debug.Log("response got");

        string responseStr = new StreamReader(responseStream).ReadToEnd();
        return GetSubstring(responseStr, "\"href\":\"", "\"");
    }

    public string GetFilenameFromLink(string directLink) {
        return WWW.UnEscapeURL(GetSubstring(directLink, "filename=", "&"));
    }

    public long GetFileSizeFromLink(string directLink) {
        var webRequest = WebRequest.Create(directLink);
        webRequest.Method = "HEAD";
        using (var webResponse = webRequest.GetResponse()) {
            return webResponse.ContentLength;
        }
    }

    public void DirectDownloadAsync(string directLink, string filename, long timeout = baseTimeout) {
        downloadTimeoutTimer.Interval = timeout;
        downloadTimeoutTimer.Start();

        curFileName = filename;
        webClient.DownloadFileAsync(new Uri(directLink), curFileName);
        downloadingCount++;

        Debug.Log("link: " + directLink);
        Debug.Log("curFileName: " + curFileName);
    }

    public void RedirectDownloadAsync(string redirectLink) {
        string directUri = GetDirectLink(redirectLink);
        DirectDownloadAsync(directUri, GetFilenameFromLink(directUri));
    }

    private static string GetSubstring(string src, string begin, string end) {
        int substrBegin = src.IndexOf(begin) + begin.Length;
        if (substrBegin < 0)
            return null;

        int substrEnd = src.IndexOf(end, substrBegin);
        if (substrEnd < 0)
            return null;

        return src.Substring(substrBegin, substrEnd - substrBegin);
    }
    
    private void DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e) {
        downloadTimeoutTimer.Stop();

        if (downloadingCount > 0) {
            downloadingCount--;
        }
        bool isCancelled = (curBytesReceived != curTotalBytes) || e.Cancelled;
        if (isCancelled) {
            File.Delete(curFileName);
        }
        if (DownloadComplete != null) {
            DownloadComplete(curFileName, isCancelled);
        }
    }

    private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e) {
        downloadTimeoutTimer.Stop();
        downloadTimeoutTimer.Start();
        curBytesReceived = e.BytesReceived;
        curTotalBytes = e.TotalBytesToReceive;
        if (DownloadProgressChaged != null) {
            DownloadProgressChaged(curFileName, e.BytesReceived, e.TotalBytesToReceive);
        }
    }

    private bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
        bool isOk = true;
        // If there are errors in the certificate chain, look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None) {
            for (int i = 0; i < chain.ChainStatus.Length; i++) {
                if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown) {
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2)certificate);
                    if (!chainIsValid) {
                        isOk = false;
                    }
                }
            }
        }
        return isOk;
    }
}
