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

        // This constructs the bounds of the Camera.
        public Camera(FloatRect bounds)
        {
            View = new View(bounds);
            StdViewport = View.Viewport;
            
            GenerateFullscreenViewport();
            System.Console.WriteLine(bounds.ToString() + StdViewport.ToString() + FullScreenViewport.ToString());
        }

        public void GenerateFullscreenViewport()
        {
            Vector2u fullScreenSize = new Vector2u(VideoMode.DesktopMode.Width, VideoMode.DesktopMode.Height);
            Vector2f stdSize = (Vector2f)Game.RenderWindow.Size;
            stdSize.X *= StdViewport.Width;
            stdSize.Y *= StdViewport.Height;
            float scaleXFactor = fullScreenSize.X / stdSize.X;
            float scaleYFactor = fullScreenSize.Y / stdSize.Y;

            if (scaleXFactor > scaleYFactor)
            {
                // If the aspect ratio changes such that y will be stretched, we need to stretch x by the same amount to correct for it.
                float stretchFactor = scaleXFactor / scaleYFactor;
                FloatRect viewPort = StdViewport;
                float width = viewPort.Width;
                FullScreenViewport = new FloatRect(width * (stretchFactor - 1) / 2, viewPort.Top, width / stretchFactor, viewPort.Height);
            }
            else if (scaleXFactor < scaleYFactor)
            {
                // If the aspect ratio changes such that x will be stretched, we need to stretch y by the same amount to correct for it.
                float stretchFactor = scaleYFactor / scaleXFactor;
                FloatRect gameViewPort = View.Viewport;
                float height = gameViewPort.Height;
                FullScreenViewport = new FloatRect(gameViewPort.Left, height * (stretchFactor - 1) / 2, gameViewPort.Width, height);
            }
            else
            {
                // If scaleXFactor equals scaleYFactor, no stretch compensation is necessary.
                FullScreenViewport = new FloatRect(StdViewport.Left, StdViewport.Top, StdViewport.Width, StdViewport.Height);
            }
        }
    }
}