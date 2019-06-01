#if !(UNITY_3_5 || UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7)
#define UNITY_5_OR_LATER
#endif

using UnityEngine;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EveryplaySettings : ScriptableObject
{
    public bool iosSupportEnabled;
    public bool tvosSupportEnabled;
    public bool androidSupportEnabled;
    public bool standaloneSupportEnabled;

    public bool testButtonsEnabled;
    public bool earlyInitializerEnabled = true;

    public bool IsEnabled
    {
        get
        {
            #if UNITY_5_OR_LATER && (UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX)
            return standaloneSupportEnabled;
            #elif UNITY_IPHONE || UNITY_IOS
            return iosSupportEnabled;
            #elif UNITY_TVOS
            return tvosSupportEnabled;
            #elif UNITY_ANDROID
            return androidSupportEnabled;
            #else
            return false;
            #endif
        }
    }

#if UNITY_EDITOR
    public bool IsBuildTargetEnabled
    {
        get
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel ||
#if !UNITY_3_5
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXIntel64 ||
#endif
                EditorUserBuildSettings.activeBuildTarget == BuildTarget.StandaloneOSXUniversal)
            {
                return standaloneSupportEnabled;
            }
#if UNITY_5_OR_LATER
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                return iosSupportEnabled;
            }
#if UNITY_TVOS
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.tvOS)
            {
                return tvosSupportEnabled;
            }
#endif
#else
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.iPhone)
            {
                return iosSupportEnabled;
            }
#endif
            else if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                return androidSupportEnabled;
            }

            return false;
        }
    }
#endif

    public bool IsValid
    {
        get
        {
            return true;
        }
    }
}
