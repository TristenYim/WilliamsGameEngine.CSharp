using GameEngine;
using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // This class is a built-in text object which displays the lps, which can be used to assess performance.
    class LPSDisplay : TextObject
    {

        // The total time in milliseconds since this object was constructed.
        private uint _totalTime;
        
        // The total number of game loops since this object was constructed.
        private int _totalLoops;

        // Constructs the text with a built-in font, font size, color, at a built-in position.
        public LPSDisplay()
        {
            base.Text = new SFML.Graphics.Text("", Game.GetFont("Resources/Courneuf-Regular.ttf"), 16);
            Text.Color = new Color(0, 255, 0);
            Text.Position = new Vector2f(10, 10);

            // Set to 1 to avoid divide by 0 error.
            _totalTime = 1;

            _totalLoops = 0;
            AssignTag("textObject");
            AssignTag("lps");
        }
        public override void Update(Time elapsed)
        {
            _totalTime += (uint)elapsed.AsMilliseconds();
            _totalLoops++;
            decimal lps = (decimal)_totalLoops / (decimal)_totalTime * 1000;
            Text.DisplayedString = decimal.Round(lps, 1) + " LPS";
        }
    }
}