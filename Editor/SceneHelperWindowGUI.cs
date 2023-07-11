using System.Linq;
using SceneHelper.Editor.ChangeTree;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneHelper.Editor
{
    public partial class SceneHelperWindow
    {
        private readonly CustomGUI _customGUI = new();

        private Vector2 _scrollView;
        private const char _seperator = '/';

        private void OnEnableGUI()
        {
            _customGUI.Initialise();
        }

        private void OnGUI()
        {
            _customGUI.RefreshStyles();

            if (_isDirty)
            {
                LoadScenes();
                _isDirty = false;
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
                var mainRect = GUILayoutUtility.GetRect(position.width - 20f, EditorGUIUtility.singleLineHeight, GUIStyle.none);

                var offset = EditorGUI.indentLevel * 10f;
                mainRect.x += offset;
                mainRect.width -= offset;

                var sceneStateRect = mainRect;
                sceneStateRect.width = sceneStateRect.height;

                var sceneButtonRect = mainRect;
                sceneButtonRect.width *= 0.8f;
                sceneButtonRect.x += sceneStateRect.width;
                sceneButtonRect.width -= sceneStateRect.width;

                var sceneIndexRect = mainRect;
                sceneIndexRect.width = mainRect.height;
                sceneIndexRect.x = sceneButtonRect.x + sceneButtonRect.width;

                var addSceneRect = mainRect;
                addSceneRect.width = addSceneRect.height * 1f;
                addSceneRect.x = mainRect.x + (mainRect.width - addSceneRect.width);

                var loadSceneRect = addSceneRect;
                loadSceneRect.x -= addSceneRect.width;

                var pingSceneRect = loadSceneRect;
                pingSceneRect.x -= pingSceneRect.width;

                var overlap = (sceneButtonRect.x + sceneButtonRect.width + sceneIndexRect.width) - pingSceneRect.x;
                if (0 < overlap)
                {
                    sceneButtonRect.width -= overlap;
                    sceneIndexRect.x -= overlap;
                }

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

                var style = new GUIStyle(GUI.skin.button);
                if (scene.IsLoaded)
                {
                    switch (scene.OpenSceneMode)
                    {
                        case OpenSceneMode.Single:
                            GUI.Label(sceneStateRect, _customGUI.ActiveIcon);
                            style.SetupColors(_customGUI.Scene_OpenedColor);

                            style.fontStyle = FontStyle.Bold;
                            break;

                        case OpenSceneMode.Additive:
                            GUI.Label(sceneStateRect, _customGUI.PassiveIcon);
                            style.SetupColors(_customGUI.Scene_AdditiveColor);

                            style.fontStyle = FontStyle.Bold;
                            break;

                        case OpenSceneMode.AdditiveWithoutLoading:
                            GUI.Label(sceneStateRect, _customGUI.Scene_InactiveContent);
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

                if (GUI.Button(pingSceneRect, _customGUI.TargetIcon, _customGUI.ImageButton))
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

                var seperatorRect = GUILayoutUtility.GetRect(mainRect.width, 5f, GUIStyle.none);
                seperatorRect.x = mainRect.x;
                seperatorRect.y += 2;
                seperatorRect.height = 1;
                EditorGUI.DrawRect(seperatorRect, new Color(1, 1, 1, 0.1f));

                GUI.enabled = true;
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
            Selection.activeObject = @object;
            EditorGUIUtility.PingObject(@object);
        }
    }
}