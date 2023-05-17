using SFML.System;

// This is an interface. An interface is like a contract, with any object that implements it agreeing to do something.
namespace GameEngine
{
    // This represents any objects that exists within the 2D space in the positional tree.
    interface Positional
    {
        // This allows other classes to get and set the position.
        public Vector2f Position
        {
            get;
            set;
        }

        // This is used for quick deletion once a positional object has already been inserted in the tree.
        public PositionalTree TreeNodePointer
        {
            get;
            set;
        }
    }
}