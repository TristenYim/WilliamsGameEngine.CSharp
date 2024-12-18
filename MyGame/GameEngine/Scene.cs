﻿using System.Collections.Generic;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using GameEngine;

namespace GameEngine
{
    // The Scene manages all the GameObjects currently in the game.
    abstract class Scene
    {
        // This holds our GameObjects.
        private readonly List<GameObject> _gameObjects = new List<GameObject>();

        // This is a 2D space of GameObjects which can be (but does not have to be) bounded by the Window.
        public SpatialTree SpatialTree { get; protected set; }

        // This is the list of Cameras, which both act as render layers and allow for separate perspective control. Cameras are rendered first-to-last.
        public Camera[] Cameras { get; protected set; }

        // This is the index of the current Camera that the Camera debug controls apply to.
        protected int _currentCam;

        // Set to true to make WASD shift the Camera.
        protected bool _cameraDebugMode = false;

        // These are the keybinds for the Camera debug movements.
        protected Keyboard.Key _cameraDebugUpKey = Keyboard.Key.W;
        protected Keyboard.Key _cameraDebugLeftKey = Keyboard.Key.A;
        protected Keyboard.Key _cameraDebugDownKey = Keyboard.Key.S;
        protected Keyboard.Key _cameraDebugRightKey = Keyboard.Key.D;
        protected Keyboard.Key _cameraDebugClockwiseKey = Keyboard.Key.E;
        protected Keyboard.Key _cameraDebugCounterclockwiseKey = Keyboard.Key.Q;
        protected Keyboard.Key _cameraDebugZoomInKey = Keyboard.Key.C;
        protected Keyboard.Key _cameraDebugZoomOutKey = Keyboard.Key.X;
        protected Keyboard.Key _cameraDebugResetKey = Keyboard.Key.R;

        // TODO: Add culling
        // Does not draw offscreen elements (except for bounding boxes drawn by SpatialTree) if true.
        //protected bool _cullingEnabled = true;

        // Puts a GameObject into the scene and the SpatialTree if appropriate.
        public void AddGameObject(GameObject gameObject)
        {
            // This adds the game object onto the back (the end) of the list of game objects.
            //_gameObjects.AddLast(gameObject);
            //gameObject.ListNodePointer = _gameObjects.Last;
            _gameObjects.Add(gameObject);

            // This adds the game object into the SpatialTree if it belongs on the tree.
            if (gameObject.Position != null)
            {
                SpatialTree.Insert(gameObject);
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
            SpatialTree.HandleCollisions();

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
                if (Keyboard.IsKeyPressed(_cameraDebugResetKey))
                {
                    // If the reset key is held, nothing else needs to be processed
                    Cameras[_currentCam].Reset();
                }
                else
                {
                    // We store the deltas to reduce the number of method calls.
                    Vector2f posDelta = new Vector2f();
                    float rotationDelta = 0f;
                    float zoomFactor = 1f;

                    if (Keyboard.IsKeyPressed(_cameraDebugUpKey))
                    {
                        posDelta.Y -= Cameras[_currentCam].View.Size.Y * 0.75f * time.AsSeconds();
                    }
                    if (Keyboard.IsKeyPressed(_cameraDebugLeftKey))
                    {
                        posDelta.X -= Cameras[_currentCam].View.Size.X * 0.75f * time.AsSeconds();
                    }
                    if (Keyboard.IsKeyPressed(_cameraDebugDownKey))
                    {
                        posDelta.Y += Cameras[_currentCam].View.Size.Y * 0.75f * time.AsSeconds();
                    }
                    if (Keyboard.IsKeyPressed(_cameraDebugRightKey))
                    {
                        posDelta.X += Cameras[_currentCam].View.Size.Y * 0.75f * time.AsSeconds();
                    }
                    if (Keyboard.IsKeyPressed(_cameraDebugClockwiseKey))
                    {
                        rotationDelta += 60 * time.AsSeconds();
                    }
                    if (Keyboard.IsKeyPressed(_cameraDebugCounterclockwiseKey))
                    {
                        rotationDelta -= 60 * time.AsSeconds();
                    }
                    if (Keyboard.IsKeyPressed(_cameraDebugZoomInKey))
                    {
                        zoomFactor -= 0.5f * time.AsSeconds();
                    }
                    if (Keyboard.IsKeyPressed(_cameraDebugZoomOutKey))
                    {
                        zoomFactor += 0.5f * time.AsSeconds();
                    }

                    // By storing the deltas, we only have to make 3 calls no matter how many different ways we move.
                    Cameras[_currentCam].View.Move(posDelta);
                    Cameras[_currentCam].View.Rotate(rotationDelta);
                    Cameras[_currentCam].View.Zoom(zoomFactor);
                }
            }
        }

        // This method removes GameObjects that indicate they are dead from the Scene.
        private void RemoveDeadGameObjects()
        {
            // Index game objects and remove them if they are dead.
            for (int i = _gameObjects.Count - 1; i > -1; i--)
            {
                if (_gameObjects[i].IsDead())
                {
                    if (_gameObjects[i].Position != null)
                    {
                        // Delete it from the SpatialTree if it may be on the it.
                        SpatialTree.Delete(_gameObjects[i]);
                    }
                    _gameObjects.RemoveAt(i);
                }
            }
        }

        // This method calls draw on each of our game objects.
        private void DrawGameObjects()
        {
            for (int i = 0; i < Cameras.Length; i++)
            {
                Cameras[i].Draw();
            }
            Debug.Draw();
        }
    }
}
