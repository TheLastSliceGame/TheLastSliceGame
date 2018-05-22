using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TheLastSlice.Managers;
using TheLastSlice.Models;

namespace TheLastSlice.Entities
{
    public enum EntityType { None, Road, House, Pickup, Car, Ingredient, Obstacle };

    public class Entity
    {
        public Guid Guid { get; private set; }
        public int Width { get; protected set; }
        public int Height { get; protected set; }
        public int HalfWidth { get { return Width / 2; } }
        public int HalfHeight { get { return Height / 2; } }
        public bool IsBlocking { get; protected set; }
        public bool Hidden { get; set; }
        public Rectangle CollisionComponent { get; protected set; }
        public EntityType Type { get; protected set; }
        public Map Map { get; set; }
        public Texture2D Texture { get; protected set; }
        public AnimationManager AnimationManager;

        protected Dictionary<string, Animation> Animations;
        protected SoundEffect SoundEffect { get; set; }
        protected String AssetCode { get; set; }
        protected Vector2 m_Position;

        public Entity(Vector2 position = default(Vector2), String assetCode = null)
        {
            Type = EntityType.None;
            Guid = System.Guid.NewGuid();
            Position = position;
            IsBlocking = false;
            Hidden = false;
            CollisionComponent = Rectangle.Empty;

            if(assetCode != null)
            {
                Texture2D texture = TheLastSliceGame.Instance.Content.Load<Texture2D>("Entity/Misc/" + assetCode);

                if (texture != null)
                {
                    Texture = texture;
                    Height = texture.Height;
                    Width = texture.Width;
                }
            }
        }

        public virtual Vector2 Position
        {
            get { return m_Position; }
            set
            {
                //When the position changes we need to update the collision component as well. - H.E.
                m_Position = value;

                CollisionComponent = new Rectangle((int)value.X, (int)value.Y, Width, Height);

                if (AnimationManager != null)
                {
                    AnimationManager.Position = m_Position;
                }
            }
        }

        public virtual Vector2 GetPositionOnGrid()
        {
            int posX = (int)Math.Ceiling(m_Position.X)/ TheLastSliceGame.Instance.EntityWidth;
            int posY = (int)Math.Ceiling(m_Position.Y - TheLastSliceGame.MapManager.MapStartingYPos) / TheLastSliceGame.Instance.EntityHeight;

            return new Vector2(posX, posY);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!Hidden)
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

            //Comment this out if you want to see the collision boxes - H.E.
            //if (CollisionComponent != Rectangle.Empty)
            //{
            //    TheLastSliceGame.Instance.DrawDebugBorder(CollisionComponent, 2, Color.HotPink);
            //}
        }

        protected virtual void SetAnimations()
        {
        }

        public virtual void AddAnimations(String name, Animation anim)
        {
            if(Animations == null)
            {
                Animations = new Dictionary<string, Animation>();
            }

            Animations.Add(name, anim);
            
            if (AnimationManager == null)
            {
                AnimationManager = new AnimationManager(anim);
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

        public bool IsColliding(Entity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return CollisionComponent.Intersects(entity.CollisionComponent);
        }
        
        public bool IsColliding(Entity entity, out Rectangle overlap)
        {
            bool collided = false;
            overlap = Rectangle.Empty;
            if (entity == null)
            {
                return collided;
            }
            overlap = Rectangle.Intersect(CollisionComponent, entity.CollisionComponent);
            if(overlap != Rectangle.Empty)
            {
                collided = true;
            }
            return collided;
        }

        public virtual void OnCollided(Entity collidedWith)
        {

        }

        public virtual void OnAddedToMap()
        {
            LoadTexture();
            SetAnimations();
        }

        public virtual void LoadTexture()
        {

        }
        
        public virtual void AddAnimations()
        {

        }
    }
}
