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
        private List<GameObject> _unsplittableObjects = new List<GameObject>();

        // Represents all positional objects that can be fully contained in a child node.
        private List<GameObject> _splittableObjects = new List<GameObject>();

        // The children are numbered based on the standard mathematical quadrant system.
        private PositionalTree _child1 = null;
        private PositionalTree _child2 = null;
        private PositionalTree _child3 = null;
        private PositionalTree _child4 = null;

        // Pointer to the parent node.
        private PositionalTree _parent;

        // True if this node is a leaf node. Used for just about every operation.
        private bool _isLeaf;

        // The four lines marking the bounding box of this node.
        private readonly float _leftBound;
        private readonly float _topBound;
        private readonly float _rightBound;
        private readonly float _bottomBound;
        public float LeftBound
        {
            get => _leftBound;
        }
        public float TopBound
        {
            get => _topBound;
        }
        public float RightBound
        {
            get => _rightBound;
        }
        public float BottomBound
        {
            get => _bottomBound;
        }        
        public FloatRect Bounds
        {
            get => new FloatRect(_leftBound, _rightBound, _rightBound - _leftBound, _topBound - _bottomBound);
        }

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

        // The color of the splittable object bounding boxes.
        private static readonly Color SplittableObjectBorderColor = Color.Green;

        // The color of the unsplittable object bounding boxes
        private static readonly Color UnsplittableObjectBorderColor = new Color(255, 130, 0);

        // Constructs the positional tree based on the given bounds.
        public PositionalTree(FloatRect bounds, PositionalTree parent)
        {
            // Set the parent.
            _parent = parent;

            // Set the bounds and axes.
            float width = bounds.Width;
            float height = bounds.Height;
            _leftBound = bounds.Left;
            _topBound = bounds.Top;
            _rightBound = _leftBound + width;
            _bottomBound = _topBound + height;
            _xSplit = _leftBound + width / 2f;
            _ySplit = _topBound + height / 2f;

            // Set the drawable bounding box.
            _boundingBox = new RectangleShape(new Vector2f(width - 2 * BorderThickness, height - 2 * BorderThickness));
            _boundingBox.Position = new Vector2f(_leftBound + BorderThickness, _topBound + BorderThickness);
            _boundingBox.OutlineThickness = BorderThickness;
            _boundingBox.OutlineColor = TreeBorderColor;
            _boundingBox.FillColor = Color.Transparent;
            _isLeaf = true;
        }

        // Adds a positional object into the right part of this tree and splits the tree if necessary.
        public void Insert(GameObject gameObject) {
            // There are separate methods for collidable objects and point-only objects to optimize each one separately
            if (gameObject.IsCollidable)
            {
                FloatRect collisionRect = gameObject.GetCollisionRect();
                float left = collisionRect.Left;
                float top = collisionRect.Top;
                Insert(gameObject, left, top, left + collisionRect.Width, top + collisionRect.Height);
            }
            else //if (gameObject.BelongsOnTree)
            {
                float x = gameObject.Position.X;
                float y = gameObject.Position.Y;
                Insert(gameObject, x, y);
            }
            /*else
            {
                Console.WriteLine("Warning: Tried to insert object that does not belong on tree" +
                                  "in bounds between (" + _leftBound + ", " + _topBound + ") and (" + _rightBound + ", " + _bottomBound + ")");
                return;
            }*/
        }
        private void Insert(GameObject cObject, float left, float top, float right, float bottom)
        {
            // This helper method is for collidable objects.
            bool negX = left < _xSplit;
            bool negY = top < _ySplit;
            bool posX = right >= _xSplit;
            bool posY = bottom >= _ySplit;

            // Since the object has a collision box, we must additionally check if its lying on as well as outside of bounds.
            if (!(negX ^ posX) && !(negY ^ posY) || left < _leftBound || right > _rightBound || top < _topBound || bottom > _bottomBound)
            {
                cObject.NodePointer = this;
                _unsplittableObjects.Add(cObject);
            }
            else if (_isLeaf)
            {
                // If its a leaf, add it to splittable objects and split if necessary.
                cObject.NodePointer = this;
                _splittableObjects.Add(cObject);
                if (_splittableObjects.Count >= NodeCapacity)
                {
                    Split();
                }
            }
            else
            {
                // Otherwise, insert it into the right quadrant.
                if (posX)
                {
                    if (posY)
                    {
                        _child1.Insert(cObject, left, top, right, bottom);
                    }
                    else
                    {
                        _child4.Insert(cObject, left, top, right, bottom);
                    }
                }
                else
                {
                    if (posY)
                    {
                        _child2.Insert(cObject, left, top, right, bottom);
                    }
                    else
                    {
                        _child3.Insert(cObject, left, top, right, bottom);
                    }
                }
            }
        }
        private void Insert(GameObject pObject, float x, float y)
        {
            // This helper method is for point only objects.
            bool posX = x >= _xSplit;
            bool posY = y >= _ySplit;

            // Since this is only a point, we only have to check if its out of bounds.
            if (x < _leftBound || x > _rightBound || y < _topBound || y > _bottomBound)
            {
                pObject.NodePointer = this;
                _unsplittableObjects.Add(pObject);
                return;
            }
            else if (_isLeaf)
            {
                // If its a leaf, add it to splittable objects and split if necessary.
                pObject.NodePointer = this;
                _splittableObjects.Add(pObject);
                if (_splittableObjects.Count >= NodeCapacity)
                {
                    Split();
                }
            }
            else
            {
                // Otherwise, insert it into the right quadrant.
                if (posX)
                {
                    if (posY)
                    {
                        _child1.Insert(pObject, x, y);
                    }
                    else
                    {
                        _child4.Insert(pObject, x, y);
                    }
                }
                else
                {
                    if (posY)
                    {
                        _child2.Insert(pObject, x, y);
                    }
                    else
                    {
                        _child3.Insert(pObject, x, y);
                    }
                }
            }
        }

        // Constructs the children if they are null and moves all splittable objects into their correct quadrant
        public void Split()
        {
            if (_child1 == null)
            {
                Vector2f size = new Vector2f((_rightBound - _leftBound) / 2f, (_bottomBound - _topBound) / 2f);
                _child1 = new PositionalTree(new FloatRect(new Vector2f(_leftBound + size.X, _topBound + size.Y), size), this);
                _child2 = new PositionalTree(new FloatRect(new Vector2f(_leftBound, _topBound + size.Y), size), this);
                _child3 = new PositionalTree(new FloatRect(new Vector2f(_leftBound, _topBound), size), this);
                _child4 = new PositionalTree(new FloatRect(new Vector2f(_leftBound + size.X, _topBound), size), this);
            }
            _isLeaf = false;
            foreach (var splitObject in _splittableObjects)
            {
                Insert(splitObject);
            }
            _splittableObjects.Clear();
        }

        // This deletes an object using its NodePointer if available, or by performing a search delete if not.
        public void Delete(GameObject pObject)
        {
            // In most cases, there should be no reason to perform a search-delete on a pObject.
            /*if (!pObject.BelongsOnTree)
            {
                Console.WriteLine("Warning: Tried to delete object that does not belong on tree" +
                                  "in bounds between (" + _leftBound + ", " + _topBound + ") and (" + _rightBound + ", " + _bottomBound + ")");
                return;
            }*/
            /*if (pObject.NodePointer == null)
            {
                Console.WriteLine("Warning: Performed a search-delete for an object at (" + pObject.Position.X + ", " + pObject.Position.Y + ")");
                SearchDelete(pObject);
            }*/
            //else
            {
                PositionalTree node = pObject.NodePointer;

                if (!node._splittableObjects.Remove(pObject) && !node._unsplittableObjects.Remove(pObject))
                {
                    // An object's NodePointer must contain it, otherwise you're implementing NodePointer wrong.
                    //node.ThrowOperationErrorException("delete", pObject);
                    Console.WriteLine("Warning: Performed a search-delete for an object at (" + pObject.Position.X + ", " + pObject.Position.Y + ")");
                    SearchDelete(pObject);
                }

                // If deleting from this turned it into an empty leaf, merge if necessary.
                else if (node._parent != null && node.IsEmptyLeaf())
                {
                    node._parent.Merge();
                }
            }
        }

        // Searches for an instance of an object and deletes it.
        private void SearchDelete(GameObject pObject)
        {
            // First try the easy part, checking if the object can be removed from the splittable or unsplittable objects.
            if (_unsplittableObjects.Remove(pObject) || _isLeaf && _splittableObjects.Remove(pObject))
            {
                Merge();
                return;
            }

            // Then try deleting it from the right child.
            float x = pObject.Position.X;
            float y = pObject.Position.Y;
            if (x >= _xSplit && x <= _rightBound)
            {
                if (y >= _ySplit && y <= _bottomBound)
                {
                    // Deletes from child1 if +X and +Y in reltation to the split axes.
                    _child1.SearchDelete(pObject);
                    return;
                }
                else if (y >= _topBound)
                {
                    // Deletes from child4 if +X and -Y in relation to the split axes.
                    _child4.SearchDelete(pObject);
                    return;
                }
            }
            else if (x >= _leftBound)
            {
                if (y >= _ySplit && y <= _bottomBound)
                {
                    // Deletes from child2 if -X and +Y in relation to the split axes.
                    _child2.SearchDelete(pObject);
                    return;
                }
                else if (y >= _topBound)
                {
                    // Goes to child3 if -X and -Y in relation to the split axes.
                    _child3.SearchDelete(pObject);
                    return;
                }
            }

            // If it fails a search-delete, that means you tried to delete an object that is not in the tree.
            ThrowOperationErrorException("search-delete", pObject);
        }

        // Use this when deleting to ensure that nodes whose children are empty leaves will merge.
        private void Merge()
        {
            // It all its children are empty leaves, make this a leaf.
            if (_child1 == null || _child1.IsEmptyLeaf() && _child2.IsEmptyLeaf() && _child3.IsEmptyLeaf() && _child4.IsEmptyLeaf())
            {
                _isLeaf = true;

                // Sometimes, a merge will trigger other merges. This accounts for those scenarios.
                if (_parent != null)
                {
                    _parent.Merge();
                }
            }
        }

        // Deletes the object, and inserts it into the nearest parent which can fully contain it.
        public void Move (GameObject pObject, Vector2f newPos)
        {
            if (this != pObject.NodePointer)
            {
                pObject.NodePointer.Move(pObject, newPos);
            }
            else
            {
                pObject.Position = newPos;

                // There are separate helper methods for collidable objects and point only objects to optimize each one separately 
                if (pObject.IsCollidable)
                {
                    FloatRect collisionRect = pObject.GetCollisionRect();
                    float left = collisionRect.Left;
                    float top = collisionRect.Top;
                    float right = left + collisionRect.Width;
                    float bottom = top + collisionRect.Height;
                    
                    // If the object does not need to be moved to a different node, don't bother deleting or reinserting it.
                    if (_parent == null || left >= _leftBound && right <= _rightBound && top >= _topBound && bottom <= _bottomBound)
                    {
                        // Otherwise delete and reinsert it if the object is in bounds.
                        Delete(pObject);
                        Insert(pObject, left, top, right, bottom);
                    }
                    else
                    {
                        // Otherwise move it if it is out of bounds
                        Delete(pObject);
                        _parent.Move(pObject, left, top, right, bottom);
                    }
                }
                else
                {
                    float x = newPos.X;
                    float y = newPos.Y;

                    // If the object does not need to be moved to a different node, don't bother deleting or reinserting it.
                    if (_parent == null || x >= _leftBound && x <= _rightBound && y >= _topBound && y <= _bottomBound)
                    {
                        // Otherwise delete and reinsert it so it gets put in the right child.
                        Delete(pObject);
                        Insert(pObject, x, y);
                    }
                    else
                    {
                        // Move it if it is out of bounds
                        Delete(pObject);
                        _parent.Move(pObject, x, y);
                    }
                }
            }
        }
        private void Move (GameObject cObject, float left, float top, float right, float bottom)
        {
            if (_parent == null || left >= _leftBound && right <= _rightBound && top >= _topBound && bottom <= _bottomBound)
            {
                // If this node can fully contain the object, insert it.
                Insert(cObject, left, top, right, bottom);
            }
            else
            {
                // Otherwise try the parent.
                _parent.Move(cObject, left, top, right, bottom);
            }
        }
        private void Move (GameObject pObject, float x, float y)
        {
            if (_parent == null || x >= _leftBound && x <= _rightBound && y >= _topBound && y <= _bottomBound)
            {
                // If this node can fully contain the object, insert it.
                Insert(pObject, x, y);
            }
            else
            {
                // Otherwise try the parent.
                _parent.Move(pObject, x, y);
            }
        }

        // Handles the collisions of all objects in the entire tree.
        // This is implemented using something called recursion, where a method repeatedly calls itself.
        // In this case, the node handles its own collisions then divides a list of objects that check for collisions into four separate lists.
        // Each check list only contains objects that intersect its corresponding child node.
        // This makes checking for collisions O(n log(n)) rather than O(n^2).
        public void HandleCollisions()
        {
            HandleCollisions(new List<GameObject>());
        }
        private void HandleCollisions(List<GameObject> checkList)
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
            List<GameObject> child1CheckList = new List<GameObject>();
            List<GameObject> child2CheckList = new List<GameObject>();
            List<GameObject> child3CheckList = new List<GameObject>();
            List<GameObject> child4CheckList = new List<GameObject>();
            foreach (var cObject in checkList)
            {
                FloatRect collisionRect = cObject.GetCollisionRect();
                float left = collisionRect.Left;
                float top = collisionRect.Top;
                bool posX = left + collisionRect.Width > _xSplit;
                bool negX = left < _xSplit;
                bool posY = top + collisionRect.Height > _ySplit;
                bool negY = top < _ySplit;
                if (posX && posY)
                {
                    child1CheckList.Add(cObject);
                }
                if (posX && negY)
                {
                    child4CheckList.Add(cObject);
                }
                if (negX && posY)
                {
                    child2CheckList.Add(cObject);
                }
                if (negX && negY)
                {
                    child3CheckList.Add(cObject);
                }
            }

            // Handle the collisions of each child.
            _child1.HandleCollisions(child1CheckList);
            _child2.HandleCollisions(child2CheckList);
            _child3.HandleCollisions(child3CheckList);
            _child4.HandleCollisions(child4CheckList);
        }

        // Each object in handle list is checked against check list and then added to check list if it is checking for collisions.
        private void HandleCollisionsInList(List<GameObject> handleList, List<GameObject> checkList)
        {
            foreach (var cObject in handleList)
            {
                // Don't check for collisions if the object isn't even collidable
                if (!cObject.IsCollidable)
                {
                    continue;
                }

                // Check for collisions
                foreach(var cObjectChecks in checkList)
                {
                    if (cObject.GetCollisionRect().Intersects(cObjectChecks.GetCollisionRect()))
                    {
                        cObjectChecks.HandleCollision(cObject);
                        cObject.HandleCollision(cObjectChecks);
                    }
                }

                // Add this to the check list if this checks for collisions
                if (cObject.IsCollisionCheckEnabled())
                {
                    checkList.Add(cObject);
                }
            }
        }

        // Checks if an object is both empty and is leaf is true. Used for determining whether to update is leaf when deleting.
        private bool IsEmptyLeaf()
        {
            return _isLeaf && _splittableObjects.Count == 0 && _unsplittableObjects.Count == 0;
        }

        // This exception helps with debugging if the engine is performing operations incorrectly for some reason.
        private void ThrowOperationErrorException(String operation, GameObject pObject)
        {
            throw new Exception("Could not " + operation + " object at (" + pObject.Position.X + ", " + pObject.Position.Y + ")" +
                                "in bounds between (" + _leftBound + ", " + _topBound + ") and (" + _rightBound + ", " + _bottomBound + ")");
        }

        // Draws a box around the bounds of this nodes and all non-empty children, recursively.
        public void Draw()
        {
            // Draw the bounding box.
            Game.RenderWindow.Draw(_boundingBox);

            // Seeing the collision boxes gives other useful information.
            DrawObjectCollisionBoxes();

            // Draw the bounding boxes of the children unless this is a leaf node.
            if (!_isLeaf)
            {
                _child1.Draw();
                _child2.Draw();
                _child3.Draw();
                _child4.Draw();
            }
        }

        // Draws a box around the collision box of each object in this node.
        private void DrawObjectCollisionBoxes()
        {
            Camera cam = Game.CurrentScene.Camera;

            // Sets the thickness, border, and scale of the rectangle that will be drawn.
            RectangleShape drawableRect = new RectangleShape();
            drawableRect.OutlineThickness = BorderThickness;
            drawableRect.FillColor = Color.Transparent;

            // Setup and Draw the rectangle for each splittable and unsplittable GameObject
            foreach (var cObject in _splittableObjects)
            {
                if (!cObject.IsCollidable)
                {
                    continue;
                }
                drawableRect.OutlineColor = SplittableObjectBorderColor;

                FloatRect drawRect = cObject.GetCollisionRect();

                drawableRect.Size = new Vector2f(drawRect.Width - 2 * BorderThickness, drawRect.Height - 2 * BorderThickness);
                drawableRect.Position = new Vector2f(drawRect.Left + BorderThickness, drawRect.Top + BorderThickness);
                
                Game.RenderWindow.Draw(drawableRect);
            }
            foreach (var cObject in _unsplittableObjects)
            {
                if (!cObject.IsCollidable)
                {
                    continue;
                }
                drawableRect.OutlineColor = UnsplittableObjectBorderColor;
                
                FloatRect drawRect = cObject.GetCollisionRect();
                drawableRect.Size = new Vector2f(drawRect.Width - 2 * BorderThickness, drawRect.Height - 2 * BorderThickness);
                drawableRect.Position = new Vector2f(drawRect.Left + BorderThickness, drawRect.Top + BorderThickness);
                
                Game.RenderWindow.Draw(drawableRect);
            }
        }
    }
}