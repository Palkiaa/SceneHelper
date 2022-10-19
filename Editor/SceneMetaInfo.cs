using UnityEditor.SceneManagement;

public class SceneMetaInfo
{
    /// <summary>
    /// This is actually a GUID
    /// </summary>
    public string AssetId;

    public string AssetPath;

    public string Name;

    public int? BuildIndex;

    public bool IsDefaultPlayScene;
    public bool IsLoaded => OpenSceneMode != null;

    public OpenSceneMode? OpenSceneMode;
}