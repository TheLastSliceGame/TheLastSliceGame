using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TheLastSlice.Managers;
using TheLastSlice.Models;

namespace TheLastSlice.Entities
{
    //IN Ingredient, CU Cube, HE Heart, TC Recycle Can, BX Cardboard Box, GS Gas Can
    public enum PickupType { IN, GS, TC, HE, CU, BX };

    public class Pickup : Entity
    {
        public PickupType PickupType { get; protected set; }

        public Pickup(Vector2 position, String assetCode = null) : base(position)
        {
            Type = EntityType.Pickup;
            AssetCode = assetCode;
            PickupType type;
            Enum.TryParse(assetCode, out type);
            PickupType = type;

            if(PickupType != PickupType.IN)
            {
                if (assetCode != null)
                {
                    switch (PickupType)
                    {
                        case PickupType.GS:
                            SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Gas");
                            break;
                        case PickupType.TC:
                            SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Recycle");
                            break;
                        case PickupType.CU:
                            SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/CubePickup");
                            break;
                        case PickupType.BX:
                            //A cardboard box, huh. Just like Zanzibar.
                            SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Box");
                            break;
                        default:
                            SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Pickup");
                            break;
                    }
                }
            }
        }

        public override void LoadTexture()
        {
            if (AssetCode != null)
            {
                int currentLevel = TheLastSliceGame.LevelManager.CurrentLevelNum;
                String assetPath = "Entity/Level" + currentLevel.ToString() + "/Pickups/" + AssetCode;
                Texture2D pickupTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath);

                //Heart is the only obstacle that has animations
                if (AssetCode == "HE")
                {
                    Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
                    animations.Add("IDLE", new Animation(pickupTexture, 4));
                    Animations = new Dictionary<string, Animation>(animations);
                    AnimationManager = new AnimationManager(Animations.First().Value);
                    Height = (Animations.First().Value.FrameHeight);
                    Width = (Animations.First().Value.FrameWidth);
                }
                else if (pickupTexture != null)
                {
                    Texture = pickupTexture;
                    Height = pickupTexture.Height;
                    Width = pickupTexture.Width;
                }
                else
                {
                    Debug.WriteLine(" *** ERROR - No asset found for asset code {0}", AssetCode);
                }

                IsBlocking = false;
                CollisionComponent = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            }
        }

        public override void OnCollided(Entity collidedWith)
        {
            Player player = collidedWith as Player;
            if (player != null)
            {
                if (SoundEffect != null)
                {
                    SoundEffect.Play();
                }

                TheLastSliceGame.MapManager.CurrentMap.AddEntityForRemoval(this);
            }
        }

        public virtual int GetScore()
        {
            //The cake is a lie
            switch(PickupType)
            {
                case PickupType.TC:
                {
                    return 500;
                }
                case PickupType.GS:
                {
                    return 100;
                }
                case PickupType.CU:
                {
                    return 1000;
                }
                case PickupType.BX:
                {
                    return 500;
                }
                default:
                {
                    return 10;
                }
            }
        }

        protected override void SetAnimations()
        {
            if (PickupType == PickupType.HE)
            {
                AnimationManager.Play(Animations["IDLE"]);
            }
        }

        public static bool isPickupType(PickupType pickupType, string assetCode)
        {
            return assetCode == pickupType.ToString();
        }
    }
}
