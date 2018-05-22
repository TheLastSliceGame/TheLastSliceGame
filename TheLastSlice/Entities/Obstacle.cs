using com.bitbull.meat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheLastSlice.Managers;
using TheLastSlice.Models;

namespace TheLastSlice.Entities
{
    //FI Fire, PH Pothole
    public enum ObstacleType { FI, PH, PB };
    public class Obstacle : Entity
    {
        public ObstacleType ObstacleType { get; private set; }
        public int PBTravelDistanceX { get; set; }

        private Vector2 PBInitialPos;
        private enum PBDirection { LEFT, RIGHT};
        private PBDirection m_PBDirection;
        private Lerper Lerper;
        private float LerperAccelerationSlow;
        private float LerperMinVelocitSlow;
        private float LerperMaxVelocitSlow;
        private float LerperAccelerationFast;
        private float LerperMinVelocitFast;
        private float LerperMaxVelocitFast;

        public Obstacle(Vector2 position, String assetCode) : base(position)
        {
            Type = EntityType.Obstacle;
            AssetCode = assetCode;
            ObstacleType type;
            Enum.TryParse(assetCode, out type);
            ObstacleType = type;
            IsBlocking = true;
            PBTravelDistanceX = 2 * TheLastSliceGame.Instance.EntityWidth;
            Lerper = new Lerper();
            Lerper.Amount = 0.05f;
            LerperAccelerationFast = 1f;
            LerperMinVelocitFast = 3f;
            LerperMaxVelocitFast = 5f;
            LerperAccelerationSlow = LerperAccelerationFast / 2;
            LerperMinVelocitSlow = LerperMinVelocitFast / 2;
            LerperMaxVelocitSlow = LerperMaxVelocitFast / 2 ;

            Lerper.Acceleration = LerperAccelerationSlow;
            Lerper.MinVelocity = LerperMinVelocitSlow;
            Lerper.MaxVelocity = LerperMaxVelocitSlow;
        }

        public override void LoadTexture()
        {
            int currentLevel = TheLastSliceGame.LevelManager.CurrentLevelNum;
            String assetPath = "Entity/Level" + currentLevel.ToString() + "/Obstacles/" + AssetCode;

            //Fire is the only obstacle that has animations
            if (AssetCode == "FI")
            {
                Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
                Texture2D obstacleTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath);
                animations.Add("IDLE", new Animation(obstacleTexture, 4));
                Animations = new Dictionary<string, Animation>(animations);
                AnimationManager = new AnimationManager(Animations.First().Value);
                Height = (Animations.First().Value.FrameHeight);
                Width = (Animations.First().Value.FrameWidth);
            }
            else if (AssetCode == "PB") //Paper Boi, Paper Boi All about that paper, boy
            {
                Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
                Texture2D boyRightTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>("Entity/Level2/Vehicles/paperboy_right");
                Texture2D boyLeftTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>("Entity/Level2/Vehicles/paperboy_left");
                animations.Add("RIGHT", new Animation(boyRightTexture, 4));
                animations.Add("LEFT", new Animation(boyLeftTexture, 4));
                Animations = new Dictionary<string, Animation>(animations);
                AnimationManager = new AnimationManager(Animations.First().Value);
                Height = (Animations.First().Value.FrameHeight);
                Width = (Animations.First().Value.FrameWidth);
                PBInitialPos = Position;
            }
            else if (AssetCode == "PH")
            {
                Texture2D obstacleTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath);
                Texture = obstacleTexture;
                Height = obstacleTexture.Height;
                Width = obstacleTexture.Width;
            }
            else
            {
                Debug.WriteLine(" *** ERROR - No asset found for asset code {0}", AssetCode);
            }

            CollisionComponent = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public override void Update(GameTime gameTime)
        {
            if (ObstacleType == ObstacleType.PB)
            {
                if (TheLastSliceGame.LevelManager.CurrentLevelNum > 2)
                {
                    Lerper.Acceleration = LerperAccelerationFast;
                    Lerper.MinVelocity = LerperMinVelocitFast;
                    Lerper.MaxVelocity = LerperMaxVelocitFast;
                }

                switch (m_PBDirection)
                {
                    case PBDirection.LEFT:
                        Position = new Vector2(Lerper.Lerp(Position.X, PBInitialPos.X), Position.Y);
                        break;
                    case PBDirection.RIGHT:
                        Position = new Vector2(Lerper.Lerp(Position.X, PBInitialPos.X + PBTravelDistanceX), Position.Y);
                        break;
                    default:
                        break;
                }

                if(Position.X >= PBInitialPos.X + PBTravelDistanceX)
                {
                    m_PBDirection = PBDirection.LEFT;
                    AnimationManager.Play(Animations["LEFT"]);
                }

                if (Position.X <= PBInitialPos.X)
                {
                    m_PBDirection = PBDirection.RIGHT;
                    AnimationManager.Play(Animations["RIGHT"]);
                }

                CollisionComponent = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);

                if(IsColliding(TheLastSliceGame.Instance.Player))
                {
                    TheLastSliceGame.Instance.Player.OnCollided(this);
                    CollisionComponent = new Rectangle(0, 0, Width, Height);
                }

            }

            base.Update(gameTime);
        }

        protected override void SetAnimations()
        {
            if (ObstacleType == ObstacleType.FI)
            {
                AnimationManager.Play(Animations["IDLE"]);
            }

            if (ObstacleType == ObstacleType.PB)
            {
                AnimationManager.Play(Animations["RIGHT"]);
                m_PBDirection = PBDirection.RIGHT;
            }
        }
    }
}
