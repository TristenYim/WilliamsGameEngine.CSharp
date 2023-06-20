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
        public static bool DrawSpatialTreeEnabled { get; set; }
        public static bool FPSDisplayEnabled { get; set; }
        private static SpatialTree _treeToDraw;
        private static RectangleShape _treeBoundingBox;
        private static Time _time;
        private static float _startTime;
        private static int _totalFrames;
        public static void InitializeDrawSpatialTree(SpatialTree tree, Color treeBoundingBoxColor, int treeBoundingBoxThickness)
        {
            _treeToDraw = tree;
            _treeBoundingBox = new RectangleShape();
            _treeBoundingBox.OutlineColor = treeBoundingBoxColor;
            _treeBoundingBox.OutlineThickness = treeBoundingBoxThickness;
            _treeBoundingBox.FillColor = Color.Transparent;
        }
        public static void InitializeFPSDisplay()
        {
            _time = new Time();
            _startTime = _time.AsSeconds();
            _totalFrames = 0;
            System.Console.WriteLine(_startTime);
        }
        public static void AddFrame()
        {
            _totalFrames++;
        }
        private static void DrawSpatialTree(SpatialTree tree)
        {
            if (DrawSpatialTreeEnabled)
            {
                ThrowExceptionIfNotInitialized(_treeBoundingBox, "Draw Spatial Tree");
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
        public static void Draw()
        {
            DrawSpatialTree(_treeToDraw);
        }
        private static void ThrowExceptionIfNotInitialized(object varToCheck, string action)
        {
            if (varToCheck == null)
            {
                throw new Exception("Error: Tried to '" + action + "' before it was initialized!");
            }
        }
    }
}