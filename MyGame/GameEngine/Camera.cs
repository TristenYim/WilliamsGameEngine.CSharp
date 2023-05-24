using SFML.System;
using SFML.Graphics;

namespace GameEngine
{
    class Camera
    {
        // These are the four bounding lines of the camera.
        private float _left;
        private float _top;
        private float _right;
        private float _bottom;
        public float Left
        {
            get => _left;
            set => _left = value;
        }
        public float Top
        {
            get => _top;
            set => _top = value;
        }
        public float Right
        {
            get => _right;
            set => _right = value;
        }
        public float Bottom
        {
            get => _bottom;
            set => _bottom = value;
        }

        // This constructs the bounds of the camera.
        public Camera(FloatRect bounds)
        {
            _left = bounds.Left;
            _top = bounds.Top;
            _right = _left + bounds.Width;
            _bottom = _top + bounds.Height;
        }

        // This shifts the camera.
        public void Translate(Vector2f vector)
        {
            float x = vector.X;
            float y = vector.Y;
            _left += x;
            _top += y;
            _right += x;
            _bottom += y;
        }

        // TODO: Implement this.
        public void Dilate(float factor, Vector2f origin)
        {
            
        }
    }
}