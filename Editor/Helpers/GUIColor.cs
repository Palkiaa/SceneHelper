using System;

namespace UnityEngine
{
    public class GUIColor : IDisposable
    {
        private Color _originalColor;
        private Color _color;

        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                GUI.color = value;
            }
        }

        public GUIColor(Color color)
        {
            _originalColor = GUI.color;
            _color = color;
            GUI.color = color;
        }

        public void Dispose()
        {
            GUI.color = _originalColor;
        }
    }
}