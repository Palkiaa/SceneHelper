﻿using UnityEditor;
using UnityEngine;

namespace SceneHelper.Editor
{
    public static class Tools
    {
        [MenuItem("Tools/Scene Helper", priority = -2000)]
        internal static void OpenSceneHelper()
        {
            SceneHelperWindow.Init();
        }

#if SceneHelper_Dev
        [MenuItem("Tools/Force close Scene Helper", priority = -2000)]
        internal static void CloseSceneHelper()
        {
            var window = EditorWindow.GetWindow<SceneHelperWindow>();

            if (window != null)
                window.Close();
        }
#endif
    }
}