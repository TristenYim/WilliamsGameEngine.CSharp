using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // This class is a built-in text object which displays the fps, which can be used to assess performance.
    class FPSDisplay : TextObject
    {

        // Timer used to keep track of fps.
        private readonly Timer _timer;
        
        // The total number of game frames since this object was constructed or reset.
        private int _totalFrames;

        // The index of the Camera this draws on.
        private readonly int _cameraIndex;

        // The color of the dipslayed fps.
        private readonly static Color FPSColor = Color.Yellow;

        // Constructs the text with a built-in font, font size, color, at a built-in position.
        public FPSDisplay()
        {
            _cameraIndex = Game.CurrentScene.Cameras.Length - 1;
            base.Text = new SFML.Graphics.Text("", Game.GetFont("Resources/Courneuf-Regular.ttf"), 24);
            Text.Color = FPSColor;
            Text.Position = new Vector2f(10, 10);

            // Set to 1 to avoid divide by 0 error.
            _timer = new Timer(500);

            _totalFrames = 0;
            AssignTag("textObject");
            AssignTag("fps");
        }
        public override void Update(Time elapsed)
        {
            _timer.Update(elapsed);
            _totalFrames++;
            if (_timer.Time != 0)
                {
                decimal fps = (decimal)_totalFrames / (decimal)_timer.Time * 1000;
                if (_timer.SurpassedTarget)
                {
                    Text.DisplayedString = "FPS: " + decimal.Round(fps, 1);
                    _timer.Reset();
                    _totalFrames = 0;
                }
                
                Game.CurrentScene.Cameras[_cameraIndex].DrawQueue.Enqueue(this);
            }
        }
    }
}