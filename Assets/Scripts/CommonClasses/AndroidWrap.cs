using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class AndroidWrap {
    public static AndroidJavaClass UnityPlayerCl {
        get {
            if (m_UnityPlayerCl == null)
                m_UnityPlayerCl = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return m_UnityPlayerCl;
        }
    }
    public static AndroidJavaClass MediaStoreCl {
        get {
            if (m_MediaStoreCl == null)
                m_MediaStoreCl = new AndroidJavaClass("android.provider.MediaStore$Images$Media");
            return m_MediaStoreCl;
        }
    }
    public static AndroidJavaClass BmpFactoryCl {
        get {
            if (m_BmpFactoryCl == null)
                m_BmpFactoryCl = new AndroidJavaClass("android.graphics.BitmapFactory");
            return m_BmpFactoryCl;
        }
    }

    public static AndroidJavaObject ActivityObj {
        get {
            if (m_ActivityObj == null)
                m_ActivityObj = UnityPlayerCl.GetStatic<AndroidJavaObject>("currentActivity");
            return m_ActivityObj;
        }
    }
    public static AndroidJavaObject ContentResolverObj {
        get {
            if (m_ContentResolverObj == null)
                m_ContentResolverObj = ActivityObj.Call<AndroidJavaObject>("getContentResolver");
            return m_ContentResolverObj;
        }
    }
    public static AndroidJavaObject AudioManagerObj {
        get {
            if (m_AudioManagerObj == null)
                m_AudioManagerObj = ActivityObj.Call<AndroidJavaObject>("getSystemService", "audio");
            return m_AudioManagerObj;
        }
    }

    private static AndroidJavaClass m_UnityPlayerCl = null;
    private static AndroidJavaClass m_MediaStoreCl = null;
    private static AndroidJavaClass m_BmpFactoryCl = null;

    private static AndroidJavaObject m_ActivityObj = null;
    private static AndroidJavaObject m_ContentResolverObj = null;
    private static AndroidJavaObject m_AudioManagerObj = null;



}

