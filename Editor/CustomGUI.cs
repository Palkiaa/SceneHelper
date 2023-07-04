using UnityEditor;
using UnityEngine;

namespace SceneHelper.Editor
{
    public class CustomGUI
    {
        public readonly Color MainBackgroundColor = new(0.2f, 0.2f, 0.2f);

        public readonly Color Scene_OpenedColor = Color.green;
        public readonly Color Scene_AdditiveColor = Color.yellow;
        public readonly Color Scene_AdditiveNotLoadedColor = Color.grey;

        public readonly Color Active = Color.white;
        public readonly Color Inactive = Color.grey;

        public GUIContent Scene_InactiveContent { get; private set; }
        public GUIContent ActiveIcon { get; private set; }
        public GUIContent PassiveIcon { get; private set; }
        public GUIContent UnloadedIcon { get; private set; }

        public GUIContent TargetIcon { get; private set; }

        public GUIContent SceneOptionsButtonContent = new GUIContent("\u2261", "Options...");  // \u2261 \u20AA
        public GUIContent ScenePlayButtonContent = new GUIContent("\u25BA", "Play directly");

        public GUIContent Scene_SetDefaultContent;
        public GUIContent Scene_UnsetDefaultContent;

        public GUIContent Scene_LoadAdditivelyContent;
        public GUIContent Scene_ActiveContent;
        public GUIContent Scene_UnloadContent;

        public GUIStyle BorderedImageButton { get; private set; }

        public GUIStyle ImageButton { get; private set; }

        private Texture2D backgroundTexture;

        public void Initialise()
        {
            ActiveIcon = new GUIContent(EditorGUIUtility.IconContent("lightMeter/greenLight"))
            {
                tooltip = "The currently active scene"
            };

            PassiveIcon = new GUIContent(EditorGUIUtility.IconContent("lightMeter/orangeLight"))
            {
                tooltip = "Scene that was loaded additively"
            };

            Scene_InactiveContent = new GUIContent(EditorGUIUtility.IconContent("lightMeter/lightRim"))
            {
                tooltip = "Scene additively added, but currently not loaded"
            };

            UnloadedIcon = new GUIContent(EditorGUIUtility.IconContent("lightMeter/redLight"))
            {
                tooltip = "The previously loaded scene"
            };

            TargetIcon = new GUIContent(EditorGUIUtility.IconContent("d_pick"))
            {
                tooltip = "Go to file"
            };

            Scene_LoadAdditivelyContent = new GUIContent(EditorGUIUtility.IconContent("Toolbar Plus"))
            {
                tooltip = "Load scene additively"
            };

            Scene_ActiveContent = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"))
            {
                tooltip = "Active scene (cannot unload)"
            };

            Scene_UnloadContent = new GUIContent(EditorGUIUtility.IconContent("Toolbar Minus"))
            {
                tooltip = "Unload scene"
            };

            Scene_SetDefaultContent = new GUIContent(EditorGUIUtility.IconContent("d_Scene"))
            {
                tooltip = "Set as default"
            };

            Scene_UnsetDefaultContent = new GUIContent(EditorGUIUtility.IconContent("d_Scene"))
            {
                tooltip = "Unset default"
            };

            ImageButton = new GUIStyle()
            {
                alignment = TextAnchor.MiddleCenter
            };
        }

        public void RefreshStyles()
        {
        }
    }
}