using System;
using System.Collections.Generic;
using System.Linq;

using GitCollab.Helpers.ChangeTree;

using UnityEditor;
using UnityEditor.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

public partial class SceneHelperWindow : EditorWindow
{
    public static void Init()
    {
        var w = CreateWindow<SceneHelperWindow>("Scene Helper");
        w.ShowUtility();
        w.minSize = new Vector2(320, 450);
    }

    public static void RefreshScenes()
    {
        Refresh?.Invoke();
    }

    private static event Action Refresh;

    private readonly List<SceneMetaInfo> Scenes = new List<SceneMetaInfo>();
    private readonly List<TreeItem<SceneMetaInfo>> SceneTree = new List<TreeItem<SceneMetaInfo>>();

    private bool IsDirty;

    private void OnEnable()
    {
        OnEnableGUI();

        //https://docs.unity3d.com/ScriptReference/AssetPostprocessor.html

        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        //https://docs.unity3d.com/ScriptReference/SceneManagement.EditorSceneManager.html
        //https://docs.unity3d.com/ScriptReference/SceneManagement.SceneManager.html

        EditorSceneManager.activeSceneChangedInEditMode += SceneManager_activeSceneChanged;
        SceneManager.sceneUnloaded += EditorSceneManager_sceneUnloaded;
        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
        EditorSceneManager.sceneClosed += EditorSceneManager_sceneClosed;

        Refresh += SceneHelperWindow_Refresh;
        LoadScenes();
    }

    private void OnDisable()
    {
        Refresh -= SceneHelperWindow_Refresh;

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
        AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

        EditorSceneManager.activeSceneChangedInEditMode -= SceneManager_activeSceneChanged;
        SceneManager.sceneUnloaded -= EditorSceneManager_sceneUnloaded;
        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        EditorSceneManager.sceneOpened -= EditorSceneManager_sceneOpened;
        EditorSceneManager.sceneClosed -= EditorSceneManager_sceneClosed;
    }

    private void SceneHelperWindow_Refresh()
    {
        //LoadScenes();
        IsDirty = true;
        Repaint();
    }

    private void EditorSceneManager_sceneUnloaded(Scene scene)
    {
        foreach (var sceneMetaInfo in Scenes)
        {
            if (sceneMetaInfo.Name == scene.name)
            {
                sceneMetaInfo.OpenSceneMode = null;
            }
        }
    }

    private void LoadScenes()
    {
        //Loading basic scene info
        var scenes = GetScenes();

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            var matchingScene = scenes.FirstOrDefault(s => s.Name == scene.name);
            if (matchingScene != null)
            {
                if (scene.isLoaded)
                {
                    if (!scene.isSubScene)
                    {
                        matchingScene.OpenSceneMode = OpenSceneMode.Single;
                    }
                    else
                    {
                        matchingScene.OpenSceneMode = OpenSceneMode.Additive;
                    }
                }
            }
        }

        foreach (var scene in scenes)
        {
            var match = Scenes.FirstOrDefault(s => s.AssetId == scene.AssetId);
            if (match != null && match.OpenSceneMode.HasValue && !scene.OpenSceneMode.HasValue)
            {
                scene.OpenSceneMode = match.OpenSceneMode;
            }
        }

        //var activeScene = SceneManager.GetActiveScene();
        //var matchingScene = scenes.FirstOrDefault(s => activeScene.name == s.Name);
        //if (matchingScene != null)
        //{
        //    matchingScene.OpenSceneMode = null;// = true;
        //}

        //foreach (var scene in Scenes)
        //{
        //    var sceneAsset = EditorSceneManager.GetSceneByPath(scene.AssetPath);//.isSubScene
        //    Debug.Log($"{sceneAsset.name} - {sceneAsset.isLoaded} : {sceneAsset.isSubScene}");
        //    //scene.OpenSceneMode =
        //    //sceneAsset.isLoaded
        //}

        //Setting up scene tree
        var pathSeperator = '/';

        var treeCreator = new TreeCreator<SceneMetaInfo>();
        var result = treeCreator.Handle(scenes,
                (entry, level) =>
                {
                    return entry.AssetPath.Split(pathSeperator)[level - 1];
                },
                (entry) =>
                {
                    return entry.AssetPath.Split(pathSeperator).Count();
                });

        Scenes.Clear();
        Scenes.AddRange(scenes);

        SceneTree.Clear();
        SceneTree.AddRange(result);
    }

    private IEnumerable<SceneMetaInfo> GetScenes()
    {
        var scenes = new List<SceneMetaInfo>();

        var sceneIds = AssetDatabase.FindAssets("t:scene");
        foreach (var sceneId in sceneIds)
        {
            var sceneAssetPath = AssetDatabase.GUIDToAssetPath(sceneId);
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(sceneAssetPath);

            //var sceneName = assetPath;
            //var endOfPath = sceneName.LastIndexOf('/');
            //if (0 <= endOfPath)
            //    sceneName = sceneName.Substring(endOfPath + 1);
            //
            //var fileType = sceneName.LastIndexOf('.');
            //if (0 <= fileType)
            //    sceneName = sceneName.Substring(0, fileType);
            var buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneAssetPath);

            scenes.Add(new SceneMetaInfo
            {
                AssetId = sceneId,
                AssetPath = sceneAssetPath,
                Name = sceneAsset.name,
                BuildIndex = buildIndex != -1 ? buildIndex : (int?)null,
                IsDefaultPlayScene = EditorSceneManager.playModeStartScene == sceneAsset,
                OpenSceneMode = null
            });
        }

        return scenes;
    }

    private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
    {
        foreach (var scene in Scenes)
        {
            if (newScene.name == scene.Name)
            {
                scene.OpenSceneMode = OpenSceneMode.Single;
            }
            else
            {
                scene.OpenSceneMode = null;
            }
        }
        Repaint();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        foreach (var sceneMetaInfo in Scenes)
        {
            if (sceneMetaInfo.Name == scene.name)
            {
                if (loadSceneMode == LoadSceneMode.Single)
                {
                    sceneMetaInfo.OpenSceneMode = OpenSceneMode.Single;
                }
                else if (loadSceneMode == LoadSceneMode.Additive)
                {
                    sceneMetaInfo.OpenSceneMode = OpenSceneMode.Additive;
                }
            }
        }
        Repaint();
    }

    private void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
    {
        foreach (var sceneMetaInfo in Scenes)
        {
            if (sceneMetaInfo.Name == scene.name)
            {
                sceneMetaInfo.OpenSceneMode = mode;
            }
        }
        Repaint();
    }



    private void EditorSceneManager_sceneClosed(Scene scene)
    {
        foreach (var sceneMetaInfo in Scenes)
        {
            if (sceneMetaInfo.Name == scene.name)
            {
                sceneMetaInfo.OpenSceneMode = null;
            }
        }
        Repaint();
    }

    /// <summary>
    /// React to the play mode state changing. When in play mode, disable collab.
    /// </summary>
    /// <param name="state">Editor play mode state.</param>
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
    }

    /// <summary>
    /// Restore window state after assembly reload.
    /// </summary>
    private void OnAfterAssemblyReload()
    {
    }

    /// <summary>
    /// Save state before domain reload.
    /// </summary>
    private void OnBeforeAssemblyReload()
    {
    }
}