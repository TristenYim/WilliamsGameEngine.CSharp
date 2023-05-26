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
        public PositionalTree PositionalTree { get; protected set; }

        // Enabling will draw boxes around used PostitionalTree nodes (and the collision boxes of objects inside).
        // This can be useful for troubleshooting broken collision detection or visualizing how PositionalTree is optimtizing your game.
        // PLEASE NOTE: Enabling this will greatly reduce framerate due to the amount of extra things being drawn on screen!
        protected bool _treeDebugMode = false;

        // This determines how the objects in PostitionalTree should be offset and scaled to the screen.
        public Camera Camera { get; protected set; }

        // Set to true to make WASD shift the Camera.
        protected bool _cameraDebugMode = false;

        // These are the keybinds for the Camera debug movements.
        protected Keyboard.Key _cameraDebugForwardKey = Keyboard.Key.W;
        protected Keyboard.Key _cameraDebugLeftKey = Keyboard.Key.A;
        protected Keyboard.Key _cameraDebugBackwardKey = Keyboard.Key.S;
        protected Keyboard.Key _cameraDebugRightKey = Keyboard.Key.D;
        protected Keyboard.Key _cameraDebugZoomInKey = Keyboard.Key.Q;
        protected Keyboard.Key _cameraDebugZoomOutKey = Keyboard.Key.E;
        protected Keyboard.Key _cameraDebugResetKey = Keyboard.Key.R;

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
                PositionalTree.Insert(gameObject);
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

            // Draw the Window as updated by the GameObjects.
            Game.RenderWindow.Display();
        }

        // This method lets Game objects respond to Collisions.
        private void HandleCollisions()
        {
            PositionalTree.HandleCollisions();

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
                if (Keyboard.IsKeyPressed(_cameraDebugForwardKey))
                {
                    Camera.Translate(new Vector2f(0, 500 * time.AsSeconds()));
                }
                if (Keyboard.IsKeyPressed(_cameraDebugLeftKey))
                {
                    Camera.Translate(new Vector2f(500 * time.AsSeconds(), 0));
                }
                if (Keyboard.IsKeyPressed(_cameraDebugBackwardKey))
                {
                    Camera.Translate(new Vector2f(0, -500 * time.AsSeconds()));
                }
                if (Keyboard.IsKeyPressed(_cameraDebugRightKey))
                {
                    Camera.Translate(new Vector2f(-500 * time.AsSeconds(), 0));
                }
                if (Keyboard.IsKeyPressed(_cameraDebugZoomInKey))
                {
                    Vector2f scale = Camera.Scale;
                    scale *= 1 + 0.5f * time.AsSeconds();
                    Camera.Scale = scale;
                }
                if (Keyboard.IsKeyPressed(_cameraDebugZoomOutKey))
                {
                    Vector2f scale = Camera.Scale;
                    scale /= 1 + 0.5f * time.AsSeconds();
                    Camera.Scale = scale;
                }
                if (Keyboard.IsKeyPressed(_cameraDebugResetKey))
                {
                    Camera.Scale = new Vector2f(1f, 1f);
                    Vector2f camPos = Camera.Position;
                    Camera.Translate(new Vector2f(-camPos.X, -camPos.Y));
                }
            }
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
                        // Delete it from the PositionalTree if it may be on the it.
                        PositionalTree.Delete(gameObject);
                    }
                    _gameObjects.RemoveAt(i);
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

            // Debug information in RecursiveDraw will draw over all gameObjects.
            if (_treeDebugMode)
            {
                PositionalTree.RecursiveDraw(Camera.Position);
            }

            // The bezels hide extra information if the player resizes the window to prevent them from gaining an unfair advantage.
            RectangleShape xBezel = Game.XBezel;
            RectangleShape yBezel = Game.YBezel;
            if (xBezel.Size != Game.NullSize)
            {
                float width = xBezel.Size.X;
                xBezel.Position = new Vector2f(-width, 0);
                Game.RenderWindow.Draw(xBezel);
                xBezel.Position = new Vector2f(width, 0);
                Game.RenderWindow.Draw(xBezel);
            }
            if (yBezel.Size != Game.NullSize)
            {
                float height = yBezel.Size.Y;
                yBezel.Position = new Vector2f(0, -height);
                Game.RenderWindow.Draw(yBezel);
                yBezel.Position = new Vector2f(0, Camera.Height);
                Game.RenderWindow.Draw(yBezel);
            }
        }

        // Modify the Sprite based on its absolute position (In PositionalTree) and the modifiers in Camera.
        public void UpdateCameraObject(Transformable transformable, Vector2f position)
        {
            Vector2f camScale = Camera.Scale;
            Vector2f newPosition = Camera.Position + position;
            newPosition.X *= camScale.X;
            newPosition.Y *= camScale.Y;
            transformable.Position = newPosition;
            transformable.Scale = Camera.Scale;
        }
    }
}
