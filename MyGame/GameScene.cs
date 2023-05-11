using GameEngine;
using SFML.System;

namespace MyGame
{
    class GameScene : Scene
    {
        private int _lives = 3;
        private int _score;
        public GameScene()
        {
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