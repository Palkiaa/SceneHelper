using System.Linq;

using UnityEditor;

//https://docs.unity3d.com/ScriptReference/AssetModificationProcessor.html
public class SceneAssetProcessor : AssetModificationProcessor
{
    private const string sceneFilter = ".unity";

    private static void OnWillCreateAsset(string assetName)
    {
        if (assetName.Contains(sceneFilter))
        {
            SceneHelperWindow.RefreshScenes();
        }
    }

    private static AssetDeleteResult OnWillDeleteAsset(string sourcePath, RemoveAssetOptions removeAssetOptions)
    {
        if (sourcePath.EndsWith(sceneFilter))
        {
            SceneHelperWindow.RefreshScenes();
        }

        return AssetDeleteResult.DidNotDelete;
    }

    private static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
    {
        if (sourcePath.EndsWith(sceneFilter))
        {
            SceneHelperWindow.RefreshScenes();
        }

        return AssetMoveResult.DidNotMove;
    }

    private static string[] OnWillSaveAssets(string[] paths)
    {
        if (paths.Any(s => s.EndsWith(sceneFilter)))
        {
            SceneHelperWindow.RefreshScenes();
        }

        return paths;
    }
}