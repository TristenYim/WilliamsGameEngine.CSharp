using SFML.System;
using SFML.Graphics;

namespace GameEngine
{
    class Camera
    {
        // These are the four bounding lines of the Camera.
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

        // This is the current scale factor which will be applied to all Sprites (that call UpdateCameraSprite).
        private Vector2f _scale = new Vector2f(1, 1);
        public Vector2f Scale
        {
            get => _scale;
        }

        // This constructs the bounds of the Camera.
        public Camera(FloatRect bounds)
        {
            _left = bounds.Left;
            _top = bounds.Top;
            _right = _left + bounds.Width;
            _bottom = _top + bounds.Height;

            setScale();
        }

        // This shifts the Camera.
        public void Translate(Vector2f vector)
        {
            float x = vector.X;
            float y = vector.Y;
            _left += x;
            _top += y;
            _right += x;
            _bottom += y;
        }

        // This zooms the camera about the top left corner
        public void Dilate(float factor)
        {
            _left *= factor;
            _top *= factor;
            _right *= factor;
            _bottom *= factor;
            _scale *= factor;
        }

        private void setScale()
        {
            float scaleX = Game.RenderWindow.Size.X / (_right - _left);
            float scaleY = Game.RenderWindow.Size.Y / (_top - _bottom);

            // Do this to ensure the image fits on the screen even if the aspect ratio is off.
            if (scaleX > scaleY)
            {
                scaleY = scaleX;
            }
            else
            {
                scaleX = scaleY;
            }

            _scale.X = scaleX;
            _scale.Y = scaleY;
        }
    }
}