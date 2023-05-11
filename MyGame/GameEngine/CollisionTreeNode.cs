using System.Collections.Generic;
using GameEngine;
using SFML.System;
using SFML.Graphics;

namespace GameEngine {
    // This is not a game object because its not meant to be inserted into the scene. It is a feature of the scene itself.
    class CollisionTreeNode {
        private List<CollidableObject> _objects = new List<CollidableObject>();
        public List<CollidableObject> Objects
        {
            get => _objects;
        }

        // The children are numbered based on the standard mathematical quadrant system.
        private CollisionTreeNode _child1 = null;
        private CollisionTreeNode _child2 = null;
        private CollisionTreeNode _child3 = null;
        private CollisionTreeNode _child4 = null;

        // The bounding box of this node.
        private FloatRect _bounds;

        // The drawable counterpart to bounds
        private RectangleShape _boundingBox;

        // The number of objects that can be stored in a node before the tree is split.
        private static int nodeCapacity = 4;

        // The thickness of the bounding box
        private static float borderThickness = 3f;

        public CollisionTreeNode(FloatRect bounds)
        {
            _bounds = bounds;
            _boundingBox = new RectangleShape(new Vector2f(_bounds.Width - 2 * borderThickness, _bounds.Height - 2 * borderThickness));
            _boundingBox.Position = new Vector2f(_bounds.Left + borderThickness, _bounds.Top + borderThickness);
            _boundingBox.OutlineThickness = borderThickness;
            _boundingBox.OutlineColor = Color.Green;
            _boundingBox.FillColor = Color.Transparent;
        }

        // Adds a collidable object to this node and splits the node if it exceeds capacity
        public void insert(CollidableObject collidableObject)
        {
            _objects.Add(collidableObject);
            if (_objects.Capacity > nodeCapacity)
            {
                split();
                _objects.Capacity = nodeCapacity;
            }
            return;
        }

        // Calls insert on all collidable objects in the provided list
        public void insertAll(List<CollidableObject> objects)
        {
            foreach (CollidableObject collidableObject in objects)
            {
                insert(collidableObject);
            }
        }

        public void handleCollisions(CollidableObject collidableObject)
        {
            
        }

        public void recursiveDraw() {
            Game.RenderWindow.Draw(_boundingBox);
            if (_child1 != null)
            {
                _child1.recursiveDraw();
                _child2.recursiveDraw();
                _child3.recursiveDraw();
                _child4.recursiveDraw();
            }
        }

        public void split()
        {
            Vector2f axis = new Vector2f((_bounds.Width - _bounds.Left) / 2f, (_bounds.Height - _bounds.Top) / 2f);
            if (_child1 == null)
            {
                Vector2f size = new Vector2f(_bounds.Width / 2f, _bounds.Height / 2f);
                _child1 = new CollisionTreeNode(new FloatRect(new Vector2f(_bounds.Left + size.X, _bounds.Top + size.Y), size));
                _child2 = new CollisionTreeNode(new FloatRect(new Vector2f(_bounds.Left, _bounds.Top + size.Y), size));
                _child3 = new CollisionTreeNode(new FloatRect(new Vector2f(_bounds.Left, _bounds.Top), size));
                _child4 = new CollisionTreeNode(new FloatRect(new Vector2f(_bounds.Left + size.X, _bounds.Top), size));
            }
            foreach (CollidableObject collidableObject in _objects)
            {
                Vector2f point = new Vector2f(collidableObject.GetCollisionRect().Left, collidableObject.GetCollisionRect().Top);
                insertInRightQuadrant(axis, collidableObject, point);
                point.X += collidableObject.GetCollisionRect().Width;
                point.Y += collidableObject.GetCollisionRect().Height;
                insertInRightQuadrant(axis, collidableObject, point);
            }
        }

        private void insertInRightQuadrant(Vector2f axis, CollidableObject collidableObject, Vector2f point) {
            if (point.X >= axis.X) {
                if (point.Y >= axis.Y) {
                    _child1.insert(collidableObject);
                } else {
                    _child4.insert(collidableObject);
                }
            } else {
                if (point.Y >= axis.Y) {
                    _child2.insert(collidableObject);
                } else {
                    _child4.insert(collidableObject);
                }
            }
        }
    }
}