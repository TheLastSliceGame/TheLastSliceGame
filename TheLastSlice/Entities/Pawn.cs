using Microsoft.Xna.Framework;
using System;

namespace TheLastSlice.Entities
{
    public class Pawn : Entity
    {
        //Pawns can move - H.E.
        public Vector2 Velocity { get; protected set; }
        public Vector2 OldVelocity { get; protected set; }
        public Vector2 OldPosition { get; private set; }
        protected float Speed { get; set; }

        public Pawn(Vector2 position, String assetCode = null) : base(position)
        {
            Speed = 200.0f;
            Velocity = Vector2.Zero;
            OldPosition = position;
            CollisionComponent = new Rectangle((int)position.X, (int)position.Y, Width, Height);
        }

        public override Vector2 GetPositionOnGrid()
        {
            int posX = (int)Math.Ceiling(Position.X + HalfWidth) / TheLastSliceGame.Instance.EntityWidth;
            int posY = (int)Math.Ceiling(Position.Y + HalfHeight - TheLastSliceGame.MapManager.MapStartingYPos) / TheLastSliceGame.Instance.EntityHeight;

            return new Vector2(posX, posY);
        }

        public virtual void Move(GameTime gameTime)
        {
            if (OldPosition != Position)
            {
                TheLastSliceGame.MapManager.CurrentMap.AddMovedPawn(this);
            }
        }

        public override void PostUpdate()
        {
            Velocity = Vector2.Zero; // Comment this out for some sweet PAC MAN controls y'all - H.E.
            OldPosition = Position;
        }

        public virtual void OnBlockingCollisionResolvePosition(Entity collidedWith)
        {

        }
    }
}