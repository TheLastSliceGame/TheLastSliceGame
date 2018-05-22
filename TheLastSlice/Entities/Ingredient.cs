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
    //Ingredients/Toppings

    //AN Anchovy, BA Bacon, CH Cheese, FRR Frog Left, 
    //FRL Frog Right, GA Garlic, GB Green Peppers, HB Habenero, 
    //JP Jalapeno, MR Mushrooms, OL Olives, ON Onions, 
    //PA Pineapple, PP Pepperoni, SA Sausage, TM Tomatoes
    public enum IngredientType
    {
        AN, BA, CH, FRR, FRL,
        GA, GB, HB, JP, MR, OL,
        ON, PA, PP, SA, TM
    };

    public class Ingredient : Pickup
    {
        public IngredientType IngredientType { get; private set; }

        public Ingredient(Vector2 position, String assetCode) : base(position)
        {
            AssetCode = assetCode;
            PickupType = PickupType.IN;
            IngredientType type;
            Enum.TryParse(assetCode, out type);
            IngredientType = type;
        }

        public override void LoadTexture()
        {
            int currentLevel = TheLastSliceGame.LevelManager.CurrentLevelNum;

            String assetPath = "";
            if (AssetCode == "UIFRL")
            {
                assetPath = "UI/" + AssetCode;
            }
            else
            {
                assetPath = "Entity/Level" + currentLevel.ToString() + "/Pickups/Ingredients/" + AssetCode;
            }
            
            Texture2D ingredientTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath);

            if (ingredientTexture != null)
            {
                int numberOfFrames = ingredientTexture.Width / ingredientTexture.Height;
                Texture = null;
                Dictionary<string, Animation> animations = new Dictionary<string, Animation>();
                animations.Add("IDLE", new Animation(ingredientTexture, numberOfFrames));
                Animations = new Dictionary<string, Animation>(animations);
                AnimationManager = new AnimationManager(Animations.First().Value);
                Height = (Animations.First().Value.FrameHeight);
                Width = (Animations.First().Value.FrameWidth);
            }
            else
            {
                Debug.WriteLine(" *** ERROR - No asset found for asset code {0}", AssetCode);
            }

            if (AssetCode == "FRL" || AssetCode == "FRR")
            {
                SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Frog-Hit");
            }
            else
            {
                SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Pickup");
            }

            CollisionComponent = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        protected override void SetAnimations()
        {
            if (IsFrog())
            {
                AnimationManager.Play(Animations["IDLE"]);
            }
        }

        public override int GetScore()
        {
            switch (IngredientType)
            {
                case IngredientType.FRL:
                case IngredientType.FRR:
                {
                    return 500;
                }
                default:
                {
                    return 50;
                }
            }
        }

        public override void OnCollided(Entity collidedWith)
        {
            Player player = collidedWith as Player;
            if (player != null)
            {
                if (!player.IsInventoryFull() && !(IsFrog() && player.HeartMode))
                {
                    if (SoundEffect != null)
                    {
                        SoundEffect.Play();
                    }

                    TheLastSliceGame.MapManager.CurrentMap.AddEntityForRemoval(this);
                }
            }
        }

        public static string ToIngredientName(IngredientType ingredientType)
        {
            switch (ingredientType)
            {
                case IngredientType.AN:
                    return "Anchovy";

                case IngredientType.BA:
                    return "Bacon";

                case IngredientType.CH:
                    return "Cheese";

                case IngredientType.GA:
                    return "Garlic";

                case IngredientType.GB:
                    return "Green Peppers";

                case IngredientType.HB:
                    return "Habenero";

                case IngredientType.JP:
                    return "Jalapeno";

                case IngredientType.MR:
                    return "Mushrooms";

                case IngredientType.OL:
                    return "Olives";

                case IngredientType.ON:
                    return "Onions";

                case IngredientType.PA:
                    return "Pineapple";

                case IngredientType.PP:
                    return "Pepperoni";

                case IngredientType.SA:
                    return "Sausage";

                case IngredientType.TM:
                    return "Tomatoes";

                default:
                    return null;
            }
        }

        public bool IsFrog()
        {
            return (IngredientType == IngredientType.FRL || IngredientType == IngredientType.FRR);
        }

    }
}
