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

        // This is the fullscreen ViewPort
        public FloatRect StdViewport { get; private set; }
        public FloatRect FullScreenViewport { get; private set; }

        // This is the Queue of objects to draw
        // TODO: Make this an array or list of Queues to allow for render layers.
        public Queue<GameObject> DrawQueue { get; }

        // This constructs the bounds of the Camera.
        public Camera(FloatRect bounds)
        {
            View = new View(bounds);
            StdViewport = View.Viewport;
            
            GenerateFullscreenViewport();
            System.Console.WriteLine(bounds.ToString() + StdViewport.ToString() + FullScreenViewport.ToString());

            DrawQueue = new Queue<GameObject>();
        }

        // Can only be called if isFull
        public void GenerateFullscreenViewport()
        {
            Vector2f fullScreenSize = new Vector2f(VideoMode.DesktopMode.Width, VideoMode.DesktopMode.Height);
            Vector2f stdSize = (Vector2f)Game.RenderWindow.Size;
            stdSize.X *= StdViewport.Width;
            stdSize.Y *= StdViewport.Height;
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