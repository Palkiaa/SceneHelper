using System.Linq;

using GitCollab.Helpers.ChangeTree;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneHelperWindow
{
    private Vector2 scrollView;
    private const char seperator = '/';

    private GUIContent InactiveIcon;
    private GUIContent ActiveIcon;
    private GUIContent PassiveIcon;
    private GUIContent UnloadedIcon;

    private GUIContent GoToFileIcon = new GUIContent("\u25CB", "Options...");

    private GUIContent SceneOptionsButtonContent = new GUIContent("\u2261", "Options...");  // \u2261 \u20AA

    private GUIContent ScenePlayButtonContent = new GUIContent("\u25BA", "Play directly");
    private GUIStyle ScenePlayButtonStyle;

    private GUIStyle SceneLoadedButtonStyle;
    private GUIContent SceneDefaultButtonAddContent = new GUIContent("=", "Set As Default");
    private GUIContent SceneUnsetDefaultButtonAddContent = new GUIContent("x", "Unset default");

    private GUIContent SceneLoadedButtonAddContent = new GUIContent("+", "Load scene additively");
    private GUIContent SceneLoadedButtonActiveContent = new GUIContent("*", "Active scene (cannot unload)");
    private GUIContent SceneLoadedButtonRemoveContent = new GUIContent("-", "Unload scene");

    private GUIStyle ButoonStyle;

    private void OnEnableGUI()
    {
        InactiveIcon = EditorGUIUtility.IconContent("lightMeter/lightRim");
        InactiveIcon.tooltip = "Scene is inactive";

        ActiveIcon = EditorGUIUtility.IconContent("lightMeter/greenLight");
        ActiveIcon.tooltip = "The currently focused scene";

        PassiveIcon = EditorGUIUtility.IconContent("lightMeter/orangeLight");
        PassiveIcon.tooltip = "Scene that was loaded additevely";

        UnloadedIcon = EditorGUIUtility.IconContent("lightMeter/redLight");
        UnloadedIcon.tooltip = "The previously loaded scene";

        //GoToFileIcon = EditorGUIUtility.IconContent("pick_uielements");
    }

    private void OnGUI()
    {
        if (IsDirty)
        {
            LoadScenes();
            IsDirty = false;
        }

        //foreach (var scene in Scenes)
        //{
        //    EditorGUILayout.LabelField(scene.Name, scene.AssetId);
        //}

        //GUILayout.Space(15);

        EditorGUILayout.BeginVertical(GUI.skin.box);

        scrollView = EditorGUILayout.BeginScrollView(scrollView, GUILayout.Height(position.height));// - (130f)));

        foreach (var item in SceneTree)
        {
            RenderLeaf(item);
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private string BuildTitle(string text, string prefix = null)
    {
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            return $"{prefix}/{text}";
        }
        return text;
    }

    public static void DrawUIBox(Color borderColor, Color backgroundColor, Rect rect, int width = 2)
    {
        Rect outter = new Rect(rect);
        Rect inner = new Rect(rect.x + width, rect.y + width, rect.width - width * 2, rect.height - width * 2);
        EditorGUI.DrawRect(outter, borderColor);
        EditorGUI.DrawRect(inner, backgroundColor);
    }

    private GUIStyle SetupColors(GUIStyle guiStlye, Color color)
    {
        guiStlye.normal.textColor = color;
        guiStlye.hover.textColor = color;
        guiStlye.active.textColor = color;

        return guiStlye;
    }

    private void RenderLeaf(TreeItem<SceneMetaInfo> treeItem, string prefix = null)
    {
        if (!treeItem.Data.Any())
        {
            foreach (var child in treeItem.Children)
            {
                RenderLeaf(child, BuildTitle(treeItem.Summary, prefix));
            }
            return;
        }

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal(GUI.skin.box);

        //GUILayout.Label(BuildTitle(treeItem.Summary, prefix), EditorStyles.boldLabel);
        EditorGUILayout.LabelField(BuildTitle(treeItem.Summary, prefix), EditorStyles.boldLabel);

        EditorGUILayout.EndHorizontal();

        EditorGUI.indentLevel++;
        foreach (var scene in treeItem.Data)
        {
            var rect = GUILayoutUtility.GetRect(position.width - 20f, EditorGUIUtility.singleLineHeight, GUIStyle.none);

            var offset = EditorGUI.indentLevel * 10f;
            rect.x += offset;
            rect.width -= offset;

            var iconRect = rect;
            iconRect.width = iconRect.height;

            var sceneButtonRect = rect;
            sceneButtonRect.width *= 0.8f;
            //DrawUIBox(Color.white, Color.grey, labelRect, 1);

            sceneButtonRect.x += iconRect.width;
            sceneButtonRect.width -= iconRect.width;

            if (scene.BuildIndex.HasValue)
            {
                var sceneIndexRect = rect;
                sceneIndexRect.width = rect.height;
                sceneIndexRect.x = sceneButtonRect.x + sceneButtonRect.width;

                GUI.Label(sceneIndexRect, $"{scene.BuildIndex.Value}");
            }

            //var buttonSkin = GUI.skin.button;
            var style = new GUIStyle(GUI.skin.button);
            //style.border = buttonSkin.border;
            //style.margin = buttonSkin.margin;
            //style.padding = buttonSkin.padding;
            if (scene.IsLoaded)
            {
                switch (scene.OpenSceneMode)
                {
                    case OpenSceneMode.Single:
                        GUI.Label(iconRect, ActiveIcon);
                        SetupColors(style, Color.green);

                        style.fontStyle = FontStyle.Bold;
                        break;

                    case OpenSceneMode.Additive:
                        GUI.Label(iconRect, PassiveIcon);
                        SetupColors(style, Color.yellow);

                        style.fontStyle = FontStyle.Bold;
                        break;

                    case OpenSceneMode.AdditiveWithoutLoading:
                        GUI.Label(iconRect, InactiveIcon);
                        SetupColors(style, Color.grey);

                        style.fontStyle = FontStyle.Italic;
                        break;
                }
            }

            //GUI.Label(labelRect, scene.Name, style);
            if (EditorApplication.isPlaying && !scene.BuildIndex.HasValue)
            {
                GUI.enabled = false;
            }

            if (GUI.Button(sceneButtonRect, scene.Name, style))
            {
                if (!EditorApplication.isPlaying)
                {
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(scene.AssetPath, OpenSceneMode.Single);
                    }
                }
                else
                {
                    SceneManager.LoadScene(scene.Name, LoadSceneMode.Single);
                }
            }

            var addSceneRect = rect;
            addSceneRect.width = addSceneRect.height * 1.1f;
            addSceneRect.x = rect.x + rect.width - (addSceneRect.width);

            var loadSceneRect = addSceneRect;
            loadSceneRect.x -= addSceneRect.width;

            var setDefaultButton = loadSceneRect;
            //setDefaultButton.width = setDefaultButton.height * 1.2f;
            setDefaultButton.x -= setDefaultButton.width;

            //https://forum.unity.com/threads/check-if-asset-inside-package-is-readonly.900902/
            if (scene.IsDefaultPlayScene)
            {
                if (GUI.Button(loadSceneRect, SceneUnsetDefaultButtonAddContent))
                {
                    EditorSceneManager.playModeStartScene = null;
                }
            }
            else
            {
                if (GUI.Button(loadSceneRect, SceneDefaultButtonAddContent))
                {
                    SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.AssetPath);
                    EditorSceneManager.playModeStartScene = myWantedStartScene;
                    Refresh.Invoke();
                }
            }
            /*if (GUI.Button(loadSceneRect, ScenePlayButtonContent))
            {
                //https://docs.unity3d.com/ScriptReference/EditorApplication.html
                SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.AssetPath);
                if (!EditorApplication.isPlaying && EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.playModeStartScene = myWantedStartScene;
                    EditorApplication.EnterPlaymode();
                }
                else
                {
                    SceneManager.LoadScene(myWantedStartScene.name, LoadSceneMode.Single);
                }

                //EditorSceneManager.playModeStartScene = null;
            }*/

            if (scene.OpenSceneMode.HasValue)
            {
                if (scene.OpenSceneMode == OpenSceneMode.Single)
                {
                    var originalVal = GUI.enabled;
                    GUI.enabled = false;
                    GUI.Button(addSceneRect, SceneLoadedButtonActiveContent);
                    GUI.enabled = originalVal;
                }
                else if (GUI.Button(addSceneRect, SceneLoadedButtonRemoveContent))
                {
                    EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(scene.AssetPath), true);
                }
            }
            else
            {
                if (GUI.Button(addSceneRect, SceneLoadedButtonAddContent))
                {
                    EditorSceneManager.OpenScene(scene.AssetPath, OpenSceneMode.Additive);
                }
            }

            if (GUI.Button(setDefaultButton, GoToFileIcon))
            {
                var sceneObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.AssetPath);
                EditorGUIUtility.PingObject(sceneObject);

                //SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.AssetPath);
                //if (myWantedStartScene != null)
                //{
                //    if (EditorSceneManager.playModeStartScene != myWantedStartScene)
                //    {
                //        EditorSceneManager.playModeStartScene = myWantedStartScene;
                //    }
                //    else
                //    {
                //        EditorSceneManager.playModeStartScene = null;
                //    }
                //}
                //else
                //    Debug.Log($"Could not find Scene: {scene.AssetPath}");
            }

            var seperatorRect = GUILayoutUtility.GetRect(rect.width, 5f, GUIStyle.none);
            seperatorRect.x = rect.x;
            seperatorRect.y += 2;
            seperatorRect.height = 1;
            EditorGUI.DrawRect(seperatorRect, new Color(1, 1, 1, 0.1f));

            GUI.enabled = true;
            //EditorGUILayout.LabelField(string.Empty, GUI.skin.horiz`ontalSlider);
        }
        EditorGUI.indentLevel--;

        EditorGUI.indentLevel++;
        foreach (var child in treeItem.Children)
        {
            RenderLeaf(child);
        }
        EditorGUI.indentLevel--;
    }
}