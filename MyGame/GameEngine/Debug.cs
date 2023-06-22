using System;
using System.Collections.Generic;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace GameEngine
{
    static class Debug
    {
        // FPS display members:
        private static bool _fpsDisplayEnabled;
        public static bool FPSDisplayEnabled { 
            get => _fpsDisplayEnabled;
            set
            {
                if (value && _fpsText == null)
                {
                    ThrowDebugException("FPS Displaying");
                }
                _fpsDisplayEnabled = value;
            }
        }

        // This is time when the frames started getting counted or were last set to 0.
        private static double _fpsStartMS;

        // The total number of frames counted since _fpsStartMS
        private static int _totalFrames;

        // This is the FPS Text that gets drawn to the screen
        private static Text _fpsText;

        // The View the FPS counter draws to.
        private static View _fpsView;

        // Spatial tree drawing members:
        private static bool _drawSpatialTreeEnabled;
        public static bool DrawSpatialTreeEnabled { 
            get => _drawSpatialTreeEnabled;
            set
            {
                if (value && TreeToDraw == null)
                {
                    ThrowDebugException("Spatial Tree Drawing");
                }
                _drawSpatialTreeEnabled = value;
            }
        }

        // The SpatialTree that gets drawn.
        public static SpatialTree TreeToDraw { private get; set; }

        // This is the RectangleShape of the bounding box of the SpatialTree that gets drawn.
        private static RectangleShape _treeBoundingBox;

        // The View the SpatialTree bounding box gets drawn to.
        private static View _spatialTreeView;
        public static void InitializeDrawSpatialTree(SpatialTree tree, Color treeBoundingBoxColor, int treeBoundingBoxThickness, View view)
        {
            TreeToDraw = tree;
            _treeBoundingBox = new RectangleShape();
            _treeBoundingBox.OutlineColor = treeBoundingBoxColor;
            _treeBoundingBox.OutlineThickness = treeBoundingBoxThickness;
            _treeBoundingBox.FillColor = Color.Transparent;
            _spatialTreeView = view;
        }
        private static void DrawSpatialTree(SpatialTree tree)
        {
            if (_drawSpatialTreeEnabled)
            {
                Game.RenderWindow.SetView(_spatialTreeView);
                _treeBoundingBox.Position = new Vector2f(tree.LeftBound + _treeBoundingBox.OutlineThickness, tree.TopBound + _treeBoundingBox.OutlineThickness);
                _treeBoundingBox.Size = new Vector2f(tree.RightBound - tree.LeftBound - _treeBoundingBox.OutlineThickness * 2, tree.BottomBound - tree.TopBound - _treeBoundingBox.OutlineThickness * 2);
                Game.RenderWindow.Draw(_treeBoundingBox);
                if (!tree.IsLeaf && tree.Child1 != null)
                {
                    DrawSpatialTree(tree.Child1);
                    DrawSpatialTree(tree.Child2);
                    DrawSpatialTree(tree.Child3);
                    DrawSpatialTree(tree.Child4);
                }
            }
        }
        public static void InitializeFPSDisplay(View fpsView)
        {
            _fpsStartMS = Game.CurrentTimeMS;
            _totalFrames = 0;
            _fpsText = new Text("", Game.GetFont("Resources/Courneuf-Regular.ttf"), 36);
            _fpsText.FillColor = Color.Yellow;
            _fpsText.Position = new Vector2f(10, 10);
            _fpsView = fpsView;
        }
        private static void DrawFPS()
        {
            _totalFrames++;

            // This ensures the FPS is always mostly up-to-date by setting the frame count to 0 once every second.
            if (_fpsDisplayEnabled && Game.CurrentTimeMS - 1000.0 > _fpsStartMS)
            {
                _fpsText.DisplayedString = "FPS: " + decimal.Round((decimal)((double)_totalFrames / (Game.CurrentTimeMS - _fpsStartMS) * 1000.0), 2);
                _fpsStartMS = Game.CurrentTimeMS;
                _totalFrames = 0;
            }

            Game.RenderWindow.SetView(_fpsView);
            Game.RenderWindow.Draw(_fpsText);
        }

        // Calls all the draw methods for each debug feature.
        public static void Draw()
        {
            DrawFPS();
            DrawSpatialTree(TreeToDraw);
        }

        // These exceptions are thrown when you try to enable a debug feature without initializing it.
        private static void ThrowDebugException(string feature)
        {
            throw new Exception("Error: Tried to enable '" + feature + "' before it was initialized!");
        }
    }
}