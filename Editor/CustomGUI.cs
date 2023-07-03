using UnityEditor;

using UnityEngine;

public class CustomGUI
{
    public readonly Color Scene_OpenedColor = Color.green;
    public readonly Color Scene_AdditiveColor = Color.yellow;
    public readonly Color Scene_AdditiveNotLoadedColor = Color.grey;

    public GUIContent InactiveIcon { get; private set; }
    public GUIContent ActiveIcon { get; private set; }
    public GUIContent PassiveIcon { get; private set; }
    public GUIContent UnloadedIcon { get; private set; }

    public readonly GUIContent GoToFileIcon = new GUIContent("\u25CB", "Go to file");

    public readonly GUIContent SceneOptionsButtonContent = new GUIContent("\u2261", "Options...");  // \u2261 \u20AA

    public readonly GUIContent ScenePlayButtonContent = new GUIContent("\u25BA", "Play directly");
    public readonly GUIStyle ScenePlayButtonStyle;

    public readonly GUIStyle SceneLoadedButtonStyle;
    public readonly GUIContent SceneDefaultButtonAddContent = new GUIContent("=", "Set As Default");
    public readonly GUIContent SceneUnsetDefaultButtonAddContent = new GUIContent("x", "Unset default");

    public readonly GUIContent SceneLoadedButtonAddContent = new GUIContent("+", "Load scene additively");
    public readonly GUIContent SceneLoadedButtonActiveContent = new GUIContent("*", "Active scene (cannot unload)");
    public readonly GUIContent SceneLoadedButtonRemoveContent = new GUIContent("-", "Unload scene");

    public readonly GUIStyle ButoonStyle;

    public CustomGUI()
    {
        //GoToFileIcon = EditorGUIUtility.IconContent("pick_uielements");
    }

    public void OnEnableGUI()
    {
        ActiveIcon = EditorGUIUtility.IconContent("lightMeter/greenLight");
        ActiveIcon.tooltip = "The currently active scene";

        PassiveIcon = EditorGUIUtility.IconContent("lightMeter/orangeLight");
        PassiveIcon.tooltip = "Scene that was loaded additively";

        InactiveIcon = EditorGUIUtility.IconContent("lightMeter/lightRim");
        InactiveIcon.tooltip = "Scene additively added, but currently not loaded";

        UnloadedIcon = EditorGUIUtility.IconContent("lightMeter/redLight");
        UnloadedIcon.tooltip = "The previously loaded scene";
    }
}

public static class CustomGUIExtensions
{
    public static GUIStyle SetupColors(this GUIStyle guiStlye, Color color)
    {
        guiStlye.normal.textColor = color;
        guiStlye.hover.textColor = color;
        guiStlye.active.textColor = color;

        return guiStlye;
    }
}