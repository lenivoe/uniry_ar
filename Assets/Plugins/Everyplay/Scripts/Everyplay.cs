#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define UNITY_5_OR_LATER
#endif

#if !UNITY_EDITOR

#if ((UNITY_IPHONE || UNITY_IOS) && EVERYPLAY_IPHONE)
#define EVERYPLAY_IPHONE_ENABLED
#elif (UNITY_TVOS && EVERYPLAY_TVOS)
#define EVERYPLAY_TVOS_ENABLED
#elif (UNITY_ANDROID && EVERYPLAY_ANDROID)
#define EVERYPLAY_ANDROID_ENABLED
#elif (UNITY_5_OR_LATER && UNITY_STANDALONE_OSX && EVERYPLAY_STANDALONE)
#define EVERYPLAY_OSX_ENABLED
#endif

#else

#if UNITY_5_OR_LATER && UNITY_EDITOR_OSX
#define EVERYPLAY_OSX_ENABLED
#endif

#endif

#if EVERYPLAY_IPHONE_ENABLED || EVERYPLAY_ANDROID_ENABLED
#define EVERYPLAY_BINDINGS_ENABLED
#elif EVERYPLAY_TVOS_ENABLED || EVERYPLAY_OSX_ENABLED
#define EVERYPLAY_CORE_BINDINGS_ENABLED
#endif

#if EVERYPLAY_TVOS_ENABLED
#define EVERYPLAY_NO_FACECAM_SUPPORT
#endif

#if EVERYPLAY_OSX_ENABLED
#if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
#define EVERYPLAY_RESET_BINDINGS_ENABLED
#endif
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;
using System.Collections;
using EveryplayMiniJSON;

public class Everyplay : MonoBehaviour
{
    // Enumerations

    public enum UserInterfaceIdiom
    {
        Phone = 0,
        Tablet,
        TV,
        iPhone = Phone,
        iPad = Tablet
    };

    // Delegates and events

    public delegate void WasClosedDelegate();

    public static event WasClosedDelegate WasClosed;

    public delegate void ReadyForRecordingDelegate(bool enabled);

    public static event ReadyForRecordingDelegate ReadyForRecording;

    public delegate void RecordingStartedDelegate();

    public static event RecordingStartedDelegate RecordingStarted;

    public delegate void RecordingStoppedDelegate();

    public static event RecordingStoppedDelegate RecordingStopped;

    public delegate void FileReadyDelegate(string filepath);

    public static event FileReadyDelegate FileReady;

    public delegate void ThumbnailTextureReadyDelegate(Texture2D texture, bool portrait);

    public static event ThumbnailTextureReadyDelegate ThumbnailTextureReady;

    //public delegate void RequestReadyDelegate(string response);

    //public delegate void RequestFailedDelegate(string error);

    // Private member variables

    //private static string clientId;
    private static bool appIsClosing = false;
    private static bool hasMethods = true;
    private static bool seenInitialization = false;
    private static bool readyForRecording = false;

    #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
    private const string nativeMethodSource = "EveryplayPlugin";
    #else
    private const string nativeMethodSource = "__Internal";
    #endif

    private static Everyplay everyplayInstance = null;

    private static Everyplay EveryplayInstance
    {
        get
        {
            if (everyplayInstance == null && !appIsClosing)
            {
                EveryplaySettings settings = (EveryplaySettings) Resources.Load("EveryplaySettings");

                if (settings != null)
                {
                    if (settings.IsEnabled)
                    {
                        GameObject everyplayGameObject = new GameObject("Everyplay");

                        if (everyplayGameObject != null)
                        {
                            everyplayGameObject.name = everyplayGameObject.name + everyplayGameObject.GetInstanceID();

                            everyplayInstance = everyplayGameObject.AddComponent<Everyplay>();

                            if (everyplayInstance != null)
                            {
                                hasMethods = true;

                                // Initialize the native
                                #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
                                try
                                {
                                    InitEveryplay(everyplayGameObject.name);
                                }
                                catch (DllNotFoundException)
                                {
                                    hasMethods = false;
                                    everyplayInstance.OnApplicationQuit();
                                    return null;
                                }
                                catch (EntryPointNotFoundException)
                                {
                                    hasMethods = false;
                                    everyplayInstance.OnApplicationQuit();
                                    return null;
                                }
                                #endif

                                if (seenInitialization == false)
                                {
                                    #if EVERYPLAY_OSX_ENABLED
                                    #if UNITY_5_OR_LATER
                                    AudioConfiguration config = AudioSettings.GetConfiguration();
                                    AudioSettings.Reset(config);
                                    #endif
                                    #endif
                                }

                                seenInitialization = true;

                                // Add test buttons if requested
                                #if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS
                                if (settings.testButtonsEnabled)
                                {
                                    AddTestButtons(everyplayGameObject);
                                }
                                #endif

                                DontDestroyOnLoad(everyplayGameObject);
                            }
                        }
                    }
                }
            }

            return everyplayInstance;
        }
    }

    // Public static methods

    public static void Initialize()
    {
        // If everyplayInstance is not yet initialized, calling EveryplayInstance property getter will trigger the initialization
        if (EveryplayInstance == null)
        {
            Debug.Log("Unable to initialize Everyplay. Everyplay might be disabled for this platform or the app is closing.");
        }
    }

    public static void ShowSharingModal()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED
            EveryplayShowSharingModal();
            #endif
        }
    }

    public static void PlayLastRecording()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED
            EveryplayPlayLastRecording();
            #endif
        }
    }

    public static void StartRecording()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplayStartRecording();
            #endif
        }
    }

    public static void GetFilepath()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplayGetFilepath();
            #endif
        }
    }

    public static void StopRecording()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplayStopRecording();
            #endif
        }
    }

    public static void PauseRecording()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplayPauseRecording();
            #endif
        }
    }

    public static void ResumeRecording()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplayResumeRecording();
            #endif
        }
    }

    public static bool IsRecording()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            return EveryplayIsRecording();
            #endif
        }
        return false;
    }

    public static bool IsRecordingSupported()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            return EveryplayIsRecordingSupported();
            #endif
        }
        return false;
    }

    public static bool IsPaused()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            return EveryplayIsPaused();
            #endif
        }
        return false;
    }

    [Obsolete("Everyplay HUD-less functionality is no longer maintained and may not function properly.")]
    public static bool SnapshotRenderbuffer()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            return EveryplaySnapshotRenderbuffer();
            #endif
        }
        return false;
    }

    public static bool IsSupported()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            return EveryplayIsSupported();
            #endif
        }
        return false;
    }

    public static bool IsSingleCoreDevice()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            return EveryplayIsSingleCoreDevice();
            #endif
        }
        return false;
    }

    public static int GetUserInterfaceIdiom()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            return EveryplayGetUserInterfaceIdiom();
            #endif
        }
        return 0;
    }

    public static void SetTargetFPS(int fps)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetTargetFPS(fps);
            #endif
        }
    }

    public static void SetMotionFactor(int factor)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetMotionFactor(factor);
            #endif
        }
    }

    public static void SetAudioResamplerQuality(int quality)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if UNITY_ANDROID && !UNITY_EDITOR
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetAudioResamplerQuality(quality);
            #endif
            #endif
        }
    }

    public static void SetMaxRecordingMinutesLength(int minutes)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetMaxRecordingMinutesLength(minutes);
            #endif
        }
    }

    public static void SetMaxRecordingSecondsLength(int seconds)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetMaxRecordingSecondsLength(seconds);
            #endif
        }
    }

    public static void SetLowMemoryDevice(bool state)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetLowMemoryDevice(state);
            #endif
        }
    }

    public static void SetDisableSingleCoreDevices(bool state)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetDisableSingleCoreDevices(state);
            #endif
        }
    }

    private static Texture2D currentThumbnailTargetTexture = null;
    public static void SetThumbnailTargetTexture(Texture2D texture)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            currentThumbnailTargetTexture = texture;
            #if !UNITY_3_5
            #if EVERYPLAY_IPHONE_ENABLED || EVERYPLAY_OSX_ENABLED
            if (texture != null)
            {
                EveryplaySetThumbnailTargetTexture(currentThumbnailTargetTexture.GetNativeTexturePtr());
                EveryplaySetThumbnailTargetTextureWidth(currentThumbnailTargetTexture.width);
                EveryplaySetThumbnailTargetTextureHeight(currentThumbnailTargetTexture.height);
            }
            else
            {
                EveryplaySetThumbnailTargetTexture(System.IntPtr.Zero);
            }
            #elif EVERYPLAY_ANDROID_ENABLED
            if (texture != null)
            {
                int textureId = currentThumbnailTargetTexture.GetNativeTexturePtr().ToInt32();
                EveryplaySetThumbnailTargetTextureId(textureId);
                EveryplaySetThumbnailTargetTextureWidth(currentThumbnailTargetTexture.width);
                EveryplaySetThumbnailTargetTextureHeight(currentThumbnailTargetTexture.height);
            }
            else
            {
                EveryplaySetThumbnailTargetTextureId(0);
            }
            #endif
            #endif
        }
    }

    [Obsolete("Use SetThumbnailTargetTexture(Texture2D texture) instead.")]
    public static void SetThumbnailTargetTextureId(int textureId)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetThumbnailTargetTextureId(textureId);
            #endif
        }
    }

    [Obsolete("Defining texture width is no longer required when SetThumbnailTargetTexture(Texture2D texture) is used.")]
    public static void SetThumbnailTargetTextureWidth(int textureWidth)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetThumbnailTargetTextureWidth(textureWidth);
            #endif
        }
    }

    [Obsolete("Defining texture height is no longer required when SetThumbnailTargetTexture(Texture2D texture) is used.")]
    public static void SetThumbnailTargetTextureHeight(int textureHeight)
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplaySetThumbnailTargetTextureHeight(textureHeight);
            #endif
        }
    }

    public static void TakeThumbnail()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
            EveryplayTakeThumbnail();
            #endif
        }
    }

    public static bool IsReadyForRecording()
    {
        if (EveryplayInstance != null && hasMethods == true)
        {
            return readyForRecording;
        }
        return false;
    }

    // Private static methods

    private static void RemoveAllEventHandlers()
    {
        WasClosed = null;
        ReadyForRecording = null;
        RecordingStarted = null;
        RecordingStopped = null;
        ThumbnailTextureReady = null;
        FileReady = null;
    }

    private static void Reset()
    {
        #if EVERYPLAY_RESET_BINDINGS_ENABLED
        try
        {
            if (seenInitialization)
            {
                ResetEveryplay();
            }
        }
        catch (DllNotFoundException)
        {
        }
        catch (EntryPointNotFoundException)
        {
        }
        #endif
    }

    #if UNITY_ANDROID || UNITY_IPHONE || UNITY_IOS
    private static void AddTestButtons(GameObject gameObject)
    {
        Texture2D textureAtlas = (Texture2D) Resources.Load("everyplay-test-buttons", typeof(Texture2D));
        if (textureAtlas != null)
        {
            EveryplayRecButtons recButtons = gameObject.AddComponent<EveryplayRecButtons>();
            if (recButtons != null)
            {
                recButtons.atlasTexture = textureAtlas;
            }
        }
    }

    #endif

    // Private instance methods

    #if UNITY_EDITOR && !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1)
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        if (everyplayInstance != null)
        {
            everyplayInstance.OnApplicationQuit();
        }
        else
        {
            Reset();
        }
    }

    #endif

    // Monobehaviour methods

    void OnApplicationQuit()
    {
        Reset();

        if (currentThumbnailTargetTexture != null)
        {
            SetThumbnailTargetTexture(null);
            currentThumbnailTargetTexture = null;
        }
        RemoveAllEventHandlers();
        appIsClosing = true;
        everyplayInstance = null;
    }

    // Private instance methods called by native

    private void EveryplayHidden(string msg)
    {
        if (WasClosed != null)
        {
            WasClosed();
        }
    }

    private void EveryplayReadyForRecording(string jsonMsg)
    {
        Dictionary<string, object> dict = EveryplayDictionaryExtensions.JsonToDictionary(jsonMsg);
        bool enabled;

        if (EveryplayDictionaryExtensions.TryGetValue(dict, "enabled", out enabled))
        {
            readyForRecording = enabled;

            if (ReadyForRecording != null)
            {
                ReadyForRecording(enabled);
            }
        }
    }

    private void EveryplayFileReady(string jsonMsg)
    {
        Dictionary<string, object> dict = EveryplayDictionaryExtensions.JsonToDictionary(jsonMsg);
        string videoURL;

        if (EveryplayDictionaryExtensions.TryGetValue(dict, "videoURL", out videoURL))
        {
            if (FileReady != null)
            {
                FileReady(videoURL);
            }
        }
    }

    private void EveryplayRecordingStarted(string msg)
    {
        if (RecordingStarted != null)
        {
            RecordingStarted();
        }
    }

    private void EveryplayRecordingStopped(string msg)
    {
        if (RecordingStopped != null)
        {
            RecordingStopped();
        }
    }

    private void EveryplayThumbnailTextureReady(string jsonMsg)
    {
        #if !UNITY_3_5
        if (ThumbnailTextureReady != null)
        {
            Dictionary<string, object> dict = EveryplayDictionaryExtensions.JsonToDictionary(jsonMsg);
            long texturePtr;
            bool portrait;

            if (currentThumbnailTargetTexture != null && EveryplayDictionaryExtensions.TryGetValue(dict, "texturePtr", out texturePtr) && EveryplayDictionaryExtensions.TryGetValue(dict, "portrait", out portrait))
            {
                long currentPtr = (long) currentThumbnailTargetTexture.GetNativeTexturePtr();
                if (currentPtr == texturePtr)
                {
                    ThumbnailTextureReady(currentThumbnailTargetTexture, portrait);
                }
            }
        }
        #endif
    }

    // Native calls

    #if EVERYPLAY_IPHONE_ENABLED || EVERYPLAY_TVOS_ENABLED || EVERYPLAY_OSX_ENABLED

    #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
    [DllImport(nativeMethodSource)]
    private static extern void InitEveryplay(string gameObjectName);
    #endif

    #if EVERYPLAY_BINDINGS_ENABLED

    [DllImport(nativeMethodSource)]
    private static extern void EveryplayShowSharingModal();

    [DllImport(nativeMethodSource)]
    private static extern void EveryplayPlayLastRecording();
    #endif

    #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
    [DllImport(nativeMethodSource)]
    private static extern void EveryplayStartRecording();

    [DllImport(nativeMethodSource)]
    private static extern void EveryplayStopRecording();

    [DllImport(nativeMethodSource)]
    private static extern void EveryplayGetFilepath();

    [DllImport(nativeMethodSource)]
    private static extern void EveryplayPauseRecording();

    [DllImport(nativeMethodSource)]
    private static extern void EveryplayResumeRecording();

    [DllImport(nativeMethodSource)]
    private static extern bool EveryplayIsRecording();

    [DllImport(nativeMethodSource)]
    private static extern bool EveryplayIsRecordingSupported();

    [DllImport(nativeMethodSource)]
    private static extern bool EveryplayIsPaused();

    [DllImport(nativeMethodSource)]
    private static extern bool EveryplaySnapshotRenderbuffer();

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetTargetFPS(int fps);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetMotionFactor(int factor);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetMaxRecordingMinutesLength(int minutes);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetMaxRecordingSecondsLength(int seconds);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetLowMemoryDevice(bool state);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetDisableSingleCoreDevices(bool state);

    [DllImport(nativeMethodSource)]
    private static extern bool EveryplayIsSupported();

    [DllImport(nativeMethodSource)]
    private static extern bool EveryplayIsSingleCoreDevice();

    [DllImport(nativeMethodSource)]
    private static extern int EveryplayGetUserInterfaceIdiom();
    #endif

    #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetThumbnailTargetTexture(System.IntPtr texturePtr);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetThumbnailTargetTextureId(int textureId);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetThumbnailTargetTextureWidth(int textureWidth);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplaySetThumbnailTargetTextureHeight(int textureHeight);

    [DllImport(nativeMethodSource)]
    private static extern void EveryplayTakeThumbnail();
    #endif

    #if EVERYPLAY_RESET_BINDINGS_ENABLED
    [DllImport(nativeMethodSource)]
    private static extern void ResetEveryplay();
    #endif

    #elif EVERYPLAY_ANDROID_ENABLED

    private static AndroidJavaObject everyplayUnity;

    #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
    private static void InitEveryplay(string gameObjectName)
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = jc.GetStatic<AndroidJavaObject>("currentActivity");
        everyplayUnity = new AndroidJavaObject("com.everyplay.Everyplay.unity.EveryplayUnity3DWrapper");
        everyplayUnity.Call("initEveryplay", activity, gameObjectName);
    }

    #endif

    #if EVERYPLAY_BINDINGS_ENABLED

    private static void EveryplayShowSharingModal()
    {
        everyplayUnity.Call("showSharingModal");
    }

    private static void EveryplayPlayLastRecording()
    {
        everyplayUnity.Call("playLastRecording");
    }

    #endif

    #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
    private static void EveryplayStartRecording()
    {
        everyplayUnity.Call("startRecording");
    }

    private static void EveryplayGetFilepath()
    {
        everyplayUnity.Call("getFilePath");
    }

    private static void EveryplayStopRecording()
    {
        everyplayUnity.Call("stopRecording");
    }

    private static void EveryplayPauseRecording()
    {
        everyplayUnity.Call("pauseRecording");
    }

    private static void EveryplayResumeRecording()
    {
        everyplayUnity.Call("resumeRecording");
    }

    private static bool EveryplayIsRecording()
    {
        return everyplayUnity.Call<bool>("isRecording");
    }

    private static bool EveryplayIsRecordingSupported()
    {
        return everyplayUnity.Call<bool>("isRecordingSupported");
    }

    private static bool EveryplayIsPaused()
    {
        return everyplayUnity.Call<bool>("isPaused");
    }

    private static bool EveryplaySnapshotRenderbuffer()
    {
        return everyplayUnity.Call<bool>("snapshotRenderbuffer");
    }

    /*
    private static void EveryplaySetMetadata(string json)
    {
        everyplayUnity.Call("setMetadata", json);
    }
    */
    private static void EveryplaySetTargetFPS(int fps)
    {
        everyplayUnity.Call("setTargetFPS", fps);
    }

    private static void EveryplaySetMotionFactor(int factor)
    {
        everyplayUnity.Call("setMotionFactor", factor);
    }

    private static void EveryplaySetAudioResamplerQuality(int quality)
    {
        everyplayUnity.Call("setAudioResamplerQuality", quality);
    }

    private static void EveryplaySetMaxRecordingMinutesLength(int minutes)
    {
        everyplayUnity.Call("setMaxRecordingMinutesLength", minutes);
    }

    private static void EveryplaySetMaxRecordingSecondsLength(int seconds)
    {
        everyplayUnity.Call("setMaxRecordingSecondsLength", seconds);
    }

    private static void EveryplaySetLowMemoryDevice(bool state)
    {
        everyplayUnity.Call("setLowMemoryDevice", state ? 1 : 0);
    }

    private static void EveryplaySetDisableSingleCoreDevices(bool state)
    {
        everyplayUnity.Call("setDisableSingleCoreDevices", state ? 1 : 0);
    }

    private static bool EveryplayIsSupported()
    {
        return everyplayUnity.Call<bool>("isSupported");
    }

    private static bool EveryplayIsSingleCoreDevice()
    {
        return everyplayUnity.Call<bool>("isSingleCoreDevice");
    }

    private static int EveryplayGetUserInterfaceIdiom()
    {
        return everyplayUnity.Call<int>("getUserInterfaceIdiom");
    }

    #endif

    #if EVERYPLAY_BINDINGS_ENABLED || EVERYPLAY_CORE_BINDINGS_ENABLED
    private static void EveryplaySetThumbnailTargetTextureId(int textureId)
    {
        everyplayUnity.Call("setThumbnailTargetTextureId", textureId);
    }

    private static void EveryplaySetThumbnailTargetTextureWidth(int textureWidth)
    {
        everyplayUnity.Call("setThumbnailTargetTextureWidth", textureWidth);
    }

    private static void EveryplaySetThumbnailTargetTextureHeight(int textureHeight)
    {
        everyplayUnity.Call("setThumbnailTargetTextureHeight", textureHeight);
    }

    private static void EveryplayTakeThumbnail()
    {
        everyplayUnity.Call("takeThumbnail");
    }

    #endif

    #endif
}

#if UNITY_EDITOR
[InitializeOnLoad]
public class EveryplayEditorRecording
{
    [Obsolete]
    static EveryplayEditorRecording()
    {
        EditorApplication.playmodeStateChanged = OnUnityPlayModeChanged;
    }

    private static void OnUnityPlayModeChanged()
    {
        if (EditorApplication.isPaused == true)
        {
            Everyplay.PauseRecording();
        }
        else if (EditorApplication.isPlaying == true)
        {
            Everyplay.ResumeRecording();
        }
    }
}
#endif

#if UNITY_EDITOR || UNITY_STANDALONE_OSX
public class EveryplaySendMessageDispatcher
{
    public static void Dispatch(string name, string method, string message)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null)
        {
            obj.SendMessage(method, message);
        }
    }
}
#endif
