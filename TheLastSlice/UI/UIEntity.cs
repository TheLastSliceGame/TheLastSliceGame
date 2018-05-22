using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;
using TheLastSlice.Managers;
using TheLastSlice.Models;

namespace TheLastSlice.UI
{
    public enum EntityType { None, Road, House, Pickup, Car, Ingredient, Obstacle };

    public class UIEntity
    {
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int HalfWidth { get { return Width / 2; } }
        public int HalfHeight { get { return Height / 2; } }
        public Texture2D Texture { get; protected set; }
        public AnimationManager AnimationManager;

        protected Dictionary<string, Animation> Animations;
        protected Vector2 m_Position;

        public UIEntity(Vector2 position, Texture2D texture)
        {
            Position = position;
            Texture = texture;
            Height = texture.Height;
            Width = texture.Width;
        }

        public UIEntity(Vector2 position, Animation animation)
        {
            Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
            animations.Add("IDLE", animation);
            Animations = new Dictionary<string, Animation>(animations);
            AnimationManager = new AnimationManager(Animations.First().Value);
            Height = (Animations.First().Value.FrameHeight);
            Width = (Animations.First().Value.FrameWidth);
            AnimationManager.Position = position;
        }

        public virtual Vector2 Position
        {
            get { return m_Position; }
            set
            {
                m_Position = value;

                if (AnimationManager != null)
                {
                    AnimationManager.Position = m_Position;
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Texture != null)
            {
                spriteBatch.Draw(Texture, Position, Color.White);
            }
            else if (AnimationManager != null)
            {
                AnimationManager.Draw(spriteBatch);
            }

        }

        public virtual void Update(GameTime gameTime)
        {
            if(AnimationManager != null)
            {
                AnimationManager.Position = Position;
                AnimationManager.Update(gameTime);
            }
        }

        public virtual void PostUpdate()
        {
        }
        
    }
}
