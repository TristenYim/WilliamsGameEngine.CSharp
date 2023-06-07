using System.Collections.Generic;
using SFML.System;
using SFML.Graphics;
using System;

namespace GameEngine
{
    // This is not a game object because its not meant to be inserted into the scene. It is a feature of the scene itself.
    class SpatialTree : Drawable
    {
        // Represents all collidable objects that cannot be fully contained in any of the child nodes.
        private readonly List<GameObject> _unsplittableObjects = new List<GameObject>();

        // Represents all spatial objects that can be fully contained in a child node.
        private readonly List<GameObject> _splittableObjects = new List<GameObject>();

        // The children are numbered based on the standard mathematical quadrant system.
        private SpatialTree _child1 = null;
        private SpatialTree _child2 = null;
        private SpatialTree _child3 = null;
        private SpatialTree _child4 = null;

        // Pointer to the parent node.
        private readonly SpatialTree _parent;

        // True if this node is a leaf node. Used for just about every operation.
        private bool _isLeaf;

        // The four lines marking the bounding box of this node.
        public float LeftBound { get; }
        public float TopBound { get; }
        public float RightBound { get; }
        public float BottomBound { get; }        
        public FloatRect Bounds
        {
            get => new FloatRect(LeftBound, RightBound, RightBound - LeftBound, TopBound - BottomBound);
        }

        // The axes that mark the splits among children, used to insert an object into the correct child.
        private readonly float _xSplit;
        private readonly float _ySplit;

        // The number of objects that can be stored in a node before the tree is split.
        private const int NodeCapacity = 4;

        public int CameraIndex { get; private set; }

        // The drawable counterpart to bounds, used for drawing the bounding box when debugging.
        private readonly RectangleShape _boundingBox;

        // The thickness of the bounding box and object boxes.
        private const float BorderThickness = 1f;

        // The color of the tree bounding boxes.
        private static readonly Color TreeBorderColor = Color.Cyan;

        // The color of the splittable object bounding boxes.
        private static readonly Color SplittableObjectBorderColor = Color.Green;

        // The color of the unsplittable object bounding boxes
        private static readonly Color UnsplittableObjectBorderColor = new Color(255, 130, 0);

        // Constructs the spatial tree based on the given bounds.
        public SpatialTree(FloatRect bounds, SpatialTree parent, int cameraIndex)
        {
            //_splittableObjects = new GameObject[NodeCapacity];
            _parent = parent;
            _isLeaf = true;

            // Set the bounds and axes.
            LeftBound = bounds.Left;
            TopBound = bounds.Top;
            RightBound = LeftBound + bounds.Width;
            BottomBound = TopBound + bounds.Height;
            _xSplit = LeftBound + bounds.Width / 2f;
            _ySplit = TopBound + bounds.Height / 2f;

            // Set the drawing members.
            CameraIndex = cameraIndex;
            _boundingBox = new RectangleShape(new Vector2f(bounds.Width - 2 * BorderThickness, bounds.Height - 2 * BorderThickness));
            _boundingBox.Position = new Vector2f(LeftBound + BorderThickness, TopBound + BorderThickness);
            _boundingBox.OutlineThickness = BorderThickness;
            _boundingBox.OutlineColor = TreeBorderColor;
            _boundingBox.FillColor = Color.Transparent;
        }

        // Adds a spatial object into the right part of this tree and splits the tree if necessary.
        public void Insert(GameObject gameObject) {
            // There are separate methods for collidable objects and point-only objects to optimize each one separately
            if (gameObject.IsCollidable)
            {
                Insert(gameObject, gameObject.GetCollisionRect().Left, gameObject.GetCollisionRect().Top, gameObject.GetCollisionRect().Left 
                       + gameObject.GetCollisionRect().Width, gameObject.GetCollisionRect().Top + gameObject.GetCollisionRect().Height);
            }
            else if (gameObject.BelongsOnTree)
            {
                Insert(gameObject, gameObject.Position.X, gameObject.Position.Y);
            }
            else
            {
                Console.WriteLine("Warning: Tried to insert object that does not belong on tree" +
                                  "in bounds between (" + LeftBound + ", " + TopBound + ") and (" + RightBound + ", " + BottomBound + ")");
                return;
            }
        }
        
        // This helper method is for collidable objects.
        private void Insert(GameObject cObject, float left, float top, float right, float bottom)
        {
            bool negX = left < _xSplit;
            bool negY = top < _ySplit;
            bool posX = right >= _xSplit;
            bool posY = bottom >= _ySplit;

            // Since the object has a collision box, we must additionally check if its lying on as well as outside of bounds.
            if (!(negX ^ posX) && !(negY ^ posY) || rectIsOutOfBounds(left, top, right, bottom))
            {
                cObject.TreeNodePointer = this;
                _unsplittableObjects.Add(cObject);
            }
            else if (_isLeaf)
            {
                // If its a leaf, add it to splittable objects and split if necessary.
                cObject.TreeNodePointer = this;
                _splittableObjects.Add(cObject);
                if (_splittableObjects.Count >= NodeCapacity)
                {
                    Split();
                }
            }
            else
            {
                // Otherwise, insert it into the right quadrant.
                getRightQuadrant(posX, posY).Insert(cObject, left, top, right, bottom);
            }
        }
        
        // This helper method is for point-only objects.
        private void Insert(GameObject pObject, float x, float y)
        {
            // Since this is only a point, we only have to check if its out of bounds.
            if (pointIsOutOfBounds(x, y))
            {
                pObject.TreeNodePointer = this;
                _unsplittableObjects.Add(pObject);
                return;
            }
            else if (_isLeaf)
            {
                // If its a leaf, add it to splittable objects and split if necessary.
                pObject.TreeNodePointer = this;
                _splittableObjects.Add(pObject);
                if (_splittableObjects.Count >= NodeCapacity)
                {
                    Split();
                }
            }
            else
            {
                // Otherwise, insert it into the right quadrant.
                getRightQuadrant(x >= _xSplit, y >= _ySplit).Insert(pObject, x, y);
            }
        }

        // Constructs the children if they are null and moves all splittable objects into their correct quadrant
        public void Split()
        {
            // This creates the children if they have not yet been created.
            if (_child1 == null)
            {
                Vector2f size = new Vector2f((RightBound - LeftBound) / 2f, (BottomBound - TopBound) / 2f);
                _child1 = new SpatialTree(new FloatRect(new Vector2f(LeftBound + size.X, TopBound + size.Y), size), this, CameraIndex);
                _child2 = new SpatialTree(new FloatRect(new Vector2f(LeftBound, TopBound + size.Y), size), this, CameraIndex);
                _child3 = new SpatialTree(new FloatRect(new Vector2f(LeftBound, TopBound), size), this, CameraIndex);
                _child4 = new SpatialTree(new FloatRect(new Vector2f(LeftBound + size.X, TopBound), size), this, CameraIndex);
            }
            _isLeaf = false;

            // Insert each object in _splittableObjects in the right quadrant then delete remove it from _splittableObjects.
            for (int i = _splittableObjects.Count - 1; i > 0; i--)
            {
                // Since we already know this is a splittable object, we do this instead of calling Insert to save a few checks.
                bool posX = _splittableObjects[i].GetCollisionRect().Left >= _xSplit;
                bool posY = _splittableObjects[i].GetCollisionRect().Top >= _ySplit;
                getRightQuadrant(posX, posY).Insert(_splittableObjects[i]);
                _splittableObjects.RemoveAt(i);
            }
        }

        // This searches for an object in the tree based on a location.
        public GameObject Search(Vector2f pos)
        {
            // First, check to see if this node contains it.
            foreach(var gameObject in _unsplittableObjects)
            {
                if ((Vector2i)gameObject.Position == (Vector2i)pos)
                {
                    return gameObject;
                }
            }

            // Only search splittable objects if this is a leaf.
            if (_isLeaf)
            {
                foreach(var gameObject in _splittableObjects)
                {
                    if ((Vector2i)gameObject.Position == (Vector2i)pos)
                    {
                        return gameObject;
                    }
                }

                // If it reaches this point, no further searching can be done and null is returned.
                return null;
            }

            // TODO: Finish this
            // If it could not be found, try the children. Don't bother searching if a result has already been found.
            GameObject result = _child1.Search(pos);
            if (result == null)
            {
                _child2.Search(pos);
            }
            if (result == null)
            {
                _child3.Search(pos);
            }
            if (result == null)
            {
                _child4.Search(pos);
            }
            return result;
        }

        // This deletes an object using its NodePointer if available, or by performing a search delete if not.
        public void Delete(GameObject pObject)
        {
            if (!pObject.TreeNodePointer._splittableObjects.Remove(pObject) && !pObject.TreeNodePointer._unsplittableObjects.Remove(pObject))
            {
                // An object's NodePointer must contain it, otherwise you're implementing NodePointer wrong.
                //node.ThrowOperationErrorException("delete", pObject);
                Console.WriteLine("Warning: Performed a search-delete for an object at (" + pObject.Position.X + ", " + pObject.Position.Y + ")");
                SearchDelete(pObject);
            }
            else if (pObject.TreeNodePointer._parent != null && pObject.TreeNodePointer.IsEmptyLeaf())
            {
                // Merge if deleting from this turned it into an empty leaf.
                pObject.TreeNodePointer._parent.Merge();
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

            // Check if it's out of bounds - if it is, that means you tried to delete an object that is not in the tree.
            if (pointIsOutOfBounds(pObject.Position))
            {
                
            }

            // Otherwise, try to delete from a child.
            bool posX = pObject.Position.X >= _xSplit;
            bool posY = pObject.Position.Y >= _ySplit;
            getRightQuadrant(pObject.Position.X >= _xSplit, pObject.Position.Y >= _ySplit).SearchDelete(pObject);
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

        // TODO: Fix the efficiency/readability of this.
        // Deletes the object, and inserts it into the nearest parent which can fully contain it.
        public void Move (GameObject pObject, Vector2f newPos)
        {
            if (this != pObject.TreeNodePointer)
            {
                pObject.TreeNodePointer.Move(pObject, newPos);
            }
            else
            {
                pObject.Position = newPos;

                // There are separate helper methods for collidable objects and point only objects to optimize each one separately.
                if (pObject.IsCollidable)
                {
                    if (_parent == null || !rectIsOutOfBounds(pObject.GetCollisionRect()))
                    {
                        // If this is a leaf, the object does not need to be moved to a different node.
                        if (!_isLeaf)
                        {
                            // Otherwise, delete and reinsert it into the right child.
                            if (!_splittableObjects.Remove(pObject))
                            {
                                _unsplittableObjects.Remove(pObject);
                            }
                            Insert(pObject, pObject.GetCollisionRect().Left, pObject.GetCollisionRect().Top, pObject.GetCollisionRect().Left 
                                   + pObject.GetCollisionRect().Width, pObject.GetCollisionRect().Top + pObject.GetCollisionRect().Height);
                        }
                    }
                    else
                    {
                        // Otherwise, delete and move it if it is out of bounds
                        if (!_splittableObjects.Remove(pObject))
                        {
                            _unsplittableObjects.Remove(pObject);
                        }
                        _parent.Move(pObject, pObject.GetCollisionRect().Left, pObject.GetCollisionRect().Top, pObject.GetCollisionRect().Left 
                                     + pObject.GetCollisionRect().Width, pObject.GetCollisionRect().Top + pObject.GetCollisionRect().Height);
                    }
                }
                else
                {
                    if (_parent == null || !pointIsOutOfBounds(newPos))
                    {
                        // If the object does not need to be moved to a different node, don't bother deleting and reinserting it.
                        if (!_isLeaf)
                        {
                            // Otherwise delete and reinsert it so it gets put in the right child.
                            _unsplittableObjects.Remove(pObject);
                            Insert(pObject, newPos.X, newPos.Y);
                        }
                    }
                    else
                    {
                        // If it is out of bounds, try to see if it can be moved into _parent
                        Delete(pObject);
                        _parent.Move(pObject, newPos.X, newPos.Y);
                    }
                }
            }
        }
        private void Move (GameObject cObject, float left, float top, float right, float bottom)
        {
            if (_parent == null || !rectIsOutOfBounds(left, top, right, bottom))
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
            if (_parent == null || !pointIsOutOfBounds(x, y))
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
                bool posX = cObject.GetCollisionRect().Left + cObject.GetCollisionRect().Width > _xSplit;
                bool negX = cObject.GetCollisionRect().Left < _xSplit;
                bool posY = cObject.GetCollisionRect().Top + cObject.GetCollisionRect().Height > _ySplit;
                bool negY = cObject.GetCollisionRect().Top < _ySplit;
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
            for (int i = 0; i < handleList.Count; i++)
            {
                GameObject cObject = handleList[i];
                
                // Don't check for collisions if the object isn't even collidable.
                if (!cObject.IsCollidable)
                {
                    continue;
                }

                // Check for collisions.
                foreach(var cObjectChecks in checkList)
                {
                    if (cObject.GetCollisionRect().Intersects(cObjectChecks.GetCollisionRect()))
                    {
                        cObjectChecks.HandleCollision(cObject);
                        cObject.HandleCollision(cObjectChecks);
                    }
                }

                // Add this to the check list if this checks for collisions.
                if (cObject.IsCollisionCheckEnabled())
                {
                    checkList.Add(cObject);
                }
            }
        }

        // Draws a box around the bounds of this nodes and all non-empty children, recursively.
        public void Draw(RenderTarget target, RenderStates states)
        {
            // Draw the bounding box.
            Game.RenderWindow.Draw(_boundingBox);

            // Seeing the collision boxes gives other useful information.
            DrawObjectCollisionBoxes(target);

            // Draw the bounding boxes of the children unless this is a leaf node.
            if (!_isLeaf)
            {
                _child1.Draw(target, states);
                _child2.Draw(target, states);
                _child3.Draw(target, states);
                _child4.Draw(target, states);
            }
        }

        // Draws a box around the collision box of each object in this node.
        private void DrawObjectCollisionBoxes(RenderTarget target)
        {
            // Sets the thickness, border, and scale of the rectangle that will be drawn.
            RectangleShape drawableRect = new RectangleShape();
            drawableRect.OutlineThickness = BorderThickness;
            drawableRect.FillColor = Color.Transparent;

            // Setup and Draw the rectangle for each splittable and unsplittable GameObject.
            drawableRect.OutlineColor = SplittableObjectBorderColor;
            DrawObjectsInList(_splittableObjects, drawableRect, target);
            drawableRect.OutlineColor = UnsplittableObjectBorderColor;
            DrawObjectsInList(_unsplittableObjects, drawableRect, target);
        }

        private void DrawObjectsInList(List<GameObject> drawList, RectangleShape baseRect, RenderTarget target)
        {
            foreach (var cObject in drawList)
            {
                if (!cObject.IsCollidable)
                {
                    continue;
                }
                baseRect.Size = new Vector2f(cObject.GetCollisionRect().Width - 2 * BorderThickness, cObject.GetCollisionRect().Height - 2 * BorderThickness);
                baseRect.Position = new Vector2f(cObject.GetCollisionRect().Left + BorderThickness, cObject.GetCollisionRect().Top + BorderThickness);
                target.Draw(baseRect);
            }
        }

        // Checks if an object is both empty and is leaf is true. Used for determining whether to update is leaf when deleting.
        private bool IsEmptyLeaf()
        {
            return _isLeaf && _splittableObjects.Count == 0 && _unsplittableObjects.Count == 0;
        }

        private SpatialTree getRightQuadrant(bool posX, bool posY)
        {
            if (posX)
            {
                if (posY)
                {
                    // _child1 if +X +Y
                    return _child1;
                }
                // _child4 if +X -Y
                return _child4;
            }
            if (posY)
            {
                // _child2 if -X +Y
                return _child2;
            }
            // _child3 if -X -Y
            return _child3;
        }
    
        // Returns true if a FloatRect is out of bounds.
        private bool rectIsOutOfBounds(FloatRect rect)
        {
            return (rect.Left <= LeftBound || rect.Top <= TopBound || rect.Left + rect.Width >= RightBound || rect.Top + rect.Height >= BottomBound);
        }

        // Returns true if a FloatRect (represented as lines) is out of bounds.
        private bool rectIsOutOfBounds(float left, float top, float right, float bottom)
        {
            return (left < LeftBound || top < TopBound || right > RightBound || bottom > BottomBound);
        }

        // Returns true if a point is out of bounds
        private bool pointIsOutOfBounds(Vector2f point)
        {
            return (point.X < LeftBound || point.Y < TopBound || point.X > RightBound || point.Y < BottomBound);
        }

        // Returns true if a point (reperesented as coordinates) is out of bounds
        private bool pointIsOutOfBounds(float x, float y)
        {
            return (x < LeftBound || y < TopBound || x > RightBound || y < BottomBound);
        }
    }
}