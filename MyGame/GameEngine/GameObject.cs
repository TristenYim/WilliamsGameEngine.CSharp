using System.Collections.Generic;
using SFML.System;
using SFML.Graphics;

namespace GameEngine
{
    // This class represents every object in your game, such as the player, enemies, and so on.
    abstract class GameObject
    {
        private bool _isDead;

        // Set this to true if you want this to check for collisions with other GameObjects.
        protected bool _isCollisionCheckEnabled;

        // Using a set prevents duplicates.
        protected readonly HashSet<string> _tags = new HashSet<string>();

        // Set this to true if this object has a position and should be inserted on the PositionalTree.
        public bool BelongsOnTree { get; protected set; }

        // Set this to true if you want GameObjects to check for collsions with this.
        // NOTE: BelongsOnTree must also be true for this to work!
        public bool IsCollidable { get; set; }

        public bool IsCollisionCheckEnabled()
        {
            return _isCollisionCheckEnabled;
        }

        public void SetCollisionCheckEnabled(bool isCollisionCheckEnabled)
        {
            _isCollisionCheckEnabled = isCollisionCheckEnabled;
        }

        // This is the rectangular collision area in PositionalTree that will trigger collisions if other GameObjects intersect it  .
        public virtual FloatRect GetCollisionRect()
        {
            return new FloatRect();
        }

        // This is the GameObject's 2D position on the PositionalTree.
        public Vector2f Position { get; set; }

        // Points to the LinkedList node this is in.
        public LinkedListNode<GameObject> ListNodePointer { get; set; }
        
        // Points to the PositionalTree node this is in, if applicable.
        public SpatialTree TreeNodePointer { get; set; }

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

        // "Dead" GameObjects will be removed from the scene.
        public bool IsDead()
        {
            return _isDead;
        }

        public void MakeDead()
        {
            _isDead = true;
        }

        // Update is called every frame. Use this to prepare to Draw (move, perform AI, etc.).
        public virtual void Update(Time elapsed) {}

        // Draw is called every frame. Use this to draw stuff on screen (and call UpdateCameraSprite in scene if the camera affects its Sprite).
        public virtual void Draw() {}

        // This is called whenever something collides with this.
        public virtual void HandleCollision(GameObject otherGameObject) {}
    }
}