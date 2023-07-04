using System.Linq;
using Helpers.ChangeTree;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneHelperWindow
{
    private CustomGUI _customGUI = new();

    private Vector2 _scrollView;
    private const char _seperator = '/';

    private void OnEnableGUI()
    {
        _customGUI.Initialise();
    }

    private void OnGUI()
    {
        _customGUI.RefreshStyles();

        if (IsDirty)
        {
            LoadScenes();
            IsDirty = false;
        }

        EditorGUI.DrawRect(new Rect(Vector2.zero, position.size), _customGUI.MainBackgroundColor);

        _scrollView = EditorGUILayout.BeginScrollView(_scrollView, GUILayout.Height(position.height));// - (130f)));

        foreach (var item in SceneTree)
        {
            RenderLeaf(item);
        }

        EditorGUILayout.EndScrollView();
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

    private void RenderLeaf(TreeItem<SceneMetaInfo> treeItem, string prefix = null, string parents = null)
    {
        if (!treeItem.Data.Any())
        {
            foreach (var child in treeItem.Children)
            {
                var subParents = parents;
                if (string.IsNullOrWhiteSpace(subParents))
                    subParents = treeItem.Summary;
                else
                    subParents = $"{subParents}/{treeItem.Summary}";

                var leafTitle = BuildTitle(treeItem.Summary, prefix);
                RenderLeaf(child, leafTitle, subParents);
            }
            return;
        }

        GUILayout.Space(8);

        EditorGUILayout.BeginHorizontal(GUI.skin.box);

        //GUILayout.Label(BuildTitle(treeItem.Summary, prefix), EditorStyles.boldLabel);
        var title = BuildTitle(treeItem.Summary, prefix);
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);

        EditorGUILayout.EndHorizontal();

        var labelRect = GUILayoutUtility.GetLastRect();
        if (GUI.Button(labelRect, string.Empty, GUIStyle.none)) //The horizontal section above is now clickable
        {
            var pathToFolder = treeItem.Summary;
            if (!string.IsNullOrWhiteSpace(parents))
                pathToFolder = $"{parents}/{pathToFolder}";

            var obj = AssetDatabase.LoadAssetAtPath(pathToFolder, typeof(Object));
            if (obj != null)
                FocusProjectAsset(obj);
            else
                Debug.LogWarning($"Folder not found: '{pathToFolder}'", this);
        }

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

            var sceneIndexRect = rect;
            sceneIndexRect.width = rect.height;
            sceneIndexRect.x = sceneButtonRect.x + sceneButtonRect.width;
            if (scene.BuildIndex.HasValue)
            {
                GUI.Label(sceneIndexRect, $"{scene.BuildIndex.Value}");
            }
            else if (GUI.Button(sceneIndexRect, "+"))
            {
                if (scene.Id.HasValue)
                {
                    var scenes = EditorBuildSettings.scenes.ToList();
                    scenes.Add(new EditorBuildSettingsScene(scene.Id.Value, true));
                    EditorBuildSettings.scenes = scenes.ToArray();
                }
                else
                {
                    Debug.LogWarning($"Could not parse Asset ID for '{scene.AssetPath}' ({scene.AssetId})", this);
                }
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
                        GUI.Label(iconRect, _customGUI.ActiveIcon);
                        style.SetupColors(_customGUI.Scene_OpenedColor);

                        style.fontStyle = FontStyle.Bold;
                        break;

                    case OpenSceneMode.Additive:
                        GUI.Label(iconRect, _customGUI.PassiveIcon);
                        style.SetupColors(_customGUI.Scene_AdditiveColor);

                        style.fontStyle = FontStyle.Bold;
                        break;

                    case OpenSceneMode.AdditiveWithoutLoading:
                        GUI.Label(iconRect, _customGUI.Scene_InactiveContent);
                        style.SetupColors(_customGUI.Scene_AdditiveNotLoadedColor);

                        style.fontStyle = FontStyle.Italic;
                        break;
                }
            }

            //If we're busy playing, only scenes that have been added to the build can be opened
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

            var addSceneRect = rect;
            addSceneRect.width = addSceneRect.height * 1f;
            addSceneRect.x = rect.x + rect.width - (addSceneRect.width);

            var loadSceneRect = addSceneRect;
            loadSceneRect.x -= addSceneRect.width;

            var setDefaultButton = loadSceneRect;
            //setDefaultButton.width = setDefaultButton.height * 1.2f;
            setDefaultButton.x -= setDefaultButton.width;

            if (GUI.Button(setDefaultButton, _customGUI.TargetIcon, _customGUI.ImageButton))
            {
                var sceneObject = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.AssetPath);
                FocusProjectAsset(sceneObject);
            }

            if (scene.OpenSceneMode.HasValue)
            {
                if (scene.OpenSceneMode == OpenSceneMode.Single)
                {
                    var originalVal = GUI.enabled;
                    GUI.enabled = false;
                    GUI.Button(addSceneRect, _customGUI.Scene_ActiveContent, _customGUI.ImageButton);
                    GUI.enabled = originalVal;
                }
                else if (GUI.Button(addSceneRect, _customGUI.Scene_UnloadContent, _customGUI.ImageButton))
                {
                    EditorSceneManager.CloseScene(SceneManager.GetSceneByPath(scene.AssetPath), true);
                }
            }
            else if (GUI.Button(addSceneRect, _customGUI.Scene_LoadAdditivelyContent, _customGUI.ImageButton))
            {
                EditorSceneManager.OpenScene(scene.AssetPath, OpenSceneMode.Additive);
            }

            if (scene.IsDefaultPlayScene)
            {
                using (_ = new GUIColor(_customGUI.Active))
                {
                    if (GUI.Button(loadSceneRect, _customGUI.Scene_UnsetDefaultContent, _customGUI.ImageButton))
                    {
                        EditorSceneManager.playModeStartScene = null;
                        RefreshScenes();
                    }
                }
            }
            else
            {
                using (_ = new GUIColor(_customGUI.Inactive))
                {
                    if (GUI.Button(loadSceneRect, _customGUI.Scene_SetDefaultContent, _customGUI.ImageButton))
                    {
                        SceneAsset myWantedStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.AssetPath);
                        EditorSceneManager.playModeStartScene = myWantedStartScene;
                        RefreshScenes();
                    }
                }
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
            var subParents = parents;
            if (string.IsNullOrWhiteSpace(subParents))
                subParents = treeItem.Summary;
            else
                subParents = $"{subParents}/{treeItem.Summary}";

            RenderLeaf(child, null, subParents);
        }
        EditorGUI.indentLevel--;
    }

    private void FocusProjectAsset(Object @object)
    {
        EditorUtility.FocusProjectWindow();
        // Select the object in the project folder
        Selection.activeObject = @object;
        // Also flash the folder yellow to highlight it
        EditorGUIUtility.PingObject(@object);
    }
}