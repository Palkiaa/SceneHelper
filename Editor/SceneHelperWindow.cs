using System;
using System.Collections.Generic;
using System.Linq;
using Helpers.ChangeTree;
using UnityEditor;
using UnityEditor.PackageManager;
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
        EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
        EditorSceneManager.sceneClosed += EditorSceneManager_sceneClosed;

        SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        SceneManager.sceneUnloaded += EditorSceneManager_sceneUnloaded;

        EditorBuildSettings.sceneListChanged += EditorBuildSettings_sceneListChanged;

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
        EditorSceneManager.sceneOpened -= EditorSceneManager_sceneOpened;
        EditorSceneManager.sceneClosed -= EditorSceneManager_sceneClosed;

        SceneManager.activeSceneChanged -= SceneManager_activeSceneChanged;
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        SceneManager.sceneUnloaded -= EditorSceneManager_sceneUnloaded;

        EditorBuildSettings.sceneListChanged -= EditorBuildSettings_sceneListChanged;
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

    private void EditorBuildSettings_sceneListChanged()
    {
        RefreshScenes();
    }

    private void LoadScenes()
    {
        //Loading scene info and its state
        var scenes = GetScenes().ToList();

        //Filter out scenes that can't be opened
        for (int i = 0; i < scenes.Count; i++)
        {
            var scene = scenes[i];

            var info = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(scene.AssetPath);
            if (info == null) //Doesn't exist inside a package, belongs to the project
                continue;

            if (info.source is not (PackageSource.Embedded or PackageSource.Local))
            {
                scenes.RemoveAt(i);
                i--;
            }
        }

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
            var buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneAssetPath);

            scenes.Add(new SceneMetaInfo
            {
                AssetId = sceneId,
                AssetPath = sceneAssetPath,
                Name = sceneAsset.name,
                BuildIndex = buildIndex != -1 ? buildIndex : null,
                IsDefaultPlayScene = EditorSceneManager.playModeStartScene == sceneAsset,
                OpenSceneMode = null
            });
        }

        var activeScene = SceneManager.GetActiveScene();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            var sceneMetaInfo = scenes.SingleOrDefault(s => s == scene);
            if (sceneMetaInfo == null)
                continue;

            if (scene.isLoaded)
                sceneMetaInfo.OpenSceneMode = sceneMetaInfo == activeScene ? OpenSceneMode.Single : OpenSceneMode.Additive;
            else
                sceneMetaInfo.OpenSceneMode = OpenSceneMode.AdditiveWithoutLoading;
        }

        return scenes;
    }

    private void SceneManager_activeSceneChanged(Scene oldScene, Scene newScene)
    {
        foreach (var sceneMetaInfo in Scenes)
        {
            if (sceneMetaInfo == newScene)
                sceneMetaInfo.OpenSceneMode = OpenSceneMode.Single;
            else
                sceneMetaInfo.OpenSceneMode = null;
        }

        RefreshScenes();
    }

    private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        var sceneMetaInfo = Scenes.SingleOrDefault(s => s == scene);
        if (sceneMetaInfo == null)
            return;

        if (loadSceneMode == LoadSceneMode.Single)
            sceneMetaInfo.OpenSceneMode = OpenSceneMode.Single;
        else if (loadSceneMode == LoadSceneMode.Additive)
            sceneMetaInfo.OpenSceneMode = OpenSceneMode.Additive;

        RefreshScenes();
    }

    private void EditorSceneManager_sceneOpened(Scene scene, OpenSceneMode mode)
    {
        var sceneMetaInfo = Scenes.SingleOrDefault(s => s == scene);
        if (sceneMetaInfo == null)
            return;

        sceneMetaInfo.OpenSceneMode = mode;

        RefreshScenes();
    }

    private void EditorSceneManager_sceneClosed(Scene scene)
    {
        var sceneMetaInfo = Scenes.SingleOrDefault(s => s == scene);
        if (sceneMetaInfo == null)
            return;

        sceneMetaInfo.OpenSceneMode = null;

        RefreshScenes();
    }

    /// <summary>
    /// React to the play mode state changing. When in play mode, disable collab.
    /// </summary>
    /// <param name="state">Editor play mode state.</param>
    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
    }

    /// <summary>
    /// Save state before domain reload.
    /// </summary>
    private void OnBeforeAssemblyReload()
    {
    }

    /// <summary>
    /// Restore window state after assembly reload.
    /// </summary>
    private void OnAfterAssemblyReload()
    {
        OnEnableGUI();
        RefreshScenes();
    }
}