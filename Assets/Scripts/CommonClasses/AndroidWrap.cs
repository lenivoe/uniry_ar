using UnityEngine;

namespace AndroidWrap {
    public static class Cls {
        public static readonly AndroidJavaClass UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        public static readonly AndroidJavaClass MediaStore = new AndroidJavaClass("android.provider.MediaStore$Images$Media");
        public static readonly AndroidJavaClass BmpFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
        public static readonly AndroidJavaClass Toast = new AndroidJavaClass("android.widget.Toast");
    }
    
    public static class Obj {
        public static AndroidJavaObject CreateIntent(AndroidJavaObject context, AndroidJavaClass cls) {
            return new AndroidJavaObject("android.content.Intent", context, cls);
        }

        public static AndroidJavaObject UnityActivity = Cls.UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        public static AndroidJavaObject Context = UnityActivity.Call<AndroidJavaObject>("getApplicationContext");
        public static AndroidJavaObject ContentResolver = UnityActivity.Call<AndroidJavaObject>("getContentResolver");
        public static AndroidJavaObject AudioManager = UnityActivity.Call<AndroidJavaObject>("getSystemService", "audio");
    }
}

