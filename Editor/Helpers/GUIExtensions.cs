namespace UnityEngine
{
    public static class GUIExtensions
    {
        public static GUIStyle SetupColors(this GUIStyle guiStlye, Color color)
        {
            guiStlye.normal.textColor = color;
            guiStlye.hover.textColor = color;
            guiStlye.active.textColor = color;
            guiStlye.focused.textColor = color;

            return guiStlye;
        }

        public static GUIStyle SetupBackground(this GUIStyle guiStlye, Texture2D texture2D)
        {
            guiStlye.normal.background = texture2D;
            guiStlye.active.background = texture2D;
            guiStlye.hover.background = texture2D;
            guiStlye.focused.background = texture2D;

            return guiStlye;
        }
    }
}