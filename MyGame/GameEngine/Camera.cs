using System.Collections.Generic;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace GameEngine
{
    class Camera
    {
        // This is the internal standard View the camera controls.
        public readonly View View;

        // This is the standard ViewPort
        public FloatRect StdViewport { get; private set; }

        // This is the fullscreen ViewPort
        public FloatRect FullScreenViewport { get; private set; }

        // The original bounds are stored to allow Camera to internally reset the View.
        private FloatRect _originalBounds;

        // This is the Queue of objects to draw.
        // A Queue is a bit like a list, except you can only add items to the back, and can only look at or take off items on the front.
        // TODO: Make this an array or list of Queues to allow for render layers.
        public Queue<GameObject> DrawQueue { get; }

        // This constructs the bounds of the Camera.
        public Camera(FloatRect bounds)
        {
            _originalBounds = bounds;
            
            View = new View(bounds);
            StdViewport = View.Viewport;
            GenerateFullscreenViewport();

            DrawQueue = new Queue<GameObject>();
        }

        public void Draw()
        {
            if (!Game.IsFullscreen)
            {
                View.Viewport = StdViewport;
            }
            else
            {
                View.Viewport = FullScreenViewport;
            }
            Game.RenderWindow.SetView(View);

            while (DrawQueue.TryDequeue(out var toDraw))
            {
                if (!toDraw.IsDead())
                {
                    toDraw.Draw();
                }
            }
        }

        // Resets the View to its original state when Camera was instantiated.
        public void Reset()
        {
            View.Reset(_originalBounds);
        }

        // Only call if IsFullscreen is disabled in Game.
        public void GenerateFullscreenViewport()
        {
            Vector2f fullScreenSize = new Vector2f(VideoMode.DesktopMode.Width, VideoMode.DesktopMode.Height);
            Vector2f stdSize = new Vector2f(Game.RenderWindow.Size.X * StdViewport.Width, Game.RenderWindow.Size.Y * StdViewport.Height);
            float scaleXFactor = fullScreenSize.X / stdSize.X;
            float scaleYFactor = fullScreenSize.Y / stdSize.Y;

            if (scaleXFactor > scaleYFactor)
            {
                // If the aspect ratio changes such that x will be stretched, we need to stretch it by the same amount to correct for it.
                float stretchFactor = scaleXFactor / scaleYFactor;
                FullScreenViewport = new FloatRect((fullScreenSize.X - stdSize.X * scaleYFactor) / fullScreenSize.X / 2f, 0f, StdViewport.Width / stretchFactor, StdViewport.Height);
            }
            else if (scaleXFactor < scaleYFactor)
            {
                // If the aspect ratio changes such that x will be stretched, we need to stretch y by the same amount to correct for it.
                float stretchFactor = scaleYFactor / scaleXFactor;
                FullScreenViewport = new FloatRect(0f, (fullScreenSize.Y - stdSize.Y * scaleXFactor) / fullScreenSize.Y / 2f, StdViewport.Width, StdViewport.Height / stretchFactor);
            }
            else
            {
                // If scaleXFactor equals scaleYFactor, no stretch compensation is necessary.
                FullScreenViewport = new FloatRect(StdViewport.Left, StdViewport.Top, StdViewport.Width, StdViewport.Height);
            }
        }
    }
}