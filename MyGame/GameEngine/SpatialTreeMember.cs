using SFML.System;

namespace GameEngine
{
    // This is an object that belongs on the SpatialTree.
    // Note that this is not a GameObject. Rather, it is a sort of "bridge" between the SpatialTree and a GameObject.
    // Specifically, it contains information that is defined by features in a GameObject, but is only relevant to the SpatialTree.
    struct SpatialTreeMember
    {
        public GameObject InternalObject { get; set; }
        public bool IsInQ1 { get; private set; }
        public bool IsInQ2 { get; private set; }
        public bool IsInQ3 { get; private set; }
        public bool IsInQ4 { get; private set; }
        public SpatialTreeMember(GameObject internalObject, Vector2f splitAxes)
        {
            InternalObject = internalObject;
            IsInQ1 = false;
            IsInQ2 = false;
            IsInQ3 = false;
            IsInQ4 = false;
            RecalculateQuadrants(splitAxes);
        }
        public void RecalculateQuadrants(Vector2f splitAxes)
        {
            if (IsPointOnly())
            {
                bool posX = InternalObject.Position.X >= splitAxes.Y;
                bool posY = InternalObject.Position.Y >= splitAxes.X;
                IsInQ1 = posX && posY;
                IsInQ2 = !posX && posY;
                IsInQ3 = !posX && !posY;
                IsInQ4 = posX && !posY;
            }
            else
            {
                bool posX = InternalObject.Position.X + InternalObject.GetCollisionRect().Width >= splitAxes.Y;
                bool posY = InternalObject.Position.Y + InternalObject.GetCollisionRect().Height >= splitAxes.X;
                bool negX = InternalObject.Position.X < splitAxes.Y;
                bool negY = InternalObject.Position.Y < splitAxes.X;
                IsInQ1 = posX && posY;
                IsInQ2 = negX && posY;
                IsInQ3 = negX && negY;
                IsInQ4 = posX && negY; 
            }
        }
        public bool IsOutOfBounds(float left, float top, float right, float bottom)
        {
            if (IsPointOnly())
            {
                return InternalObject.Position.X <= left || InternalObject.Position.Y <= top || InternalObject.Position.X > right || InternalObject.Position.Y > bottom;
            }
            else
            {
                return InternalObject.Position.X < left || InternalObject.Position.Y < top || 
                InternalObject.Position.X + InternalObject.GetCollisionRect().Width > right || 
                InternalObject.Position.Y + InternalObject.GetCollisionRect().Height > bottom;
            }
        }
        public bool IsPointOnly()
        {
            return InternalObject.GetCollisionRect == null;
        }
        public bool IsSplittable()
        {
            return IsPointOnly() || (IsInQ1 ^ IsInQ2) ^ (IsInQ3 ^ IsInQ4);
        }
    }
}