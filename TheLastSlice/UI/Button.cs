using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TheLastSlice.Managers;
using TheLastSlice.Models;

namespace TheLastSlice.UI
{
    public class Button
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public Rectangle ButtonRectangle { get; set; }
        public AnimationManager AnimationManager;

        private Texture2D Texture { get; set; }
        private String TextureName { get; set; }
        private Color[] ButtonData { get; set; }

        protected Dictionary<string, Animation> Animations;
        protected Vector2 m_Position;

        public Button(int xPos, int yPos, Texture2D UITexture)
        {
            Position = new Vector2((int)xPos, (int)yPos);
            Texture = UITexture;
            Width = Texture.Width;
            Height = Texture.Height;
            ButtonRectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public Button(int xPos, int yPos, int width, int height)
        {
            Position = new Vector2((int)xPos, (int)yPos);
            Width = width;
            Height = height;
            ButtonRectangle = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public virtual Vector2 Position
        {
            get { return m_Position; }
            set
            {
                //When the position changes we need to update the collision component as well. - H.E.
                m_Position = value;
                
                if (AnimationManager != null)
                {
                    AnimationManager.Position = m_Position;
                }
            }
        }

        public void LoadContent()
        {
       
        }

        public void Draw(SpriteBatch spriteBatch)
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
            if (AnimationManager != null)
            {
                AnimationManager.Position = Position;
                AnimationManager.Update(gameTime);
            }
        }
    }
}
