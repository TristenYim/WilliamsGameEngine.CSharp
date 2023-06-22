using System;
using System.Collections.Generic;
using System.Diagnostics;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GameEngine
{
    // The Game manages scenes and runs the main game loop.
    static class Game
    {
        // This is the internal clock which is used to calculate the Time since the last frame and to manage timers.
        private static Stopwatch _gameClock;
        
        // The current time since _gameClock was started in milliseconds.
        public static double CurrentTimeMS
        {
            get => _gameClock.Elapsed.TotalMilliseconds;
        }
        
        // The maximum number of frames that will be drawn to the screen in one second.
        private const int FramesPerSecond = 2147483647;
        //private const int FramesPerSecond = 60;

        // Set to true if you want the game to display the fps.
        public static bool FPSShowing { get; private set; }

        // We keep a current and next scene so the scene can be changed mid-frame.
        private static Scene _currentScene;
        private static Scene _nextScene;

        // Cached textures
        private static readonly Dictionary<string, Texture> Textures = new Dictionary<string, Texture>();

        // Cached sounds
        private static readonly Dictionary<string, SoundBuffer> Sounds = new Dictionary<string, SoundBuffer>();

        // Cached fonts
        private static readonly Dictionary<string, Font> Fonts = new Dictionary<string, Font>();

        // The window we will draw to.
        private static RenderWindow _window;

        // These are for storing window settings when switching between fullscreen and default style.
        public static VideoMode VideoMode { get; private set; }
        private static string _title;
        private static Styles _style;
        public static bool IsFullscreen { get; private set; }

        // These are for the fullscreen and force close keybinds
        private static bool _fullscreenKeyHeld;
        private static Keyboard.Key _closeKey;
        private static Keyboard.Key _fullscreenKey;

        // @TODO: Add more keyboard information (Whether each key is held, how long it's been held for, etc.)
        private static Timer _forceCloseTimer;

        // A flag to prevent being initialized twice.
        private static bool _initialized;

        // A Random number generator we can use throughout the game. Seeded with a constant so 
        // the game plays the same every time for easy debugging.
        // @TODO: provide a method to randomize this for when they want variety.
        public static Random Random = new Random(42);

        // Creates our render window. Must be called once at startup.
        public static void Initialize(uint windowWidth, uint windowHeight, string windowTitle, Styles windowStyle, Keyboard.Key fullscrenKey, Keyboard.Key closeKey)
        {
            // Only initialize once.
            if (_initialized) 
            {
                return;
            }
            _initialized = true;

            // Create the render window.
            VideoMode = new VideoMode(windowWidth, windowHeight);
            _title = windowTitle;
            _style = windowStyle;
            _window = new RenderWindow(VideoMode, windowTitle, windowStyle);
            _window.SetFramerateLimit(FramesPerSecond);
            IsFullscreen = false;

            // Set the closeKey and fullscreen key.
            _closeKey = closeKey;
            _fullscreenKey = fullscrenKey;

            // Setup clock and internal timers.
            _gameClock = new Stopwatch();
            _forceCloseTimer = new Timer(1000.0);

            // Add a method to be called whenever the "Closed" event fires.
            _window.Closed += ClosedEventHandler;
        }

        // Called whenever you try to close the game window.
        private static void ClosedEventHandler(object sender, EventArgs e)
        {
            // This indicates we should close the window, so just do that.
            _window.Close();
        }

        // Returns a reference to the game's RenderWindow.
        public static RenderWindow RenderWindow
        {
            get { return _window; }
        }

        // These set the fullscreen state of the window (and correct for SFML stretching).
        public static void ToggleFullScreen()
        {
            if (IsFullscreen)
            {
                UnFullscreen();
            }
            else
            {
                MakeFullscreen();
            }
        }
        public static void MakeFullscreen()
        {
            IsFullscreen = true;
            _window.Close();
            _window = new RenderWindow(VideoMode, _title, Styles.None, _window.Settings);
            _window.Position = new Vector2i(0, 0);
            _window.Size = new Vector2u(VideoMode.DesktopMode.Width, VideoMode.DesktopMode.Height);
        }
        public static void UnFullscreen()
        {
            IsFullscreen = false;
            _window.Close();
            _window = new RenderWindow(VideoMode, _title, _style, _window.Settings);
        }

        // Get a texture (pixels) from a file
        public static Texture GetTexture(string fileName)
        {
            Texture texture;

            // Getting a texture from cached textures is much faster than from a file, so the engine gets from cache whenever possible.
            if (Textures.TryGetValue(fileName, out texture)) return texture;

            texture = new Texture(fileName);
            Textures[fileName] = texture;
            return texture;
        }

        // Get a sound from a file
        public static SoundBuffer GetSoundBuffer(string fileName)
        {
            SoundBuffer soundBuffer;

            if (Sounds.TryGetValue(fileName, out soundBuffer)) return soundBuffer;

            soundBuffer = new SoundBuffer(fileName);
            Sounds[fileName] = soundBuffer;
            return soundBuffer;
        }

        // Get a font from a file
        public static Font GetFont(string fileName)
        {
            Font font;

            if (Fonts.TryGetValue(fileName, out font)) return font;

            font = new Font(fileName);
            Fonts[fileName] = font;
            return font;
        }

        // Returns the active running scene.
        public static Scene CurrentScene
        {
            get => _currentScene;
        }

        // Specifies the next Scene to run.
        public static void SetScene(Scene scene)
        {
            // If we don't have a current scene, set it.
            // Otherwise, note the next scene.
            if (_currentScene == null)
            {
                _currentScene = scene;

                // If we set showFPS to true, add an fps Object to the new scene.
                //if (FPSShowing) { _currentScene.AddGameObject(new FPSDisplay()); }
            }
            else
            {
                _nextScene = scene;
            }
        }
        
        // Begins the main game loop with the initial scene.
        public static void Run()
        {
            _gameClock.Start();
            double _previousMS = _gameClock.Elapsed.TotalMilliseconds;

            // Keep looping until the window closes.
            while (_window.IsOpen)
            {
                // If the next scene has been set, swap it with the current scene.
                if (_nextScene != null)
                {
                    _currentScene = _nextScene;
                    _nextScene = null;
                    _gameClock.Restart();
                }

                // This is the time since the last frame.
                Time time = Time.FromMicroseconds((long)(1000 * (_gameClock.Elapsed.TotalMilliseconds - _previousMS)));
                _previousMS = _gameClock.Elapsed.TotalMilliseconds;

                // Handle fullscreen / force close key events
                if (Keyboard.IsKeyPressed(_closeKey))
                {
                    if (_forceCloseTimer.IsExpired)
                    {
                        Game.RenderWindow.Close();
                    }
                }
                else
                {
                    _forceCloseTimer.Restart();
                }

                if (Keyboard.IsKeyPressed(_fullscreenKey))
                {
                    if (!_fullscreenKeyHeld)
                    {
                        Game.ToggleFullScreen();
                    }
                    _fullscreenKeyHeld = true;
                }
                else
                {
                    _fullscreenKeyHeld = false;
                }

                // Update the scene with the time since the last frame.
                _currentScene.Update(time);
            }

            _gameClock.Stop();
        }
    }
}
