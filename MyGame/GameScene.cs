using GameEngine;
using SFML.Graphics;

namespace MyGame
{
    class GameScene : Scene
    {
        private int _lives = 3;
        private int _score;
        public GameScene()
        {
            _cameraDebugMode = true;

            _positionalObjects = new PositionalTree(new FloatRect(0, 0, Game.RenderWindow.Size.X, Game.RenderWindow.Size.Y), null);
            _camera = new Camera(new FloatRect(0, 0, Game.RenderWindow.Size.X, Game.RenderWindow.Size.Y));

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