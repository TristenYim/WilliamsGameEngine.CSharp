using System.Collections.Generic;
using SFML.System;
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
        
        // True if this node is a leaf node. Used for just about every operation.
        private bool _isLeaf;

        // The four lines marking the bounding box of this node.
        private readonly float _leftBound;
        private readonly float _topBound;
        private readonly float _rightBound;
        private readonly float _bottomBound;

        // The axes that mark the splits among children, used to insert an object into the correct child.
        private readonly float _xSplit;
        private readonly float _ySplit;

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
            // Set up the bounds and axes
            float width = bounds.Width;
            float height = bounds.Height;
            _leftBound = bounds.Left;
            _topBound = bounds.Top;
            _rightBound = _leftBound + width;
            _bottomBound = _topBound + height;
            _xSplit = _leftBound + width / 2f;
            _ySplit = _topBound + height / 2f;
            
            // Set up the drawable bounding box
            _boundingBox = new RectangleShape(new Vector2f(width - 2 * BorderThickness, height - 2 * BorderThickness));
            _boundingBox.Position = new Vector2f(_leftBound + BorderThickness, _topBound + BorderThickness);
            _boundingBox.OutlineThickness = BorderThickness;
            _boundingBox.OutlineColor = TreeBorderColor;
            _boundingBox.FillColor = Color.Transparent;
            _isLeaf = true;
        }

        // Adds a positional object into the right part of this tree and splits the tree if necessary.
        public void Insert(Positional positionalObject) {
            // If the object is not splitable, it automatically gets put in unsplitable objects and nothing else needs to be done.
            if (!IsSplittable(positionalObject))
            {
                _unsplittableObjects.Add(positionalObject);
                return;
            }
            if (_isLeaf)
            {
                // If it needs to split, it will split. Otherwise it add the object to splittable objects to be split later.
                if (_splittableObjects.Count < NodeCapacity)
                {
                    _splittableObjects.Add(positionalObject);
                    return;
                } else {
                    Split();
                }
            }
            // If this got split or it is not a leaf, the object needs to be added to the right child.
            InsertInRightQuadrant(positionalObject);
        }

        // Adds a positional object into the correct child.
        private void InsertInRightQuadrant(Positional positionalObject)
        {
            float x = positionalObject.Position.X;
            float y = positionalObject.Position.Y;
            if (x >= _xSplit && x <= _rightBound)
            {
                if (y >= _ySplit && y <= _bottomBound)
                {
                    // Goes to child1 if +X and +Y in reltation to the split axes.
                    _child1.Insert(positionalObject);
                    return;
                }
                else if (x >= _topBound)
                {
                    // Goes to child4 if +X and -Y in relation to the split axes.
                    _child4.Insert(positionalObject);
                    return;
                }
            }
            else if (x >= _leftBound)
            {
                if (y >= _ySplit && y <= _bottomBound)
                {
                    // Goes to child2 if -X and +Y in relation to the split axes.
                    _child2.Insert(positionalObject);
                    return;
                }
                else if (y >= _topBound)
                {
                    // Goes to child3 if -X and -Y in relation to the split axes.
                    _child3.Insert(positionalObject);
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

        // Searches for an instance of an object and deletes it.
        public void Delete(Positional positionalObject)
        {
            // First try the easy part, checking if the object can be removed from the splittable or unsplittable objects.
            if (_unsplittableObjects.Remove(positionalObject) || _isLeaf && _splittableObjects.Remove(positionalObject))
            {
                return;
            }

            // Then try deleting it from the right child.
            DeleteFromRightQuadrant(positionalObject);

            // It is now a leaf if all its children are empty leaves after deleting.
            if (_child1 == null || _child1.IsEmptyLeaf() && _child2.IsEmptyLeaf() && _child3.IsEmptyLeaf() && _child4.IsEmptyLeaf())
            {
                _isLeaf = true;
            }
            return;
        }

        private void DeleteFromRightQuadrant(Positional positionalObject)
        {
            float x = positionalObject.Position.X;
            float y = positionalObject.Position.Y;
            if (x >= _xSplit && x <= _rightBound)
            {
                if (y >= _ySplit && y <= _bottomBound)
                {
                    // Deletes from child1 if +X and +Y in reltation to the split axes.
                    _child1.Delete(positionalObject);
                    return;
                }
                else if (y >= _topBound)
                {
                    // Deletes from child4 if +X and -Y in relation to the split axes.
                    _child4.Delete(positionalObject);
                    return;
                }
            }
            else if (x >= _leftBound)
            {
                if (y >= _ySplit && y <= _bottomBound)
                {
                    // Deletes from child2 if -X and +Y in relation to the split axes.
                    _child2.Delete(positionalObject);
                    return;
                }
                else if (y >= _topBound)
                {
                    // Goes to child3 if -X and -Y in relation to the split axes.
                    _child3.Delete(positionalObject);
                    return;
                }
            }   
        }

        // Simply deletes the object, changes its position, and reinserts it, guaranteeing it will be moved into the correct part of the tree.
        public void Move (Positional positionalObject, Vector2f newPos)
        {
            Delete(positionalObject);
            positionalObject.Position = newPos;
            Insert(positionalObject);
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
            // Handle collisions in unsplittable objects.
            HandleCollisionsInList(_unsplittableObjects, checkList);

            // If this is a leaf, after checking collisions in splittable objects, you're done checking.
            if (_isLeaf)
            {
                HandleCollisionsInList(_splittableObjects, checkList);
                return;
            }

            // Divide the check list into a separte check list for each child.
            // Each check list only contains objects within the bounds of the child.
            LinkedList<Collidable> child1CheckList = new LinkedList<Collidable>();
            LinkedList<Collidable> child2CheckList = new LinkedList<Collidable>();
            LinkedList<Collidable> child3CheckList = new LinkedList<Collidable>();
            LinkedList<Collidable> child4CheckList = new LinkedList<Collidable>();
            foreach(Collidable collidable in checkList)
            {
                FloatRect collisionRect = collidable.CollisionRect;
                float left = collisionRect.Left;
                float top = collisionRect.Top;
                float right = left + collisionRect.Width;
                float bottom = top + collisionRect.Height;
                if (right > _xSplit && bottom > _ySplit)
                {
                    child1CheckList.AddLast(collidable);
                }
                if (right > _xSplit && top < _ySplit)
                {
                    child2CheckList.AddLast(collidable);
                }
                if (left < _xSplit && bottom > _ySplit)
                {
                    child3CheckList.AddLast(collidable);
                }
                if (left < _xSplit && top < _ySplit)
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
                Vector2f size = new Vector2f((_rightBound - _leftBound) / 2f, (_bottomBound - _topBound) / 2f);
                _child1 = new PositionalTree(new FloatRect(new Vector2f(_leftBound + size.X, _topBound + size.Y), size));
                _child2 = new PositionalTree(new FloatRect(new Vector2f(_leftBound, _topBound + size.Y), size));
                _child3 = new PositionalTree(new FloatRect(new Vector2f(_leftBound, _topBound), size));
                _child4 = new PositionalTree(new FloatRect(new Vector2f(_leftBound + size.X, _topBound), size));
            }
            _isLeaf = false;
            for (int i = _splittableObjects.Count - 1; i > -1; i--)
            {
                InsertInRightQuadrant(_splittableObjects[i]);
            }
            _splittableObjects.Clear();
        }

        // Checks if an object is both empty and is leaf is true. Used for determining whether to update is leaf when deleting.
        private bool IsEmptyLeaf()
        {
            return _isLeaf && _splittableObjects.Count == 0 && _unsplittableObjects.Count == 0;
        }

        // Checks if an object is splitable by determining if either of the axes intersect it.
        private bool IsSplittable(Positional positionalObject)
        {
            if (positionalObject is not Collidable)
            {
                return true;
            }
            FloatRect collisionRect = ((Collidable)positionalObject).CollisionRect;
            float left = collisionRect.Left;
            float top = collisionRect.Top;
            return !(left < _xSplit ^ left + collisionRect.Width < _xSplit)
                     && !(top < _ySplit ^ top + collisionRect.Height < _ySplit);
        }

        // Draws a box around the bounds of this nodes and all non-empty children, recursively.
        public void RecursiveDraw()
        {
            Game.RenderWindow.Draw(_boundingBox);
            DrawObjectCollisionBoxes();
            if (!_isLeaf)
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
            RectangleShape drawableRect = new RectangleShape();
            drawableRect.OutlineThickness = BorderThickness;
            drawableRect.OutlineColor = ObjectBorderColor;
            drawableRect.FillColor = Color.Transparent;
            foreach (Positional positionalObject in _splittableObjects)
            {
                if (positionalObject is not Collidable)
                {
                    continue;
                }
                FloatRect drawRect = ((Collidable)positionalObject).CollisionRect;
                drawableRect.Size = new Vector2f(drawRect.Width - 2 * BorderThickness, drawRect.Height - 2 * BorderThickness);
                drawableRect.Position = new Vector2f(drawRect.Left + BorderThickness, drawRect.Top + BorderThickness);
                Game.RenderWindow.Draw(drawableRect);
            }
            foreach (Positional positionalObject in _unsplittableObjects)
            {
                if (positionalObject is not Collidable)
                {
                    continue;
                }
                FloatRect drawRect = ((Collidable)positionalObject).CollisionRect;
                drawableRect.Size = new Vector2f(drawRect.Width - 2 * BorderThickness, drawRect.Height - 2 * BorderThickness);
                drawableRect.Position = new Vector2f(drawRect.Left + BorderThickness, drawRect.Top + BorderThickness);
                Game.RenderWindow.Draw(drawableRect);
            }
        }
    }
}