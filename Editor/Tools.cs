using UnityEditor;

namespace SceneHelper.Editor
{
    public static class Tools
    {
        [MenuItem("Tools/Show Scene Helper", priority = -2000)]
        internal static void OpenSceneHelper()
        {
            SceneHelperWindow.Init();
        }
    }
}