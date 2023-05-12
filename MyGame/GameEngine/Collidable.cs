using SFML.Graphics;

// This is an interface. An interface is like a contract, with any object that implements it agreeing to do something.
namespace GameEngine
{
    // This represents any objects that can be collided with.
    public interface Collidable
    {
        // Used to determine whether this checks for collisions itself or simply can be collided with.
        public bool ChecksForCollisions
        {
            get;
        }

        // Used to get the collision bounds.
        public FloatRect CollisionRect
        {
            get;
        }

        // Called whenever this collides with something else.
        public void HandleCollision(Collidable otherCollidable);
    }
}