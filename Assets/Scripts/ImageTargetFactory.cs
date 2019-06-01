using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageTargetFactory : MonoBehaviour {
    [System.Serializable]
    class VideoTargetsInfo {
        [System.Serializable]
        public class VideoTargetInfo {
            [System.Serializable]
            public class TargetInfo {
                public string Path { get { return path; } }
                public Vector2 Size { get { return size == null ? Vector2.one : new Vector2(size[0], size[1]); } }

                public string path = null;
                public float[] size = null;
            }
            [System.Serializable]
            public class VideoInfo {
                public string Path { get { return path; } }
                public ImageTargetBehaviour_YandexVideo.VideoScaleType ScaleType {
                    get {
                        return (ImageTargetBehaviour_YandexVideo.VideoScaleType)System.Enum.Parse(
                            typeof(ImageTargetBehaviour_YandexVideo.VideoScaleType), scale_type, true);
                    }
                }

                public string path = null;
                public string scale_type = null;
            }
            
            public TargetInfo Target { get { return target; } }
            public VideoInfo Video { get { return video; } }

            public TargetInfo target = null;
            public VideoInfo video = null;
        }

        public List<VideoTargetInfo> videosInfo = null;
    }

    void Awake() {
        var json = Resources.Load<TextAsset>("ar_objects");
        var vinfos = JsonUtility.FromJson<VideoTargetsInfo>(json.text);
        
        foreach(var v in vinfos.videosInfo) {
            var yavideo = new GameObject().AddComponent<ImageTargetBehaviour_YandexVideo>();
            yavideo.name = "target_yavideo_" + v.Target.Path;
            yavideo.transform.parent = transform;
            yavideo.Path = v.Target.Path;
            yavideo.Size = v.Target.Size;
            yavideo.videoScaleType = v.Video.ScaleType;
            yavideo.yandexLink = v.Video.Path;
        }
    }
}