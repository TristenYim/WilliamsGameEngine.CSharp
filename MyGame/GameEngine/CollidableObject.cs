using GameEngine;
using SFML.System;
using SFML.Graphics;

namespace GameEngine
{
    class CollidableObject : GameObject
    {
        // The position of the top left corner of the sprite in terms of the game object tree (which is separate from _sprite.Position).
        private Vector2f _position;
        public Vector2f Position {
            get => _position;
            set => _position = value;
        } 


        public CollidableObject(Vector2f position) {
            _position = position;
            AssignTag("collidable");
        }

        // This function lets you specify a rectangle for collision checks.
        /*
        public virtual FloatRect GetCollisionRect()
        {
        }*/

        // Update is called every frame. Use this to prepare to draw (move, perform AI, etc.).
        public override void Update(Time elapsed)
        {
        }
    }
}