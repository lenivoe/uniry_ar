using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;

using Timer = System.Timers.Timer;

public class YDownloader {
    public enum DownloadResult { NONE, CORRUPTED, TIMEOUT, FILE_EXIST, SUCCESS }
    
    public bool IsDownloading { get; private set; } = false;

    private const string MAIN_URI = @"https://cloud-api.yandex.net/v1/disk/public/resources/download?public_key=";
    private const int BASE_TIMEOUT = 60000;


    private Action<string, DownloadResult> m_CompliteCallback;
    private Action<string, long, long> m_ProgressCallback;

    
    private readonly WebClient m_WebClient = new WebClient();
    private readonly Timer m_DownloadTimeout = new Timer();
    private DownloadResult m_Result = DownloadResult.NONE;
    private string m_Filename;
    private string m_PathToFile;



    public YDownloader() {
        ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
        m_WebClient.DownloadFileCompleted += (sender, e) => CompliteDownloading();
        m_WebClient.DownloadProgressChanged += WebClient_OnProgress;

        m_DownloadTimeout.AutoReset = false;
        m_DownloadTimeout.Elapsed += (sender, e) => {
            if (m_WebClient.IsBusy) {
                m_Result = DownloadResult.TIMEOUT;
                m_WebClient.CancelAsync();
                
                Debug.LogWarning("m_DownloadTimeout: timeout");
            }
        };
    }

    public void DownloadFileAsync(Action<string, DownloadResult> onComplite,
                                  Action<string, long, long> onProgress,
                                  string link,
                                  int timeout = BASE_TIMEOUT)
    {
        Debug.Assert(m_Result == DownloadResult.NONE, "file already downloading");
        
        m_PathToFile = m_PathToFile ?? Application.temporaryCachePath + '/';

        m_CompliteCallback = onComplite;
        m_ProgressCallback = onProgress;
        m_Result = DownloadResult.CORRUPTED;
        IsDownloading = true;

        Task.Factory.StartNew(() => {
            try {
                using(var response = GetResponse(MAIN_URI + link, ref timeout)) {
                    var dirrectLink = GetDirrectLink(response);

                    m_Filename = GetFilenameFromLink(dirrectLink);
                    var fileInfo = new FileInfo(m_Filename);
                    
                    if(fileInfo.Exists && fileInfo.Length == RequestFileSize(dirrectLink, ref timeout)){
                        m_Result = DownloadResult.FILE_EXIST;
                        CompliteDownloading();
                    } else {
                        m_DownloadTimeout.Interval = timeout;
                        m_DownloadTimeout.Start();
                        m_WebClient.DownloadFileAsync(new Uri(dirrectLink), m_Filename);
                    }
                }
            } catch(WebException e) {
                Debug.LogWarning("web exception status: " + e.Status);

                if(e.Status == WebExceptionStatus.Timeout) {
                    m_Result = DownloadResult.TIMEOUT;

                }
                CompliteDownloading();
            } catch(Exception e) {
                Debug.LogError("unexpected exception: " + e);
            }
        });
    }

    private void CompliteDownloading() {
        m_DownloadTimeout.Stop();
        
        bool needDelete = (m_Result == DownloadResult.TIMEOUT || m_Result == DownloadResult.CORRUPTED)
                            && File.Exists(m_Filename);
        if(needDelete) {
            File.Delete(m_Filename);
        }

        var result = m_Result;
        var filename = m_Filename;

        m_Filename = null;
        m_Result = DownloadResult.NONE;
        IsDownloading = false;

        m_CompliteCallback?.Invoke(filename, result);

        Debug.LogFormat("download complite {0}: {1}", result, filename);
    }

    private void WebClient_OnProgress(object sender, DownloadProgressChangedEventArgs e) {
        m_DownloadTimeout.Stop();
        m_DownloadTimeout.Start();

        if(e.BytesReceived == e.TotalBytesToReceive) {
            m_Result = DownloadResult.SUCCESS;
        }

        m_ProgressCallback?.Invoke(m_Filename, e.BytesReceived, e.TotalBytesToReceive);
    }


    private static string GetDirrectLink(WebResponse response) {
        using(var reader = new StreamReader(response.GetResponseStream())) {
            return GetSubstring(reader.ReadToEnd(), "\"href\":\"", "\"");
        }
    }

    private static WebResponse GetResponse(string link, ref int timeLeft) {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        var response = GetResponse(link, timeLeft);

        watch.Stop();
        timeLeft = Mathf.Clamp((int)(timeLeft - watch.ElapsedMilliseconds), 0, int.MaxValue);

        return response;
    }

    private static WebResponse GetResponse(string link, int timeout, bool onlyHead = false) {
        WebRequest request = WebRequest.Create(link);
        if(onlyHead) {
            request.Method = "HEAD";
        }
        request.Timeout = timeout;
        return request.GetResponse();
    }

    private string GetFilenameFromLink(string directLink) {
        return m_PathToFile + WWW.UnEscapeURL(GetSubstring(directLink, "filename=", "&"));
    }

    public long RequestFileSize(string directLink, ref int timeLeft) {
        using (var response = GetResponse(directLink, ref timeLeft)) {
            return response.ContentLength;
        }
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

    private bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) {
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
                    isOk = isOk && chainIsValid;
                }
            }
        }
        return isOk;
    }
}
