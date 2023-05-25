using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // This class represents any object which displays text.
    class TextObject : GameObject
    {
        
        // The Text we draw on screen.
        public Text Text { get; set; }

        // Constructs the Text with none of the Text properties (DisplayedString, Font, Position) set.
        public TextObject()
        {
            Text = new Text();
        }

        // Constructs the Text with all of the text properties (DisplayedString, font, position) set.
        public TextObject(string text, Font font, uint charSize, Color color, Vector2f pos)
        {
            BelongsOnTree = false;
            IsCollidable = false;
            _isCollisionCheckEnabled = false;

            Text = new Text(text, font, charSize);
            Text.Position = pos;
        }

        public override void Draw()
        {
            // Draws the Text on screen.
            Game.RenderWindow.Draw(Text);
        }

        public override void Update(Time elapsed) {}
    }
}