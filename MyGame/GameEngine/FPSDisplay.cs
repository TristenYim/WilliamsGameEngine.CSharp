using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // This class is a built-in text object which displays the fps, which can be used to assess performance.
    class FPSDisplay : TextObject
    {

        // Timer used to keep track of fps.
        private Timer _timer;
        
        // The total number of game frames since this object was constructed or reset.
        private int _totalFrames;

        // Constructs the text with a built-in font, font size, color, at a built-in position.
        public FPSDisplay()
        {
            base.Text = new SFML.Graphics.Text("", Game.GetFont("Resources/Courneuf-Regular.ttf"), 16);
            Text.Color = new Color(0, 255, 0);
            Text.Position = new Vector2f(10, 10);

            // Set to 1 to avoid divide by 0 error.
            _timer = new Timer(2000, false);

            _totalFrames = 0;
            AssignTag("textObject");
            AssignTag("fps");
        }
        public override void Update(Time elapsed)
        {
            _timer.Update(elapsed);
            _totalFrames++;
            decimal fps = (decimal)_totalFrames / (decimal)_timer.Time * 1000;
            Text.DisplayedString = "FPS: " + decimal.Round(fps, 1);
            if (_timer.surpassedTarget()) {
                _timer.reset();
                _totalFrames = 0;
            }
        }
    }
}