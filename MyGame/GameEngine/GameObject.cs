using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // This class represents every object in your game, such as the player, enemies, and so on.
    abstract class GameObject : Renderable
    {
        // TODO: Add an internal constant which stores its index in _gameObjects in Scene
        //       This would save the engine from checking if an object is dead on update, since removing an object when you know its index is easy
        //       Not entirely sure if this would really run faster, but it is worth trying
        private bool _isCollisionCheckEnabled;

        private bool _isDead;

        // Using a set prevents duplicates.
        private readonly HashSet<string> _tags = new HashSet<string>();

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
        public abstract void Update(Time elapsed);

        // Draw is called once per frame. Use this to draw your object to the screen.
        // TODO: Remove Draw() and add it to a separate object called SpriteGameObject
        public virtual void Draw()
        {
        }

        public virtual FloatRect RenderBounds
        {
            get;
        }

        // This flag indicates whether this game object should be checked for collisions.
        // The more game objects in the scene that need to be checked, the longer it takes.
        // TODO: Remove IsCollisionCheckEnabled and add it to a separate object called SpriteGameObject
        public bool IsCollisionCheckEnabled()
        {
            return _isCollisionCheckEnabled;
        }

        // TODO: Remove SetCollisionCheckEnabled and add it to a separate object called SpriteGameObject
        public void SetCollisionCheckEnabled(bool isCollisionCheckEnabled)
        {
            _isCollisionCheckEnabled = isCollisionCheckEnabled;
        }

        // This function lets you specify a rectangle for collision checks.
        // TODO: Remove SetCollisionCheckEnabled and add it to a separate object called SpriteGameObject
        public virtual FloatRect GetCollisionRect()
        {
            return new FloatRect();
        }

        // Use this to specify what happens when this object collides with another object.
        // TODO: Remove HandleCollision and add it to a separate object called SpriteGameObject
        public virtual void HandleCollision(GameObject otherGameObject)
        {
        }
    }
}