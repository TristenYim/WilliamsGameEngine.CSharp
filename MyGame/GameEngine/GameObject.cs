using System.Collections.Generic;
using SFML.System;
using SFML.Graphics;

namespace GameEngine
{
    // This class represents every object in your game, such as the player, enemies, and so on.
    abstract class GameObject
    {
        private bool _isDead;

        // Set this to true if and only if you want stuff to be able to collide with this.
        protected bool _isCollidable;

        // Set this to true if and only if you want this to check for collisions.
        protected bool _isCollisionCheckEnabled;

        // Set this to true if and only if this object belongs somewhere on the positional tree.
        protected bool _belongsOnTree;

        // Using a set prevents duplicates.
        protected readonly HashSet<string> _tags = new HashSet<string>();

        // Points to the positional tree node it belongs in, if this belongs of the positional tree.
        private PositionalTree _nodePointer;
        public PositionalTree NodePointer
        {
            get => _nodePointer;
            set => _nodePointer = value;
        }

        public bool IsCollidable
        {
            get => _isCollidable;
            set => _isCollidable = value;
        }

        public bool BelongsOnTree
        {
            get => _belongsOnTree;
        }

        public bool IsCollisionCheckEnabled()
        {
            return _isCollisionCheckEnabled;
        }

        public void SetCollisionCheckEnabled(bool isCollisionCheckEnabled)
        {
            _isCollisionCheckEnabled = isCollisionCheckEnabled;
        }

        // This is the rectangular area that this object takes up.
        public virtual FloatRect GetCollisionRect()
        {
            return new FloatRect();
        }

        // This is the object's 2D position on the positional tree.
        public virtual Vector2f Position
        {
            get;
            set;
        }

        // Tags let you annotate your objects so you can identify them later
        // (such as "player").
        public void AssignTag(string tag)
        {
            _tags.Add(tag);
        }

        public bool HasTag(string tag)
        {
            return _tags.Contains(tag);
        }

        // "Dead" game objects will be removed from the scene.
        public bool IsDead()
        {
            return _isDead;
        }

        public void MakeDead()
        {
            _isDead = true;
        }

        // Update is called every frame. Use this to prepare to draw (move, perform AI, etc.).
        public virtual void Update(Time elapsed) {}

        // Draw is called every frame. Use this to draw stuff on screen, and make sure to account for the pos offset the camera provides.
        public virtual void Draw(Vector2f posOffset) {}

        // This is called whenever something collides with this.
        public virtual void HandleCollision(GameObject otherGameObject) {}
    }
}