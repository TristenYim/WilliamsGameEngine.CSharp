using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // This class represents any object which displays text.
    class TextObject : GameObject
    {
        // The Text we draw on screen.
        public Text Text { get; set; }

        protected int _cameraIndex;

        // Constructs the Text with none of the Text properties (DisplayedString, Font, Position) set.
        public TextObject()
        {
            Text = new Text();
        }

        // Constructs the Text with all of the text properties (DisplayedString, font, position) set.
        public TextObject(string text, Font font, uint charSize, Color color, Vector2f pos, int cameraIndex)
        {
            Text = new Text(text, font, charSize);
            Text.Position = pos;

            _cameraIndex = cameraIndex;
        }

        public override void Draw()
        {
            // Draws the Text on screen.
            Game.RenderWindow.Draw(Text);
        }

        public override void Update(Time elapsed) 
        {
            Scene currentScene = Game.CurrentScene;
            currentScene.Cameras[_cameraIndex].DrawQueue.Enqueue(this);
        }
    }
}