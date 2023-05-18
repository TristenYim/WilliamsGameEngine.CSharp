using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // This class represents any object which displays text.
    class TextObject : GameObject
    {
        
        // The text we draw on screen.
        private Text _text;
        public Text Text
        {
            get => _text;
            set => _text = value;
        }

        // Constructs the text with none of the text properties (DisplayedString, font, position) set.
        public TextObject()
        {
            _text = new Text();

            // This tag can be used to easily tell if this is a text object
            AssignTag("textObject");
        }

        // Constructs the text with all of the text properties (DisplayedString, font, position) set.
        public TextObject(string text, Font font, uint charSize, Color color, Vector2f pos)
        {
            _text = new Text(text, font, charSize);
            _text.Position = pos;

            // This tag can be used to easily tell if this is a text object
            AssignTag("textObject");
        }

        public override void Draw()
        {
            // Draws the text on screen
            Game.RenderWindow.Draw(_text);
        }

        public override void Update(Time elapsed) {}
    }
}