using System.IO;
using System.Linq;
using UnityEditor;

namespace SceneHelper.Editor
{
    public class SceneAssetProcessor : AssetModificationProcessor
    {
        private const string _sceneFilter = ".unity";

        protected static void OnWillCreateAsset(string assetName)
        {
            if (assetName.Contains(_sceneFilter))
            {
                SceneHelperWindow.RefreshScenes();
            }
        }

        protected static AssetDeleteResult OnWillDeleteAsset(string sourcePath, RemoveAssetOptions removeAssetOptions)
        {
            if (sourcePath.EndsWith(_sceneFilter))
            {
                SceneHelperWindow.RefreshScenes();
            }

            return AssetDeleteResult.DidNotDelete;
        }

        protected static AssetMoveResult OnWillMoveAsset(string sourcePath, string destinationPath)
        {
            var extension = Path.GetExtension(sourcePath);
            if (!string.IsNullOrWhiteSpace(extension))
            {
                if (sourcePath.EndsWith(_sceneFilter))
                {
                    SceneHelperWindow.RefreshScenes();
                }

                return AssetMoveResult.DidNotMove;
            }

            var assets = AssetDatabase.FindAssets("t:scene", new string[] { sourcePath });
            if (assets.Any())
            {
                SceneHelperWindow.RefreshScenes();
            }

            return AssetMoveResult.DidNotMove;
        }

        protected static string[] OnWillSaveAssets(string[] paths)
        {
            if (paths.Any(s => s.EndsWith(_sceneFilter)))
            {
                SceneHelperWindow.RefreshScenes();
            }

            return paths;
        }
    }
}