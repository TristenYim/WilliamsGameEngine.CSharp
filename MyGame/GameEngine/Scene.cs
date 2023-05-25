using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GameEngine
{
    // The Scene manages all the GameObjects currently in the game.
    abstract class Scene
    {
        // This holds our GameObjects.
        private readonly List<GameObject> _gameObjects = new List<GameObject>();

        // This is a 2D space of GameObjects which can be (but does not have to be) bounded by the Window.
        protected PositionalTree _positionalObjects;
        public PositionalTree PositionalTree
        {
            get => _positionalObjects;
        }

        // This determines how the objects in PostitionalTree should be offset and scaled to the screen.
        protected Camera _camera;
        public Camera Camera
        {
            get => _camera;
        }

        // Set to true to make WASD shift the Camera.
        protected bool _cameraDebugMode = false;

        // TODO: Add culling
        // Does not draw offscreen elements (except for bounding boxes drawn by PositionalTree) if true.
        //protected bool _cullingEnabled = true;

        // Puts a GameObject into the scene and the PositionalTree if appropriate.
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

            // Handle any Keyboard, Mouse events, etc. for our game Window.
            Game.RenderWindow.DispatchEvents();

            HandleCollisions();
            UpdateGameObjects(time);
            RemoveDeadGameObjects();
            DrawGameObjects();
            _positionalObjects.RecursiveDraw(new Vector2f(Camera.Left, Camera.Top));

            // Draw the Window as updated by the GameObjects.
            Game.RenderWindow.Display();
        }

        // This method lets Game objects respond to Collisions.
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

        // This method calls Update on each of our GameObjects.
        private void UpdateGameObjects(Time time)
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                _gameObjects[i].Update(time);
            }

            // This is for Camera debug mode, which gives manual Camera controls which are useful for well... debugging.
            if (_cameraDebugMode)
            {
                if (Keyboard.IsKeyPressed(Keyboard.Key.W))
                {
                    Camera.Translate(new Vector2f(0, 500 * time.AsSeconds()));
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.A))
                {
                    Camera.Translate(new Vector2f(500 * time.AsSeconds(), 0));
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    Camera.Translate(new Vector2f(0, -500 * time.AsSeconds()));
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    Camera.Translate(new Vector2f(-500 * time.AsSeconds(), 0));
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.Q))
                {
                    Camera.Dilate(1 + 0.5f * time.AsSeconds());
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.E))
                {
                    Camera.Dilate(1 - 0.5f * time.AsSeconds());
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.R))
                {
                    Camera.Dilate(1 / Camera.Scale.X);
                    Camera.Translate(new Vector2f(-Camera.Left, -Camera.Top));
                }
            }
        }

        // This method calls draw on each of our game objects.
        private void DrawGameObjects()
        {
            foreach (var gameObject in _gameObjects) 
            {
                gameObject.Draw();
            }
        }

        // Sets the position of the Sprite based on its position on the Tree and the Camera. Call this in Draw if your Sprite is on the Tree.
        public void UpdateCameraObject(Transformable transformable, Vector2f position)
        {
            float scaleFactor = _camera.Scale.X;
            transformable.Position = new Vector2f((position.X + Camera.Left) * scaleFactor, (position.Y + Camera.Top) * scaleFactor);
            transformable.Scale = _camera.Scale;
        }

        // This method removes GameObjects that indicate they are dead from the Scene.
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
