using SFML.System;
using SFML.Graphics;

namespace GameEngine
{
    class Camera : Transformable
    {
        // These are the four bounding lines of the Camera.
        public float Width { get; set; }
        public float Height { get; set; }

        // This constructs the bounds of the Camera.
        public Camera(FloatRect bounds)
        {
            Position = new Vector2f (bounds.Left, bounds.Top);
            Width = bounds.Width;
            Height = bounds.Height;
            Scale = new Vector2f(1f, 1f);
            Origin = new Vector2f(Width / 2f, Height / 2f);
        }

        // This shifts the Camera.
        public void Translate(Vector2f vector)
        {
            float x = vector.X;
            float y = vector.Y;
            Position = new Vector2f(Position.X + x, Position.Y + y);
        }
    }
}