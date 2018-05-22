using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheLastSlice.Entities
{
    public enum PizzaDeliveryState { None, Ordered, CurrentDelivery, Success, Failed };

    public class House : Entity
    {
        public List<Ingredient> Pizza { get; private set; }
        public PizzaDeliveryState DeliveryState { get; private set; }
        
        public House(Vector2 position, String assetCode) : base(position)
        {
            AssetCode = assetCode;
            Type = EntityType.House;
            IsBlocking = true;
            DeliveryState = PizzaDeliveryState.None;
            Pizza = new List<Ingredient>();
        }

        public override void LoadTexture()
        {
            int currentLevel = TheLastSliceGame.LevelManager.CurrentLevelNum;
            String assetPath = "Entity/Level" + currentLevel.ToString() + "/Tiles/" + AssetCode;
            Texture2D houseTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath);

            if (houseTexture != null)
            {
                Texture = houseTexture;
                Height = houseTexture.Height;
                Width = houseTexture.Width;
            }
            else
            {
                Debug.WriteLine(" *** ERROR - No asset found for asset code {0}", AssetCode);
            }

            CollisionComponent = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public void ResetDeliveryState()
        {
            DeliveryState = PizzaDeliveryState.None;
        }

        public void SetAsCurrentDelivery()
        {
            DeliveryState = PizzaDeliveryState.CurrentDelivery;
        }

        public void OnPizzaOrdered(List<Ingredient> pizza)
        {
            //House: "Hi, I'd like to order an apple, anchovy, onion pizza please. No cheese. With extra onions."
            //Pizza Time: "You're a monster. Don't ever call again." *Click*
            Pizza = pizza;
            DeliveryState = PizzaDeliveryState.Ordered;
        }

        public void DeliveryComplete(bool deliverySuccess)
        {
            DeliveryState = deliverySuccess ? PizzaDeliveryState.Success : PizzaDeliveryState.Failed;
        }
    }
}
