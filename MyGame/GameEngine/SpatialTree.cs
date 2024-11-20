using System.Collections.Generic;
using SFML.System;
using SFML.Graphics;
using System;

namespace GameEngine
{
    // This is the SpatialTree, which stores objects that have positions in a useful way which allows for efficient searching and collision detection.
    // Note that this is not a GameObject because it is a feature of the Scene itself.
    class SpatialTree
    {
        // Represents all collidable objects that cannot be fully contained in any of the child nodes.
        private readonly List<SpatialTreeMember> _unsplittableObjects = new List<SpatialTreeMember>();

        // Represents all spatial objects that can be fully contained in a child node.
        private readonly List<SpatialTreeMember> _splittableObjects = new List<SpatialTreeMember>();

        // The children are numbered based on the standard mathematical quadrant system.
        // Do note that positive y is down in SFML, so that would put Child1 in the bottom right corner for example.
        public SpatialTree Child1 { get; private set; }
        public SpatialTree Child2 { get; private set; }
        public SpatialTree Child3 { get; private set; }
        public SpatialTree Child4 { get; private set; }

        // Pointer to the parent node.
        private readonly SpatialTree _parent;

        // True if this node is a leaf node. Used for just about every operation.
        public bool IsLeaf { get; private set; }

        // The four lines marking the bounding box of this node.
        public float LeftBound { get; }
        public float TopBound { get; }
        public float RightBound { get; }
        public float BottomBound { get; }        
        public FloatRect Bounds
        {
            get => new FloatRect(LeftBound, RightBound, RightBound - LeftBound, TopBound - BottomBound);
        }

        // The axes that mark the splits among children, defining which objects belong in which child.
        // Note that the Y-axis is used to split objects in the X dimension, and vice versa.
        private Vector2f _splitAxes;

        // The number of objects that can be stored in a node before the tree is split.
        private const int NodeCapacity = 3;

        // Constructs the spatial tree based on the given bounds.
        public SpatialTree(FloatRect bounds, SpatialTree parent)
        {
            _parent = parent;
            IsLeaf = true;

            // Set the bounds and axes.
            LeftBound = bounds.Left;
            TopBound = bounds.Top;
            RightBound = LeftBound + bounds.Width;
            BottomBound = TopBound + bounds.Height;
            _splitAxes.Y = LeftBound + bounds.Width / 2f;
            _splitAxes.X = TopBound + bounds.Height / 2f;
        }

        // Adds a spatial object into the right part of this tree and splits the tree if necessary.
        public void Insert(GameObject gameObject)
        {
            if (gameObject.Position == null)
            {
                Console.WriteLine("WARNING: Tried to insert a GameObject without a position into the SpatialTree!");
            }
            SpatialTreeMember gameObjectMember = new SpatialTreeMember(gameObject, _splitAxes);
            if (gameObjectMember.IsPointOnly())
            {
                InsertPositionalOnly(gameObjectMember);
            }
            else
            {
                InsertCollidabale(gameObjectMember);
            }
        }
        
        private void InsertCollidabale(SpatialTreeMember collidableMember)
        {
            collidableMember.RecalculateQuadrants(_splitAxes);
            if (!collidableMember.IsSplittable())
            {
                _unsplittableObjects.Add(collidableMember);
            }
            else if (!IsLeaf)
            {
                InsertCollidableInRightQuadrant(collidableMember);
            }
            else
            {   
                if (_splittableObjects.Count == NodeCapacity)
                {
                    InsertCollidableInRightQuadrant(collidableMember);
                    Split();
                }
                else
                {
                    _splittableObjects.Add(collidableMember);
                }
            }
        }

        private void InsertPositionalOnly(SpatialTreeMember positionalMember)
        {
            positionalMember.RecalculateQuadrants(_splitAxes);
            if (!IsLeaf)
            {
                InsertPositionalOnlyInRightQuadrant(positionalMember);
            }
            else
            {
                if (_splittableObjects.Count == NodeCapacity)
                {
                    InsertPositionalOnlyInRightQuadrant(positionalMember);
                    Split();
                }
                else
                {
                    _splittableObjects.Add(positionalMember);
                }
            }
        }

        private void InsertInRightQuadrant(SpatialTreeMember treeMember)
        {
            if (treeMember.IsPointOnly())
            {
                InsertPositionalOnlyInRightQuadrant(treeMember);
            }
            else
            {
                InsertCollidableInRightQuadrant(treeMember);
            }
        }

        private void InsertCollidableInRightQuadrant(SpatialTreeMember collidableMember)
        {
            if (collidableMember.IsInQ1)
            {
                Child1.InsertCollidabale(collidableMember);
            }
            else if (collidableMember.IsInQ2)
            {
                Child2.InsertCollidabale(collidableMember);
            }
            else if (collidableMember.IsInQ3)
            {
                Child3.InsertCollidabale(collidableMember);
            }
            else if (collidableMember.IsInQ4)
            {
                Child4.InsertCollidabale(collidableMember);
            }
            else
            {
                Console.WriteLine("WARNING: Failed to insert a collidable SpatialTreeMember into the SpatialTree!");
            }
        }

        private void InsertPositionalOnlyInRightQuadrant(SpatialTreeMember positionalMember)
        {
            if (positionalMember.IsInQ1)
            {
                Child1.InsertCollidabale(positionalMember);
            }
            else if (positionalMember.IsInQ2)
            {
                Child2.InsertCollidabale(positionalMember);
            }
            else if (positionalMember.IsInQ3)
            {
                Child3.InsertCollidabale(positionalMember);
            }
            else if (positionalMember.IsInQ4)
            {
                Child4.InsertCollidabale(positionalMember);
            }
            else
            {
                Console.WriteLine("WARNING: Failed to insert a collidable SpatialTreeMember into the SpatialTree!");
            }
        }

        // Constructs the children if they are null and moves all splittable objects into their correct quadrant
        public void Split()
        {
            // This creates the children if they have not yet been created.
            if (Child1 == null)
            {
                Vector2f size = new Vector2f((RightBound - LeftBound) / 2f, (BottomBound - TopBound) / 2f);
                Child1 = new SpatialTree(new FloatRect(new Vector2f(LeftBound + size.X, TopBound + size.Y), size), this);
                Child2 = new SpatialTree(new FloatRect(new Vector2f(LeftBound, TopBound + size.Y), size), this);
                Child3 = new SpatialTree(new FloatRect(new Vector2f(LeftBound, TopBound), size), this);
                Child4 = new SpatialTree(new FloatRect(new Vector2f(LeftBound + size.X, TopBound), size), this);
            }
            IsLeaf = false;

            // Insert each object in _splittableObjects in the right quadrant then delete remove it from _splittableObjects.
            for (int i = _splittableObjects.Count - 1; i > 0; i--)
            {
                InsertInRightQuadrant(_splittableObjects[i]);
            }
            _splittableObjects.Clear();
        }

        // This deletes an object using its NodePointer if available, or by performing a search delete if not.
        public void Delete(GameObject pObject)
        {
            if (!pObject.TreeNodePointer._unsplittableObjects.Remove(pObject) && !pObject.TreeNodePointer._splittableObjects.Remove(pObject))
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

        // Use this when deleting to ensure that nodes whose children are empty leaves will merge.
        private void Merge()
        {
            // It all its children are empty leaves, make this a leaf.
            if (Child1 == null || Child1.IsEmptyLeaf() && Child2.IsEmptyLeaf() && Child3.IsEmptyLeaf() && Child4.IsEmptyLeaf())
            {
                IsLeaf = true;

                // Sometimes, a merge will trigger other merges. This accounts for those scenarios.
                if (_parent != null)
                {
                    _parent.Merge();
                }
            }
        }

        // Deletes the object, and inserts it into the nearest parent which can fully contain it.
        public static void Move (GameObject pObject, Vector2f newPos)
        {
            pObject.Position = newPos;

            // There are separate helper methods for collidable objects and point only objects to optimize each one separately.
            if (pObject.IsBroadcastingCollionLayers || pObject.IsCollisionCheckEnabled())
            {
                if (pObject.TreeNodePointer._parent == null || !pObject.TreeNodePointer.RectIsOutOfBounds(pObject.GetCollisionRect()))
                {
                    // If this is a leaf, the object does not need to be moved to a different node.
                    if (!pObject.TreeNodePointer.IsLeaf)
                    {
                        // If this is not a leaf, it needs to be deleted and reinserted it into the right child.
                        if (!pObject.TreeNodePointer._splittableObjects.Remove(pObject))
                        {
                            pObject.TreeNodePointer._unsplittableObjects.Remove(pObject);
                        }
                        pObject.TreeNodePointer.Insert(pObject, pObject.GetCollisionRect().Left, pObject.GetCollisionRect().Top, pObject.GetCollisionRect().Left 
                                + pObject.GetCollisionRect().Width, pObject.GetCollisionRect().Top + pObject.GetCollisionRect().Height);
                    }
                }
                else
                {
                    // If it is out of bounds, delete and try to move it into _parent.
                    pObject.TreeNodePointer.Delete(pObject);
                    pObject.TreeNodePointer._parent.Move(pObject, pObject.GetCollisionRect().Left, pObject.GetCollisionRect().Top, pObject.GetCollisionRect().Left 
                                    + pObject.GetCollisionRect().Width, pObject.GetCollisionRect().Top + pObject.GetCollisionRect().Height);
                }
            }
            else
            {
                if (pObject.TreeNodePointer._parent == null || !pObject.TreeNodePointer.PointIsOutOfBounds(newPos))
                {
                    // If the object does not need to be moved to a different node, don't bother deleting and reinserting it.
                    if (!pObject.TreeNodePointer.IsLeaf)
                    {
                        // If this is not a leaf, it needs to be deleted and reinserted it into the right child.
                        pObject.TreeNodePointer._unsplittableObjects.Remove(pObject);
                        pObject.TreeNodePointer.Insert(pObject, newPos.X, newPos.Y);
                    }
                }
                else
                {
                    // If it is out of bounds, delete and try to move it into _parent.
                    pObject.TreeNodePointer.Delete(pObject);
                    pObject.TreeNodePointer._parent.Move(pObject, newPos.X, newPos.Y);
                }
            }
        }
        private void Move (GameObject cObject, float left, float top, float right, float bottom)
        {
            if (_parent == null || !RectIsOutOfBounds(left, top, right, bottom))
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
            if (_parent == null || !PointIsOutOfBounds(x, y))
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
            if (IsLeaf)
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
                bool posX = cObject.GetCollisionRect().Left + cObject.GetCollisionRect().Width >= _splitAxes.Y;
                bool negX = cObject.GetCollisionRect().Left < _splitAxes.Y;
                bool posY = cObject.GetCollisionRect().Top + cObject.GetCollisionRect().Height >= _splitAxes.X;
                bool negY = cObject.GetCollisionRect().Top < _splitAxes.X;
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
            Child1.HandleCollisions(child1CheckList);
            Child2.HandleCollisions(child2CheckList);
            Child3.HandleCollisions(child3CheckList);
            Child4.HandleCollisions(child4CheckList);
        }

        // Each object in handle list is checked against check list and then added to check list if it is checking for collisions.
        private void HandleCollisionsInList(List<GameObject> handleList, List<GameObject> checkList)
        {
            for (int i = 0; i < handleList.Count; i++)
            {
                GameObject cObject = handleList[i];
                
                // Don't check for collisions if the object isn't even collidable.
                if (!cObject.IsBroadcastingCollionLayers)
                {
                    if (cObject.IsCollisionCheckEnabled())
                    {
                        checkList.Add(cObject);
                    }
                    continue;
                }

                // Check for collisions.
                foreach(var cObjectChecks in checkList)
                {
                    if (cObject.IsCollidingWith(cObjectChecks))
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

        // Checks if an object is both empty and is leaf is true. Used for determining whether to update is leaf when deleting.
        private bool IsEmptyLeaf()
        {
            return IsLeaf && _splittableObjects.Count == 0 && _unsplittableObjects.Count == 0;
        }

        private SpatialTree GetRightQuadrant(bool posX, bool posY)
        {
            if (posX)
            {
                if (posY)
                {
                    // Child1 if +X +Y
                    return Child1;
                }
                // Child4 if +X -Y
                return Child4;
            }
            if (posY)
            {
                // Child2 if -X +Y
                return Child2;
            }
            // Child3 if -X -Y
            return Child3;
        }
    
        // Returns true if a FloatRect is out of bounds.
        private bool RectIsOutOfBounds(FloatRect rect)
        {
            return (rect.Left <= LeftBound || rect.Top <= TopBound || rect.Left + rect.Width >= RightBound || rect.Top + rect.Height >= BottomBound);
        }

        // Returns true if a FloatRect (represented as lines) is out of bounds.
        private bool RectIsOutOfBounds(float left, float top, float right, float bottom)
        {
            return (left < LeftBound || top < TopBound || right > RightBound || bottom > BottomBound);
        }

        // Returns true if a point is out of bounds.
        private bool PointIsOutOfBounds(Vector2f point)
        {
            return (point.X < LeftBound || point.Y < TopBound || point.X > RightBound || point.Y < BottomBound);
        }

        // Returns true if a point (reperesented as coordinates) is out of bounds.
        private bool PointIsOutOfBounds(float x, float y)
        {
            return (x < LeftBound || y < TopBound || x > RightBound || y < BottomBound);
        }
    }
}