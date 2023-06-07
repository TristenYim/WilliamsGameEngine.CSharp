using GameEngine;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using System.Collections.Generic;

namespace MyGame
{
    /*class Explosion : AnimatedSprite
    {
        private readonly Sound _boom = new Sound();
        public Explosion(Vector2f pos) : base(pos)
        {
            Texture = Game.GetTexture("Resources/explosion-spritesheet.png");
            SetUpExplosionAnimation();
            PlayAnimation("explosion", AnimationMode.OnceForwards);

            _boom.SoundBuffer = Game.GetSoundBuffer("Resources/boom.wav");
            _boom.Play();
        }

        public override void Update(Time elapsed)
        {
            base.Update(elapsed);

            if (!IsPlaying())
            {
                MakeDead();
            }
        }

        private void SetUpExplosionAnimation()
        {
            var frames = new List<IntRect>
            {
                new IntRect(  0, 0, 64, 64),    // frame 1
                new IntRect( 64, 0, 64, 64),    // frame 2
                new IntRect(128, 0, 64, 64),    // frame 3
                new IntRect(192, 0, 64, 64),    // frame 4
                new IntRect(256, 0, 64, 64),    // frame 5
                new IntRect(320, 0, 64, 64),    // frame 6
                new IntRect(384, 0, 64, 64),    // frame 7
                new IntRect(448, 0, 64, 64),    // frame 8
                new IntRect(512, 0, 64, 64)    // frame 9
            };
            AddAnimation("explosion", frames);
        }
    }*/

    class Explosion : GameObject
    {
        private AnimatedSprite _animatedSprite;
        private readonly Sound _boom = new Sound();
        public Explosion(Vector2f pos)
        {
            _animatedSprite = new AnimatedSprite(Game.GetTexture("Resources/explosion-spritesheet.png"), new Vector2i(64, 64), 20);
            _animatedSprite.Position = pos;

            _boom.SoundBuffer = Game.GetSoundBuffer("Resources/boom.wav");
            _boom.Play();
        }
        public override void Update(Time elapsed)
        {
            _animatedSprite.Update(elapsed);
            Game.CurrentScene.Cameras[GameScene.GameCameraIndex].DrawQueue.Enqueue(this);
            Game.CurrentScene.Cameras[GameScene.MiniMapCameraIndex].DrawQueue.Enqueue(this);
        }
        public override void Draw()
        {
            Game.RenderWindow.Draw(_animatedSprite);
        }
    }
}
