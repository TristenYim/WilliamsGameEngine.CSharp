using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GameEngine
{
    // The Scene manages all the GameObjects currently in the game.
    abstract class Scene
    {
        // This holds our game objects.
        private readonly List<GameObject> _gameObjects = new List<GameObject>();

        // This holds all objects that implement the positional interface. This is used to improve the speed of collision detection and object searching.
        protected PositionalTree _positionalObjects;
        public PositionalTree PositionalTree
        {
            get => _positionalObjects;
        }

        // This is the camera which determines how much of the positionalTree should be drawn
        protected Camera _camera;
        public Camera Camera
        {
            get => _camera;
        }

        // Set to true 
        protected bool _cullingEnabled;

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
            _positionalObjects.RecursiveDraw(new Vector2f(Camera.Left, Camera.Top));

            // Draw the window as updated by the game objects.
            Game.RenderWindow.Display();
        }

        // This method lets game objects respond to collisions.
        private void HandleCollisions()
        {
            _positionalObjects.HandleCollisions();

            // Fun fact: Before an AP student improved this engine as their final project, it used this old, inferior collision system!
            // If you're making a really complicated game with lots of collision detection, try uncommenting this to see how slow it performs!
            
            /*
            for (int i = 0; i < _gameObjects.Count; i++)
            {    
                var gameObject = _gameObjects[i];

                if (!gameObject.IsCollidable)
                {
                    continue;
                }

                // Only check objects that ask to be checked.
                if (!gameObject.IsCollisionCheckEnabled()) continue;

                FloatRect collisionRect = gameObject.GetCollisionRect();

                // Don't bother checking if this game object has a collision rectangle with no area.
                if (collisionRect.Height == 0 || collisionRect.Width == 0) continue;

                // See if this game object is colliding with any other game object.
                for (int j = 0; j < _gameObjects.Count; j++)
                {
                    var otherGameObject = _gameObjects[j];

                    if (!otherGameObject.IsCollidable)
                    {
                        continue;
                    }

                    // Don't check an object colliding with itself.
                    if (gameObject == otherGameObject) continue;

                    if (gameObject.IsDead()) return;

                    // When we find a collision, invoke the collision handler for both objects.
                    if (collisionRect.Intersects(otherGameObject.GetCollisionRect()))
                    {
                        gameObject.HandleCollision(otherGameObject);
                        (otherGameObject).HandleCollision(gameObject);
                    }
                }
            }
            */
        }

        // This function calls update on each of our game objects.
        private void UpdateGameObjects(Time time)
        {
            for (int i = 0; i < _gameObjects.Count; i++) _gameObjects[i].Update(time);
        }

        // This function calls draw on each of our game objects.
        private void DrawGameObjects()
        {
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                Camera.Translate(new Vector2f(0, -2));
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.E))
            {
                Camera.Translate(new Vector2f(0, 2));
            }

            foreach (var gameObject in _gameObjects) 
            {
                float left = _camera.Left;
                float top = _camera.Top;
                float right = _camera.Right;
                float bottom = _camera.Bottom;
                /*if (gameObject.BelongsOnTree)
                {
                }
                else
                {*/
                    gameObject.Draw(new Vector2f(left, top));
                //}
            }
        }

        // This function removes objects that indicate they are dead from the scene.
        private void RemoveDeadGameObjects()
        {
            // Index game objects and remove them if they are dead.
            for (int i = _gameObjects.Count - 1; i > -1; i--)
            {
                GameObject gameObject = _gameObjects[i];
                if (gameObject.IsDead())
                {
                    if (gameObject.BelongsOnTree)
                    {
                        // Delete it from the tree if it should be on the tree.
                        _positionalObjects.Delete(gameObject);
                    }
                    _gameObjects.RemoveAt(i);
                }
            }
        }
    }
}
