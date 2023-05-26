using System;
using System.Collections.Generic;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GameEngine
{
    // The Game manages scenes and runs the main game loop.
    static class Game
    {
        // The number of frames that will be drawn to the screen in one second.
        private const int FramesPerSecond = 2147483647;
        //private const int FramesPerSecond = 60;

        // Set to true if you want the game to display the fps.
        private const bool ShowFPS = true;
        public static bool FPSShowing {
            get { return ShowFPS; }
        }

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
        private static VideoMode _videoMode;
        private static string _title;
        private static View _previousView;
        private static Styles _style;
        private static bool _isFullscreen;

        // These are for the fullscreen and force close keybinds
        private static bool _fullscreenKeyHeld;
        private static Keyboard.Key _closeKey;
        private static Keyboard.Key _fullscreenKey;

        // @TODO: Add more keyboard information (Whether each key is held, how long it's been held for, etc.)
        private static Timer _forceCloseTimer = new Timer(3000, false);

        // These bezels will be drawn around the window to prevent players from getting an unfair advantage by resizing the window.
        public static RectangleShape XBezel { get; set; }
        public static RectangleShape YBezel { get; set; }

        // This is used purely for resetting the size of old bezels.
        public static Vector2f NullSize { get; private set; }

        // A flag to prevent being initialized twice.
        private static bool _initialized;

        // A Random number generator we can use throughout the game. Seeded with a constant so 
        // the game plays the same every time for easy debugging.
        // @TODO: provide a method to randomize this for when they want variety.
        public static Random Random = new Random(42);

        // Creates our render window. Must be called once at startup.
        public static void Initialize(uint windowWidth, uint windowHeight, string windowTitle, Styles windowStyle, Color bezelColor, Keyboard.Key fullscrenKey, Keyboard.Key closeKey)
        {
            // Only initialize once.
            if (_initialized) return;
            _initialized = true;

            // Create the render window.
            _videoMode = new VideoMode(windowWidth, windowHeight);
            _title = windowTitle;
            _style = windowStyle;
            _window = new RenderWindow(_videoMode, windowTitle, windowStyle);
            _isFullscreen = false;
            _window.SetFramerateLimit(FramesPerSecond);

            // Set the fill color of the window bezels (Bezels are automatically added when the window is resized).
            XBezel = new RectangleShape();
            YBezel = new RectangleShape();
            XBezel.FillColor = bezelColor;
            YBezel.FillColor = bezelColor;
            NullSize = new Vector2f(0, 0);

            // Set the closeKey and fullscreen key
            _closeKey = closeKey;
            _fullscreenKey = fullscrenKey;

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
            if (_isFullscreen)
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
            _isFullscreen = true;
            _previousView = _window.GetView();

            // These variables determine if the engine needs to do stretch compensation (and how much it needs to compensate by).
            Vector2u fullScreenSize = new Vector2u(VideoMode.DesktopMode.Width, VideoMode.DesktopMode.Height);
            Vector2u prevSize = _window.Size;
            uint prevXSize = prevSize.X;
            uint prevYSize = prevSize.Y;
            float scaleXFactor = fullScreenSize.X / (float)prevXSize;
            float scaleYFactor = fullScreenSize.Y / (float)prevYSize;

            if (scaleXFactor == scaleYFactor)
            {
                // If the aspect ratio does not change, no need to compensate for SFML stretching.
                RenderWindow newWindow = new RenderWindow(_videoMode, _title, Styles.None, _window.Settings);
                newWindow.Position = new Vector2i(0, 0);
                _window.Close();
                _window = newWindow;
                _window.Size = fullScreenSize;

                // Reset the size of the bezels.
                XBezel.Size = new Vector2f();
                YBezel.Size = new Vector2f();
            }
            else if (scaleXFactor > scaleYFactor)
            {
                // If the aspect ratio changes such that y will be stretched, we need to stretch x by the same amount to correct for it.
                float stretchFactor = scaleXFactor / scaleYFactor;
                View gameView = _window.GetView();
                gameView.Size = new Vector2f(prevXSize * stretchFactor, prevYSize);

                // Close the window and reopen it in fullscreen.
                RenderWindow newWindow = new RenderWindow(_videoMode, _title, Styles.None, _window.Settings);
                newWindow.Position = new Vector2i(0, 0);
                _window.Close();
                _window = newWindow;
                _window.Size = fullScreenSize;
                _window.SetView(gameView);

                // Set the size of the bezel and reset the size of the old bezel.
                XBezel.Size = new Vector2f(prevXSize /** (stretchFactor - 1f)*/, prevYSize);
                YBezel.Size = NullSize;
            }
            else
            {
                // If the aspect ratio changes such that x will be stretched, we need to stretch y by the same amount to correct for it.
                float stretchFactor = scaleYFactor / scaleXFactor;
                View gameView = _window.GetView();
                gameView.Size = new Vector2f(prevXSize, prevYSize * stretchFactor);
                
                // Close the window and reopen it in fullscreen.
                RenderWindow newWindow = new RenderWindow(_videoMode, _title, Styles.None, _window.Settings);
                newWindow.Position = new Vector2i(0, 0);
                _window.Close();
                _window = newWindow;
                _window.Size = fullScreenSize;
                _window.SetView(gameView);

                // Set the size of the bezel and reset the size of the old bezel.
                YBezel.Size = new Vector2f(prevXSize, prevYSize /** (stretchFactor - 1f)*/);
                XBezel.Size = NullSize;
            }
        }
        public static void UnFullscreen()
        {
            _isFullscreen = false;
            RenderWindow newWindow = new RenderWindow(_videoMode, _title, _style);
            _window.Close();
            _window = newWindow;
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
                if (ShowFPS) { _currentScene.AddGameObject(new FPSDisplay()); }
            }
            else
            {
                _nextScene = scene;
            }
        }
        
        // Begins the main game loop with the initial scene.
        public static void Run()
        {
            Clock clock = new Clock();

            // Keep looping until the window closes.
            while (_window.IsOpen)
            {
                // If the next scene has been set, swap it with the current scene.
                if (_nextScene != null)
                {
                    _currentScene = _nextScene;
                    _nextScene = null;
                    clock.Restart();
                }

                // This is the time since the last frame.
                Time time = clock.Restart();

                // Handle fullscreen / force close key events
                if (Keyboard.IsKeyPressed(_closeKey))
                {
                    _forceCloseTimer.Update(time);
                    if (_forceCloseTimer.SurpassedTarget)
                    {
                        Game.RenderWindow.Close();
                    }
                }
                else
                {
                    _forceCloseTimer.Reset();
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
        }
    }
}
