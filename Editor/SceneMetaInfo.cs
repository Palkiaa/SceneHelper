using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SceneHelper.Editor
{
    public class SceneMetaInfo
    {
        /// <summary>
        /// This is actually a UnityEditor.GUID
        /// </summary>
        public string AssetId;

        public GUID? Id
        {
            get
            {
                if (GUID.TryParse(AssetId, out var guid))
                    return guid;

                return null;
            }
        }

        public string AssetPath;

        public string Name;

        public int? BuildIndex;

        public bool IsDefaultPlayScene;
        public bool IsLoaded => OpenSceneMode != null;

        public OpenSceneMode? OpenSceneMode;

        public static bool operator ==(SceneMetaInfo sceneMeta, Scene scene)
        {
            return sceneMeta.AssetPath == scene.path;
        }

        public static bool operator !=(SceneMetaInfo sceneMeta, Scene scene)
        {
            return sceneMeta.AssetPath != scene.path;
        }

        public static bool operator ==(Scene scene, SceneMetaInfo sceneMeta) => sceneMeta == scene;

        public static bool operator !=(Scene scene, SceneMetaInfo sceneMeta) => sceneMeta != scene;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}