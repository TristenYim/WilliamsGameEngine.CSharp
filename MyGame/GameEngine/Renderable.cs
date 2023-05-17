using SFML.Graphics;

// This is an interface. An interface is like a contract, with any object that implements it agreeing to do something.
namespace GameEngine
{
    // This represents any objects that can be drawn.
    interface Renderable
    {
        // Game calls draw on any object that implements this.
        public void Draw();

        // Gets the area that this object renders, used mostly for culling.
        public FloatRect RenderBounds
        {
            get;
        }
    }
}