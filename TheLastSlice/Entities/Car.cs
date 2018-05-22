using Microsoft.Xna.Framework;
using System;

namespace TheLastSlice.Entities
{
    public class Car : Pawn
    { 
        public Car(Vector2 position, String assetCode = null) : base(position, assetCode)
        {
            IsBlocking = true;
            Speed = 2.0f;
        }

        public override void Update(GameTime gameTime)
        {
            Move(gameTime);
            base.Update(gameTime);
        }

        public override void Move(GameTime gameTime)
        {
            //If a car can't move what is the point?
            base.Move(gameTime);
        }
    }
}
