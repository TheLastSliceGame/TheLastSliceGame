using com.bitbull.meat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using TheLastSlice.Entities;

namespace TheLastSlice.UI
{
    public class GameplayHUD : UIScreen
    {
        private const int GasLength = 197;
        private const int GasFullXPos = 585;

        private enum OrderState { IN, WAIT, OUT, GONE };
        private TimeSpan TransitionTime { get; set; }

        private Texture2D HUDBackground { get; set; }
        private Texture2D HUDInfo { get; set; }
        private Texture2D HUDEmptySlot { get; set; }
        private Texture2D HUDCorrectSlot { get; set; }

        private Texture2D HUDGasGreen { get; set; }
        private Texture2D HUDGasYellow { get; set; }
        private Texture2D HUDGasRed { get; set; }
        private Texture2D HUDGasCan { get; set; }

        private Texture2D HUDOrder { get; set; }

        private int GasYPos { get; set; }
        private int GasXPos { get; set; }

        private Vector2 OrderPos { get; set; }

        private Vector2 OrderPosNow { get; set; }
        private Vector2 OrderPosStart { get; set; }

        private OrderState CurrentOrderState { get; set; }
        private Lerper Lerper { get; set; }

        private bool Printed { get; set; }

        private SoundEffect PrintSound { get; set; }
        private SoundEffectInstance PrintSoundInstance { get; set; }

        private List<IngredientType> CorrectDeliveryIngredients;

        public GameplayHUD()
        {
            CorrectDeliveryIngredients = new List<IngredientType>();
        }

        public override void LoadContent()
        {
            HUDEmptySlot = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/empty");
            HUDCorrectSlot = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/collected-ingredient");
            HUDBackground = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/window");

            HUDGasCan = TheLastSliceGame.Instance.Content.Load<Texture2D>("Entity/Level1/Pickups/GS");
            HUDGasGreen = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/gas-slider-green");
            HUDGasYellow = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/gas-slider-yellow");
            HUDGasRed = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/gas-slider-red");

            HUDOrder = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/order");

            PrintSound = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/OrderUp/dotmatrix");
            PrintSoundInstance = PrintSound.CreateInstance();
            Printed = false;

            GasYPos = 19;
            GasXPos = 585;
            OrderPos = new Vector2(620, 50);

            Lerper = new Lerper();

            CurrentOrderState = OrderState.GONE;
            OrderPosStart = new Vector2(OrderPos.X, OrderPos.Y - HUDOrder.Height);
            OrderPosNow = OrderPosStart;

            Lerper.Acceleration = 1f;
            Lerper.Amount = 0.01f;
            Lerper.MinVelocity = 4f;
            Lerper.MaxVelocity = 5f;

            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            int numCurrentIngredients = TheLastSliceGame.Instance.Player.PizzaIngredients.Count;
            for (int i = 0; i < 4 && i < numCurrentIngredients; i++)
            {
                Ingredient ingredient = TheLastSliceGame.Instance.Player.PizzaIngredients[i];
                ingredient.Update(gameTime);
            }
            UpdateGas(gameTime);
            UpdateOrderTicket(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TheLastSliceGame.Instance.IsMouseVisible = false;
            DrawOrderTicket(spriteBatch);
            DrawGas(spriteBatch);
            spriteBatch.Draw(HUDBackground, new Vector2(0, 0), Color.Black);

            spriteBatch.DrawString(Text, "Level", new Vector2(10, 5), Color.White);
            spriteBatch.DrawString(Text, string.Format("{0}", TheLastSliceGame.LevelManager.CurrentLevelNum), new Vector2(80, 5), Color.White);

            spriteBatch.DrawString(Text, "Order", new Vector2(10, 20), Color.White);
            spriteBatch.DrawString(Text, string.Format("{0}", Math.Min(TheLastSliceGame.LevelManager.CurrentLevel.CompletedNumDeliveries + 1, TheLastSliceGame.LevelManager.CurrentLevel.TotalNumDeliveries)), new Vector2(80, 20), Color.White);
            spriteBatch.DrawString(Text, "Of", new Vector2(105, 20), Color.White);
            if (TheLastSliceGame.LevelManager.CurrentLevelNum > 2)
            {
                spriteBatch.DrawString(Text, "99", new Vector2(135, 20), Color.White); // jk
            }
            else
            {
                spriteBatch.DrawString(Text, TheLastSliceGame.LevelManager.CurrentLevel.TotalNumDeliveries.ToString(), new Vector2(135, 20), Color.White);
            }

            spriteBatch.Draw(HUDEmptySlot, new Vector2(175, 0), Color.White);
            spriteBatch.Draw(HUDEmptySlot, new Vector2(225, 0), Color.White);
            spriteBatch.Draw(HUDEmptySlot, new Vector2(275, 0), Color.White);
            spriteBatch.Draw(HUDEmptySlot, new Vector2(325, 0), Color.White);

            spriteBatch.DrawString(Text, "Score", new Vector2(425, 5), Color.White);
            int scorePos = 455 - (5 * TheLastSliceGame.Instance.Player.Score.ToString().Length - 1);
            spriteBatch.DrawString(Text, TheLastSliceGame.Instance.Player.Score.ToString(), new Vector2(scorePos, 20), Color.White);
            //spriteBatch.DrawString(Text, "Score: " + TheLastSliceGame.Instance.Player.Score.ToString(), new Vector2(380, 15), Color.White);

            spriteBatch.Draw(HUDGasCan, new Vector2(GasFullXPos - HUDGasCan.Width, 0), Color.White);

            DrawInventory(spriteBatch);
            PreviousMouseState = Mouse.GetState();
        }

        public void UpdateGas(GameTime gameTime)
        {
            float percentFull = (float)TheLastSliceGame.Instance.Player.Gas / TheLastSliceGame.Instance.Player.MaxGas;
            GasXPos = GasFullXPos - GasLength + (int)(percentFull * GasLength);
        }

        public void UpdateOrderTicket(GameTime gametime)
        {
            switch(CurrentOrderState)
            {
                case OrderState.IN:
                    OrderPosNow = new Vector2(OrderPos.X, Lerper.Lerp(OrderPosNow.Y, OrderPos.Y));
                    if(OrderPosNow.Equals(OrderPos))
                    {
                        TransitionTime = TimeSpan.Zero;
                        CurrentOrderState = OrderState.WAIT;
                    }
                    break;
                case OrderState.WAIT:
                    if (!Printed)
                    {
                        PrintSoundInstance.Play();
                        Printed = true;
                    }
                    if (TransitionTime == TimeSpan.Zero)
                    {
                        TransitionTime = TimeSpan.FromMilliseconds(gametime.TotalGameTime.TotalMilliseconds + 3000);
                    }
                    if (gametime.TotalGameTime.TotalMilliseconds > TransitionTime.TotalMilliseconds)
                    {
                        TransitionTime = TimeSpan.Zero;
                        CurrentOrderState = OrderState.OUT;
                    }
                    break;
                case OrderState.OUT:
                    OrderPosNow = new Vector2(OrderPos.X, Lerper.Lerp(OrderPosNow.Y, OrderPosStart.Y));
                    if (OrderPosNow.Equals(OrderPosStart))
                    {
                        CurrentOrderState = OrderState.GONE;
                    }
                    break;
                default:
                    break;
            }
        }

        public void DrawGas(SpriteBatch spriteBatch)
        {
            if(TheLastSliceGame.Instance.Player.Gas > (TheLastSliceGame.Instance.Player.MaxGas / 2))
            {
                spriteBatch.Draw(HUDGasGreen, new Vector2(GasXPos, GasYPos), Color.White);
            }
            else if (TheLastSliceGame.Instance.Player.Gas < TheLastSliceGame.Instance.Player.LowGasThreshold)
            {
                spriteBatch.Draw(HUDGasRed, new Vector2(GasXPos, GasYPos), Color.White);
            }
            else
            {
                spriteBatch.Draw(HUDGasYellow, new Vector2(GasXPos, GasYPos), Color.White);
            }
        }

        public void DrawInventory(SpriteBatch spriteBatch)
        {
            //Ingredients/Toppings
            int playerIngredientsXPos = 175;
            int numCurrentIngredients = TheLastSliceGame.Instance.Player.PizzaIngredients.Count;
            List<IngredientType> correctDrawnIngredients = new List<IngredientType>();

            for (int i = 0; i < 4 && i < numCurrentIngredients; i++)
            {               
                Ingredient ingredient = TheLastSliceGame.Instance.Player.PizzaIngredients[i];
                Vector2 newPos = new Vector2(playerIngredientsXPos, 0);

                if (ingredient.Texture != null)
                {
                    spriteBatch.Draw(ingredient.Texture, newPos, Color.White);
                }
                else if (ingredient.AnimationManager != null)
                {
                    ingredient.AnimationManager.Position = newPos;
                    ingredient.AnimationManager.Draw(spriteBatch);
                }

                if (CorrectDeliveryIngredients.Contains(ingredient.IngredientType) && !correctDrawnIngredients.Contains(ingredient.IngredientType))
                {
                    correctDrawnIngredients.Add(ingredient.IngredientType);
                    spriteBatch.Draw(HUDCorrectSlot, new Vector2(playerIngredientsXPos, 0), Color.White);
                }

                playerIngredientsXPos += 50;
            }
        }

        public void DrawOrderTicket(SpriteBatch spriteBatch)
        {
            int deliveryIngredientsYPos = 78;
            spriteBatch.Draw(HUDOrder, OrderPosNow, Color.White);
            foreach (Ingredient ingredient in TheLastSliceGame.LevelManager.CurrentLevel.CurrentDelivery.Pizza)
            {
                string ingredientName = Ingredient.ToIngredientName(ingredient.IngredientType);
                Vector2 ingredientSize = Text.MeasureString(ingredientName);
                Vector2 ingredientPos = new Vector2(OrderPosNow.X + HUDOrder.Width/2 - ingredientSize.X/2, OrderPosNow.Y + deliveryIngredientsYPos - ingredientSize.Y/2);
                spriteBatch.DrawString(Text, ingredientName, ingredientPos, Color.Black);

                deliveryIngredientsYPos += 25;
            }
        }

        public void OnDeliveryComplete()
        {
            CurrentOrderState = OrderState.IN;
            OrderPosNow = OrderPosStart;
            Printed = false;
        }

        public void OnNewDelivery()
        {
            CorrectDeliveryIngredients.Clear();
            foreach (Ingredient ingredient in TheLastSliceGame.LevelManager.CurrentLevel.CurrentDelivery.Pizza)
            {
                CorrectDeliveryIngredients.Add(ingredient.IngredientType);
            }
        }

        public void OnLevelLoaded()
        {
            CurrentOrderState = OrderState.IN;
            OrderPosNow = OrderPosStart;
            Printed = false;
            TheLastSliceGame.Instance.PlayMusic();
        }
    }
}
