using System.Collections.Generic;
using SFML.System;
using GameEngine;
using SFML.Graphics;
using System;

namespace GameEngine
{
    // This is not a game object because its not meant to be inserted into the scene. It is a feature of the scene itself.
    class PositionalTree
    {
        // Represents all collidable objects that cannot be fully contained in any of the child nodes.
        private List<Positional> _unsplittableObjects = new List<Positional>();

        // Represents all positional objects that can be fully contained in a child node.
        private List<Positional> _splittableObjects = new List<Positional>();
        
        // True if this is a leaf (is at the base of the tree).
        private bool _isLeaf;

        // True if this isLeaf and is empty.
        private bool _isEmptyLeaf;

        // The children are numbered based on the standard mathematical quadrant system.
        private PositionalTree _child1 = null;
        private PositionalTree _child2 = null;
        private PositionalTree _child3 = null;
        private PositionalTree _child4 = null;

        // The bounding box of this node.
        private FloatRect _bounds;

        // The axes that mark the splits among children.
        private Vector2f _splitAxes;

        // The drawable counterpart to bounds.
        private RectangleShape _boundingBox;

        // The number of objects that can be stored in a node before the tree is split.
        private static int nodeCapacity = 4;

        // The thickness of the bounding box.
        private static float borderThickness = 1f;

        public PositionalTree(FloatRect bounds)
        {
            _bounds = bounds;
            _splitAxes = new Vector2f(_bounds.Left + _bounds.Width / 2f, _bounds.Top + _bounds.Height / 2f);
            _boundingBox = new RectangleShape(new Vector2f(_bounds.Width - 2 * borderThickness, _bounds.Height - 2 * borderThickness));
            _boundingBox.Position = new Vector2f(_bounds.Left + borderThickness, _bounds.Top + borderThickness);
            _boundingBox.OutlineThickness = borderThickness;
            _boundingBox.OutlineColor = Game.DebugColor;
            _boundingBox.FillColor = Color.Transparent;
            _isLeaf = true;
            _isEmptyLeaf = true;
        }

        // Adds a splittable object to this node and splits the node if it exceeds capacity.
        public void insert(Positional positionalObject)
        {
            if (!_isLeaf && _child1._isEmptyLeaf && _child2._isEmptyLeaf && _child3._isEmptyLeaf && _child4._isEmptyLeaf)
            {
                _isLeaf = true;
            }
            if (_isLeaf)
            {
                if (!isSplittable(positionalObject))
                {
                    _unsplittableObjects.Add(positionalObject);
                }
                else
                {
                    _splittableObjects.Add(positionalObject);
                }
                _isEmptyLeaf = false;
                if (_splittableObjects.Count > nodeCapacity)
                {
                    split();
                }
            }
            else
            {
                insertInRightQuadrant(positionalObject);
            }
            return;
        }

        private void insertInRightQuadrant(Positional positionalObject) {
            Vector2f pos = positionalObject.Position;
            if (pos.X >= _splitAxes.X)
            {
                if (pos.Y >= _splitAxes.Y)
                {
                    _child1.insert(positionalObject);
                }
                else if (pos.Y > _bounds.Top - _bounds.Height)
                {
                    _child4.insert(positionalObject);
                }
            }
            else if (pos.X > _bounds.Left - _bounds.Width)
            {
                if (pos.Y >= _splitAxes.Y)
                {
                    _child2.insert(positionalObject);
                }
                else if (pos.Y > _bounds.Top - _bounds.Height)
                {
                    _child3.insert(positionalObject);
                }
            }
            else
            {
                _unsplittableObjects.Add(positionalObject);
            }
        }


        // Calls insert on all collidable objects in the provided list.
        public void insertAll(List<Positional> objects)
        {
            foreach (Positional collidableObject in objects)
            {
                insert(collidableObject);
            }
        }

        // Searches for an instance of an object and deletes it.
        public bool delete(Positional positionalObject)
        {
            if (_splittableObjects.Remove(positionalObject) || _unsplittableObjects.Remove(positionalObject))
            {
                if (_isLeaf && _splittableObjects.Count == 0)
                {
                    _isEmptyLeaf = true;
                }
                return true;
            }
            //deleteFromRightQuadrant(positionalObject);
            if (!_isLeaf)
            {
            return (_child1.delete(positionalObject) ||
            _child2.delete(positionalObject) ||
            _child3.delete(positionalObject) ||
            _child4.delete(positionalObject));
            }
            return false;
        }
        private void deleteFromRightQuadrant(Positional positionalObject)
        {
            Vector2f pos = positionalObject.Position;
            if (pos.X >= _splitAxes.X)
            {
                if (pos.Y >= _splitAxes.Y)
                {
                    _child1.delete(positionalObject);
                }
                else
                {
                    _child4.delete(positionalObject);
                }
            }
            else
            {
                if (pos.Y >= _splitAxes.Y)
                {
                    _child2.delete(positionalObject);
                }
                else
                {
                    _child3.delete(positionalObject);
                }
            }
        }

        public void move (Positional positionalObject, Vector2f newPos)
        {
            //delete(positionalObject);
            positionalObject.Position = newPos;
            //insert(positionalObject);
        }

        public void HandleCollisions()
        {
            
        }

        public void RecursiveDraw()
        {
            Game.RenderWindow.Draw(_boundingBox);
            foreach (Positional positionalObject in _splittableObjects)
            {
                FloatRect drawRect;
                if (positionalObject is Collidable)
                {
                    drawRect = ((Collidable)positionalObject).CollisionRect;
                }
                else
                {
                    continue;
                }
                RectangleShape drawableRect = new RectangleShape(new Vector2f(drawRect.Width - 2 * borderThickness, drawRect.Height - 2 * borderThickness));
                drawableRect.Position = new Vector2f(drawRect.Left + borderThickness, drawRect.Top + borderThickness);
                drawableRect.OutlineThickness = borderThickness;
                drawableRect.OutlineColor = Game.DebugColor;
                drawableRect.FillColor = Color.Transparent;
                Game.RenderWindow.Draw(drawableRect);
            }
            if (!_isLeaf)
            {
                _child1.RecursiveDraw();
                _child2.RecursiveDraw();
                _child3.RecursiveDraw();
                _child4.RecursiveDraw();
            }
        }

        public void split()
        {
            if (_child1 == null)
            {
                Vector2f size = new Vector2f(_bounds.Width / 2f, _bounds.Height / 2f);
                _child1 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left + size.X, _bounds.Top + size.Y), size));
                _child2 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left, _bounds.Top + size.Y), size));
                _child3 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left, _bounds.Top), size));
                _child4 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left + size.X, _bounds.Top), size));
            }
            foreach (Positional splittableObject in _splittableObjects)
            {
                insertInRightQuadrant(splittableObject);
            }
            _splittableObjects.Clear();
            _isLeaf = false;
        }

        private bool isSplittable(Positional positionalObject)
        {
            if (positionalObject is not Collidable)
            {
                return true;
            }
            FloatRect collisionRect = ((Collidable)positionalObject).CollisionRect;
            return !(collisionRect.Left < _splitAxes.X ^ collisionRect.Left + collisionRect.Width < _splitAxes.X)
                    && !(collisionRect.Top < _splitAxes.Y ^ collisionRect.Top + collisionRect.Height < _splitAxes.Y);
        }
    }
}