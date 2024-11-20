using System.Collections.Generic;
using SFML.System;
using SFML.Graphics;

namespace GameEngine
{
    // This class represents every object in your game, such as the player, enemies, and so on.
    abstract class GameObject
    {
        // If _isDead is true, this will stop updating and drawing and eventually be removed frome _gameObjects and SpatialTree in the current Scene.
        private bool _isDead;

        // Using a set prevents duplicates.
        protected readonly HashSet<string> _tags = new HashSet<string>();

        // This is the GameObject's 2D position on the SpatialTree.
        public Vector2f Position { get; set; }
        
        public SpatialTreeMember TreeMemberPointer { get; set; }

        // Points to the SpatialTree node this is in, if applicable.
        //public SpatialTree TreeNodePointer { get; set; }

        // These sets define how this collides with other GameObjects. 
        // This can only collide with objects that are broadcasting its position on one of the the same "layers" (represented by a string) 
        //      as one one of the "layers" this is checking.  
        protected HashSet<string> _collisionBroadcastLayers = new HashSet<string>();
        protected HashSet<string> _collisionCheckLayers = new HashSet<string>();
        public bool IsBroadcastingCollionLayers()
        {
            return _collisionBroadcastLayers.Count != 0;
        }

        public bool IsCollisionCheckEnabled()
        {
            return _collisionCheckLayers.Count != 0;
        }
        
        // This checks if this and otherGameObject are colliding.
        public virtual bool IsCollidingWith (GameObject otherGameObject)
        {
            if (GetCollisionRect().Intersects(otherGameObject.GetCollisionRect()))
            {
                // First check if _collisionCheckLayers contains a string in otherGameObject's _collisionBroadcastLayers.
                foreach (var layer in _collisionCheckLayers)
                {
                    if (otherGameObject._collisionBroadcastLayers.Contains(layer))
                    {
                        return true;
                    }
                }
                // Then check if a string in _collisionBroadcastLayers is contained in otherGameObject's _collisionCheckLayers.
                foreach (var layer in otherGameObject._collisionCheckLayers)
                {
                    if (_collisionBroadcastLayers.Contains(layer))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        // This is the rectangular collision area in SpatialTree that will trigger collisions if other GameObjects intersect it  .
        public virtual FloatRect GetCollisionRect()
        {
            return new FloatRect();
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