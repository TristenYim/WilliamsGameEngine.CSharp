using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;

namespace GameEngine
{
    // The Scene manages all the GameObjects currently in the game.
    class Scene
    {
        // This holds our game objects.
        private readonly List<GameObject> _gameObjects = new List<GameObject>();

        // This holds all objects that implement the positional interface. This is used to improve the speed of collision detection and object searching.
        private PositionalTree _positionalObjects = new PositionalTree(new FloatRect(0, 0, Game.RenderWindow.Size.X, Game.RenderWindow.Size.Y), null);

        public PositionalTree PositionalTree
        {
            get => _positionalObjects;
        }

        // Puts a GameObject into the scene.
        public void AddGameObject(GameObject gameObject)
        {
            // This adds the game object onto the back (the end) of the list of game objects.
            _gameObjects.Add(gameObject);
            if (gameObject.BelongsOnTree)
            {
                _positionalObjects.Insert(gameObject);
            }
        }

        // TODO: Add a completely quad tree for SpriteGameObjects
        //       In its current state, handleCollisions is O(n), which is not ideal, especially if there are many objects that check for collisions
        //       Storing a reference to each SpriteGameObject in a quad tree will make handleCollisions O(N^2), with the price of having to keep the
        //       tree sorted
        
        // Called by the Game instance once per frame.
        public void Update(Time time)
        {
            // Clear the window.
            Game.RenderWindow.Clear();

            // Go through our normal sequence of game loop stuff.

            // Handle any keyboard, mouse events, etc. for our game window.
            Game.RenderWindow.DispatchEvents();

            HandleCollisions();
            UpdateGameObjects(time);
            RemoveDeadGameObjects();
            DrawGameObjects();
            _positionalObjects.RecursiveDraw();

            // Draw the window as updated by the game objects.
            Game.RenderWindow.Display();
        }

        // This method lets game objects respond to collisions.
        private void HandleCollisions()
        {
            /*for (int i = 0; i < _gameObjects.Count; i++)
            {    
                var gameObject = _gameObjects[i];

                if (gameObject is not Collidable)
                {
                    continue;
                }

                // Only check objects that ask to be checked.
                if (!((Collidable)gameObject).ChecksForCollisions) continue;

                FloatRect collisionRect = ((Collidable)gameObject).CollisionRect;

                // Don't bother checking if this game object has a collision rectangle with no area.
                if (collisionRect.Height == 0 || collisionRect.Width == 0) continue;

                // See if this game object is colliding with any other game object.
                for (int j = 0; j < _gameObjects.Count; j++)
                {
                    var otherGameObject = _gameObjects[j];

                    if (otherGameObject is not Collidable)
                    {
                        continue;
                    }

                    // Don't check an object colliding with itself.
                    if (gameObject == otherGameObject) continue;

                    if (gameObject.IsDead()) return;

                    // When we find a collision, invoke the collision handler for both objects.
                    if (collisionRect.Intersects(((Collidable)otherGameObject).CollisionRect))
                    {
                        ((Collidable)gameObject).HandleCollision((Collidable)otherGameObject);
                        ((Collidable)otherGameObject).HandleCollision((Collidable)gameObject);
                    }
                }
            }*/
            _positionalObjects.HandleCollisions();
        }

        // This function calls update on each of our game objects.
        private void UpdateGameObjects(Time time)
        {
            for (int i = 0; i < _gameObjects.Count; i++) _gameObjects[i].Update(time);
        }

        // This function calls draw on each of our game objects.
        private void DrawGameObjects()
        {
            foreach (var gameObject in _gameObjects) 
            {
                gameObject.Draw();
            }
        }

        // This function removes objects that indicate they are dead from the scene.
        private void RemoveDeadGameObjects()
        {
            // This is a "lambda", which is a fancy name for an anonymous function.
            // It's "anonymous" because it doesn't have a name. We've declared a variable
            // named "isDead", and that variable can be used to call the function, but the
            // function itself is nameless.
            //Predicate<GameObject> isDead = gameObject => gameObject.IsDead();

            // Here we use the lambda declared above by passing it to the standard RemoveAll
            // method on List<T>, which calls our lambda once for each element in
            // gameObjects. If our lambda returns true, that game object ends up being
            // removed from our list.
            //_gameObjects.RemoveAll(isDead);
            for (int i = _gameObjects.Count - 1; i > -1; i--)
            {
                GameObject gameObject = _gameObjects[i];
                if (gameObject.IsDead())
                {
                    if (gameObject.BelongsOnTree)
                    {
                        _positionalObjects.Delete(gameObject);
                    }
                    _gameObjects.RemoveAt(i);
                }
            }
        }
    }
}
