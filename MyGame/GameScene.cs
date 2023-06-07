﻿using GameEngine;
using SFML.Graphics;

namespace MyGame
{
    class GameScene : Scene
    {
        public const int GameCameraIndex = 0;
        public const int MiniMapCameraIndex = 1;
        public GameScene()
        {
            _cameraDebugMode = true;
            _treeDebugMode = false;

            SpatialTree = new SpatialTree(new FloatRect(0, 0, Game.RenderWindow.Size.X, Game.RenderWindow.Size.Y), null, GameCameraIndex);
            _currentCam = 0;
            Cameras = new Camera[3];
            Cameras[0] = new Camera(new FloatRect(0, 0, Game.RenderWindow.Size.X, Game.RenderWindow.Size.Y));
            Cameras[1] = new Camera(new FloatRect(-24, -300, Game.RenderWindow.Size.X * 8f, Game.RenderWindow.Size.Y * 8f));
            Cameras[2] = new Camera(new FloatRect(0, 0, Game.RenderWindow.Size.X, Game.RenderWindow.Size.Y));

            Ship ship = new Ship();
            AddGameObject(ship);
            // a couple lines to test the meteor
            //Meteor meteor = new Meteor(new Vector2f(650,250));
            //AddGameObject(meteor);

            MeteorSpawner meteorSpawner = new MeteorSpawner();
            AddGameObject(meteorSpawner);
        }
    }
}