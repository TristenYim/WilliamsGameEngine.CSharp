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

        // The children are numbered based on the standard mathematical quadrant system.
        private PositionalTree _child1 = null;
        private PositionalTree _child2 = null;
        private PositionalTree _child3 = null;
        private PositionalTree _child4 = null;

        // The bounding box of this node.
        private readonly FloatRect _bounds;

        // The axes that mark the splits among children, used to insert an object into the correct child.
        private readonly Vector2f _splitAxes;

        // The drawable counterpart to bounds, used for drawing the bounding box when debugging.
        private readonly RectangleShape _boundingBox;

        // The number of objects that can be stored in a node before the tree is split.
        private const int NodeCapacity = 4;

        // The thickness of the bounding box and object boxes.
        private const float BorderThickness = 1f;

        // The color of the tree bounding boxes.
        private static readonly Color TreeBorderColor = Color.Cyan;

        // The color of the object bounding boxes.
        private static readonly Color ObjectBorderColor = Color.Green;

        // Constructs the positional tree based on the given bounds.
        public PositionalTree(FloatRect bounds)
        {
            _bounds = bounds;
            _splitAxes = new Vector2f(_bounds.Left + _bounds.Width / 2f, _bounds.Top + _bounds.Height / 2f);
            
            // Sets up the drawable bounding box
            _boundingBox = new RectangleShape(new Vector2f(_bounds.Width - 2 * BorderThickness, _bounds.Height - 2 * BorderThickness));
            _boundingBox.Position = new Vector2f(_bounds.Left + BorderThickness, _bounds.Top + BorderThickness);
            _boundingBox.OutlineThickness = BorderThickness;
            _boundingBox.OutlineColor = TreeBorderColor;
            _boundingBox.FillColor = Color.Transparent;
        }

        // Adds a positional object into the right part of this tree and splits the tree if necessary.
        public void insert(Positional positionalObject) {
            // If the object is not splitable, it automatically gets put in unsplitable objects and nothing else needs to be done.
            if (!isSplittable(positionalObject))
            {
                _unsplittableObjects.Add(positionalObject);
                return;
            }
            if (isLeaf())
            {
                // If it needs to split, it will split. Otherwise it add the object to splittable objects to be split later.
                if (_splittableObjects.Count >= NodeCapacity)
                {
                    Split();
                } else {
                    _splittableObjects.Add(positionalObject);
                    return;
                }
            }
            // If this got split or it is not a leaf, the object needs to be added to the right child.
            InsertInRightQuadrant(positionalObject);
        }

        // Adds a positional object into the correct child.
        private void InsertInRightQuadrant(Positional positionalObject)
        {
            Vector2f pos = positionalObject.Position;
            if (pos.X >= _splitAxes.X && pos.X <= _bounds.Left + _bounds.Width)
            {
                if (pos.Y >= _splitAxes.Y && pos.Y <= _bounds.Top + _bounds.Height)
                {
                    // Goes to child1 if +X and +Y in reltation to the split axes.
                    _child1.insert(positionalObject);
                    return;
                }
                else if (pos.Y >= _bounds.Top)
                {
                    // Goes to child4 if +X and -Y in relation to the split axes.
                    _child4.insert(positionalObject);
                    return;
                }
            }
            else if (pos.X >= _bounds.Left)
            {
                if (pos.Y >= _splitAxes.Y && pos.Y <= _bounds.Top + _bounds.Height)
                {
                    // Goes to child2 if -X and +Y in relation to the split axes.
                    _child2.insert(positionalObject);
                    return;
                }
                else if (pos.Y >= _bounds.Top)
                {
                    // Goes to child3 if -X and -Y in relation to the split axes.
                    _child3.insert(positionalObject);
                    return;
                }
            }

            // Currently, if the object for whatever reason cannot be inserted into any of these areas, it is marked as an unsplittable object.
            // By uncommenting the exception, you can see that this sometimes leads to objects on the borders being added as unsplittable objects.
            // TODO: Fix the problem mentioned above.
            _unsplittableObjects.Add(positionalObject);
            /*throw new Exception("Could not insert object within bounds between (" + _bounds.Left + ", " + _bounds.Top + ") and (" 
                                + (_bounds.Left + _bounds.Width) + ", " + (_bounds.Top + _bounds.Height) + ")");*/
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
                return true;
            }
            else if (!isLeaf())
            {
                return _child1.delete(positionalObject) || _child2.delete(positionalObject) || _child3.delete(positionalObject) || _child4.delete(positionalObject);
            }
            return false;
        }

        // Simply deletes the object, changes its position, and reinserts it, guaranteeing it will be moved into the correct part of the tree.
        public void move (Positional positionalObject, Vector2f newPos)
        {
            delete(positionalObject);
            positionalObject.Position = newPos;
            insert(positionalObject);
        }

        // Handles the collisions of all objects in the entire tree.
        // This is implemented using something called recursion, where a method repeatedly calls itself.
        // In this case, the node handles its own collisions then divides a list of objects that check for collisions into four separate lists.
        // Each check list only contains objects that intersect its corresponding child node.
        // This makes checking for collisions O(n log(n)) rather than O(n^2).
        public void HandleCollisions()
        {
            HandleCollisions(new LinkedList<Collidable>());
        }
        private void HandleCollisions(LinkedList<Collidable> checkList)
        {
            // Handle collisions in splittable objects.
            HandleCollisionsInList(_splittableObjects, checkList);

            // Handle collisions in unsplittable objects.
            HandleCollisionsInList(_unsplittableObjects, checkList);

            // If this is a leaf, you're done checking for collisions.
            if (isLeaf())
            {
                return;
            }

            // Divide the check list into a separte check list for each child.
            // Each check list only contains objects within the bounds of the child.
            FloatRect child1Bounds = _child1._bounds;
            FloatRect child2Bounds = _child2._bounds;
            FloatRect child3Bounds = _child3._bounds;
            FloatRect child4Bounds = _child4._bounds;
            LinkedList<Collidable> child1CheckList = new LinkedList<Collidable>();
            LinkedList<Collidable> child2CheckList = new LinkedList<Collidable>();
            LinkedList<Collidable> child3CheckList = new LinkedList<Collidable>();
            LinkedList<Collidable> child4CheckList = new LinkedList<Collidable>();
            foreach(Collidable collidable in checkList)
            {
                if (collidable.CollisionRect.Intersects(child1Bounds))
                {
                    child1CheckList.AddLast(collidable);
                }
                if (collidable.CollisionRect.Intersects(child2Bounds))
                {
                    child2CheckList.AddLast(collidable);
                }
                if (collidable.CollisionRect.Intersects(child3Bounds))
                {
                    child3CheckList.AddLast(collidable);
                }
                if (collidable.CollisionRect.Intersects(child4Bounds))
                {
                    child4CheckList.AddLast(collidable);
                }
            }

            // Handle the collisions of each child.
            _child1.HandleCollisions(child1CheckList);
            _child2.HandleCollisions(child2CheckList);
            _child3.HandleCollisions(child3CheckList);
            _child4.HandleCollisions(child4CheckList);
        }

        // Each object in handle list is checked against check list and then added to check list if it is checking for collisions.
        private void HandleCollisionsInList(List<Positional> handleList, LinkedList<Collidable> checkList)
        {
            foreach (var positional in handleList)
            {
                Collidable collidable;
                
                // Don't check for collisions if the object isn't even collidable
                if (positional is Collidable)
                {
                    collidable = (Collidable)positional;
                }
                else
                {
                    continue;
                }
                // Check for collisions
                foreach(Collidable collidableThatChecks in checkList)
                {
                    if (collidable.CollisionRect.Intersects(collidableThatChecks.CollisionRect))
                    {
                        collidableThatChecks.HandleCollision(collidable);
                        collidable.HandleCollision(collidableThatChecks);
                    }
                }

                // Add this to the check list if this checks for collisions
                if (collidable.ChecksForCollisions)
                {
                    checkList.AddLast(collidable);
                }
            }
        }

        // Constructs the children if they are null and moves all splittable objects into their correct quadrant
        public void Split()
        {
            if (_child1 == null)
            {
                Vector2f size = new Vector2f(_bounds.Width / 2f, _bounds.Height / 2f);
                _child1 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left + size.X, _bounds.Top + size.Y), size));
                _child2 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left, _bounds.Top + size.Y), size));
                _child3 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left, _bounds.Top), size));
                _child4 = new PositionalTree(new FloatRect(new Vector2f(_bounds.Left + size.X, _bounds.Top), size));
            }
            for (int i = _splittableObjects.Count - 1; i > -1; i--)
            {
                InsertInRightQuadrant(_splittableObjects[i]);
            }
            _splittableObjects.Clear();
        }

        // Checks if this is a leaf by determining if the children are empty
        private bool isLeaf()
        {
            return _child1 == null || _child1.isEmpty() && _child2.isEmpty() && _child3.isEmpty() && _child4.isEmpty();
        }

        // Recursively checks if all linked children are empty.
        private bool isEmpty()
        {
            return _splittableObjects.Count == 0 && _unsplittableObjects.Count == 0 && _child1 == null || 
                   _splittableObjects.Count == 0 && _unsplittableObjects.Count == 0 && _child1.isEmpty() && 
                   _child2.isEmpty() && _child3.isEmpty() && _child4.isEmpty();
        }

        // Checks if an object is splitable by determining if either of the axes intersect it.
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

        // Draws a box around the bounds of this nodes and all non-empty children, recursively.
        public void RecursiveDraw()
        {
            if (!isEmpty())
            {
                Game.RenderWindow.Draw(_boundingBox);
            }
            DrawObjectCollisionBoxes();
            if (!isLeaf())
            {
                _child1.RecursiveDraw();
                _child2.RecursiveDraw();
                _child3.RecursiveDraw();
                _child4.RecursiveDraw();
            }
        }

        // Draws a box around the collision box of each object in this node.
        private void DrawObjectCollisionBoxes()
        {
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
                RectangleShape drawableRect = new RectangleShape(new Vector2f(drawRect.Width - 2 * BorderThickness, drawRect.Height - 2 * BorderThickness));
                drawableRect.Position = new Vector2f(drawRect.Left + BorderThickness, drawRect.Top + BorderThickness);
                drawableRect.OutlineThickness = BorderThickness;
                drawableRect.OutlineColor = ObjectBorderColor;
                drawableRect.FillColor = Color.Transparent;
                Game.RenderWindow.Draw(drawableRect);
            }
            foreach (Positional positionalObject in _unsplittableObjects)
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
                RectangleShape drawableRect = new RectangleShape(new Vector2f(drawRect.Width - 2 * BorderThickness, drawRect.Height - 2 * BorderThickness));
                drawableRect.Position = new Vector2f(drawRect.Left + BorderThickness, drawRect.Top + BorderThickness);
                drawableRect.OutlineThickness = BorderThickness;
                drawableRect.OutlineColor = ObjectBorderColor;
                drawableRect.FillColor = Color.Transparent;
                Game.RenderWindow.Draw(drawableRect);
            }
        }
    }
}