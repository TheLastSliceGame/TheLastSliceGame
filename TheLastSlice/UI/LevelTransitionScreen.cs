using com.bitbull.meat;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace TheLastSlice.UI
{
    public class LevelTransitionScreen : UIScreen
    {
        protected SpriteFont Message { get; set; }

        private enum ScreenState {HOUSE, CAR, BUBBLE, ADD, WAIT};
        private ScreenState CurrentState { get; set; }    
        private TimeSpan TransitionTime { get; set; }
        private String LevelCompleteText { get; set; }
        private Vector2 LevelTextPosition { get; set; }
        private Texture2D Level1Scene { get; set; }
        private Texture2D Level1Car { get; set; }
        private Texture2D Level1Hill { get; set; }
        private Texture2D Level1Person { get; set; }
        private Texture2D Level1Pizza { get; set; }
        private Texture2D Level1Trash { get; set; }
        private Texture2D Level2Scene { get; set; }
        private Texture2D Level2Car { get; set; }
        private Texture2D Level2Hill { get; set; }
        private Texture2D Level2Person { get; set; }
        private Texture2D Level2Pizza { get; set; }
        private Texture2D Level2Trash { get; set; }
        private Texture2D Level3Scene { get; set; }
        private Texture2D Level3Car { get; set; }
        private Texture2D Level3Hill { get; set; }
        private Texture2D Level3Person { get; set; }
        private Texture2D Level3Trash { get; set; }
        private Texture2D Level3Light { get; set; }
        private Texture2D Level1Bubble { get; set; }
        private Texture2D Level2Bubble { get; set; }
        private Texture2D Level3Bubble { get; set; }
        private int TempScore { get; set; }
        private SoundEffect PointTotal { get; set; }
        private Vector2 CarPosition { get; set; }
        private Vector2 CarInitialPosition { get; set; }
        private Lerper Lerper { get; set; }
        private int BubbleLineOffset { get; set; }
        private int BubbleXCenter { get; set; }
        private int BubbleYCenter { get; set; }
        private int BubbleLine1Y { get; set; }
        private int BubbleLine2Y { get; set; }
        private int BubbleLine3Y { get; set; }
        private int Level1MessageNumber { get; set; }
        private int Level2MessageNumber { get; set; }
        private int Level3MessageNumber { get; set; }

        private String[][] Level1Messages = new string[][]
        {
           new string[] {"GET. IN. MY. BELLY."},
           new string[] {"COWABUNGA DUDE!"},
           new string[] {"WHAT A SPLENDID PIE.", "PIZZA PIZZA PIE."}
        };

        private String[][] Level2Messages = new string[][]
        {
           new string[] {"BOY OH BOY, YOU SURE,", "CAN HYDRATE A PIZZA!"},
           new string[] {"KEEP YOUR FRIENDS CLOSE,", "AND YOUR PIZZA CLOSER."},
           new string[] {"PROBLEMS COME AND GO", "BUT PIZZA IS FOREVER!"}
        };

        private String[][] Level3Messages = new string[][]
        {
            new string[] { "THANKS FOR THE PIZZA,", "YA FILTHY MAMMAL!"},
        };

        public LevelTransitionScreen()
        {
            BubbleLineOffset = 14;
            BubbleXCenter = 216;
            BubbleYCenter = 195;
            Lerper = new Lerper();
            CarInitialPosition = new Vector2(150, 0);
            LevelCompleteText = "New Map Unlocked!";
            TransitionTime = TimeSpan.Zero;
            TempScore = 0;
            CurrentState = ScreenState.HOUSE;
            Lerper.Acceleration = 1f;
            Lerper.Amount = 0.01f;
            Lerper.MinVelocity = 4f;
            Lerper.MaxVelocity = 5f;
            CarPosition = CarInitialPosition;
        }

        public override void LoadContent()
        {
            base.LoadContent();
            LevelTextPosition = new Vector2(TheLastSliceGame.Instance.GameWidth - 225, 25);
            Level1Scene = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/LevelTransitionScreen1");
            Level1Car = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-car1");
            Level1Hill = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-hill1");
            Level1Person = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-person1");
            Level1Pizza = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-pizza1");
            Level1Trash = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-trash1");
            Level1Bubble = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-bubble1");

            Level2Scene = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/LevelTransitionScreen2");
            Level2Car = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-car2");
            Level2Hill = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-hill2");
            Level2Person = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-person2");
            Level2Pizza = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-pizza2");
            Level2Trash = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-trash2");
            Level2Bubble = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-bubble2");

            Level3Scene = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/LevelTransitionScreen3");
            Level3Car = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-car3");
            Level3Hill = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-hill3");
            Level3Person = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery_address_5");
            Level3Trash = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-trash3");
            Level3Light = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-light3");
            Level3Bubble = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/delivery-bubble3");

            PointTotal = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/PointTotal");

            Text = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_12");
            Message = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_14");
        }

        public override void Update(GameTime gameTime)
        {
            switch (CurrentState)
            {
                case ScreenState.HOUSE:
                    SetTransitionTimeIfZero(gameTime, 1000);
                    if (gameTime.TotalGameTime.TotalMilliseconds > TransitionTime.TotalMilliseconds)
                    {
                        TransitionTime = TimeSpan.Zero;
                        CurrentState = ScreenState.CAR;

                    }
                    break;
                case ScreenState.CAR:
                    if (CarPosition == Vector2.Zero)
                    {
                        TransitionTime = TimeSpan.Zero;
                        CurrentState = ScreenState.BUBBLE;

                    }
                    UpdateCar(gameTime);
                    break;
                case ScreenState.BUBBLE:
                    SetTransitionTimeIfZero(gameTime, 1000);
                    if (gameTime.TotalGameTime.TotalMilliseconds > TransitionTime.TotalMilliseconds)
                    {
                        TransitionTime = TimeSpan.Zero;
                        CurrentState = ScreenState.ADD;

                    }
                    break;
                case ScreenState.ADD:
                    SetTransitionTimeIfZero(gameTime, 50);
                    if (TempScore >= TheLastSliceGame.Instance.Player.Score)
                    {
                        TransitionTime = TimeSpan.Zero;
                        CurrentState = ScreenState.WAIT;
                    }

                    int addToScore = 20;
                    if (TheLastSliceGame.Instance.Player.Score > 5000)
                    {
                        addToScore = 250;
                    }

                    TempScore = Math.Min(TempScore += addToScore, TheLastSliceGame.Instance.Player.Score);
                    if (TempScore < TheLastSliceGame.Instance.Player.Score && gameTime.TotalGameTime.TotalMilliseconds > TransitionTime.TotalMilliseconds)
                    {
                        PointTotal.Play();
                        TransitionTime = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 50);
                    }
                    break;
                case ScreenState.WAIT:
                    if (TransitionTime == TimeSpan.Zero)
                    {
                        TransitionTime = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 2000);
                    }
                    if (gameTime.TotalGameTime.TotalMilliseconds > TransitionTime.TotalMilliseconds)
                    {
                        TransitionTime = TimeSpan.Zero;
                        if (TheLastSliceGame.LevelManager.CurrentLevelNum - 1 >= TheLastSliceGame.LevelManager.Levels.Count)
                        {
                            //Game over sucka
                            TheLastSliceGame.Instance.GameOver(GameOverReason.Win);
                        }
                        else
                        {
                            TheLastSliceGame.Instance.ChangeState(GameState.Game);
                        }
                    }
                    break;
            }

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            TheLastSliceGame.Instance.IsMouseVisible = false;
            switch (CurrentState)
            {
                case ScreenState.HOUSE:
                    DrawBackground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawForeground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    break;
                case ScreenState.CAR:
                    DrawBackground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawCar(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawForeground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    break;
                case ScreenState.BUBBLE:
                    DrawBackground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawCar(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawBubble(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawForeground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    break;
                case ScreenState.WAIT:
                case ScreenState.ADD:
                    DrawBackground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawCar(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawBubble(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    DrawForeground(spriteBatch, TheLastSliceGame.LevelManager.CurrentLevelNum - 1);
                    spriteBatch.DrawString(Text, LevelCompleteText, new Vector2(LevelTextPosition.X -2, LevelTextPosition.Y + 2), Color.Black);
                    spriteBatch.DrawString(Text, "Score: " + TempScore, new Vector2(LevelTextPosition.X -2 , LevelTextPosition.Y + 2 + 25), Color.Black);
                    spriteBatch.DrawString(Text, LevelCompleteText, LevelTextPosition, Color.White);
                    spriteBatch.DrawString(Text, "Score: " + TempScore, new Vector2(LevelTextPosition.X, LevelTextPosition.Y + 25), Color.White);
                    break;
            }
        }

        public void DrawBackground(SpriteBatch spriteBatch, int level)
        {
            if (level == 1)
            {
                spriteBatch.Draw(Level1Scene, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level1Person, Vector2.Zero, Color.White);    
            }
            else if (level == 2)
            {
                spriteBatch.Draw(Level2Scene, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level2Person, Vector2.Zero, Color.White);
            }
            else
            {
                spriteBatch.Draw(Level3Scene, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level3Light, Vector2.Zero, Color.White);
            }
        }

        public void DrawCar(SpriteBatch spriteBatch, int level)
        {
            if (level == 1)
            {
                spriteBatch.Draw(Level1Car, CarPosition, Color.White);
            }
            else if (level == 2)
            {
                spriteBatch.Draw(Level2Car, CarPosition, Color.White);
            }
            else
            {
                spriteBatch.Draw(Level3Car, CarPosition, Color.White);
            }
        }

        public void DrawBubble(SpriteBatch spriteBatch, int level)
        {
            if (level == 1)
            {
                spriteBatch.Draw(Level1Bubble, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level1Pizza, Vector2.Zero, Color.White);
                DrawBubbleText(Level1Messages[Level1MessageNumber], spriteBatch);
            }
            else if (level == 2)
            {
                spriteBatch.Draw(Level2Bubble, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level2Pizza, Vector2.Zero, Color.White);
                DrawBubbleText(Level2Messages[Level2MessageNumber], spriteBatch);
            }
            else
            {
                spriteBatch.Draw(Level3Bubble, Vector2.Zero, Color.White);
                DrawBubbleText(Level3Messages[Level3MessageNumber], spriteBatch);
            }
        }

        public void DrawForeground(SpriteBatch spriteBatch, int level)
        {
            if (level == 1)
            {
                spriteBatch.Draw(Level1Hill, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level1Trash, Vector2.Zero, Color.White);
            }
            else if (level == 2)
            {
                spriteBatch.Draw(Level2Hill, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level2Trash, Vector2.Zero, Color.White);
            }
            else
            {
                spriteBatch.Draw(Level3Hill, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level3Person, Vector2.Zero, Color.White);
                spriteBatch.Draw(Level3Trash, Vector2.Zero, Color.White);
            }
        }

        private void DrawBubbleText(String[] lines, SpriteBatch spriteBatch)
        {
            if (lines.Length == 1)
            {
                BubbleLine1Y = BubbleYCenter;

                string line1 = lines[0];

                Vector2 line1Size = Message.MeasureString(line1);

                spriteBatch.DrawString(Message, line1, new Vector2(BubbleXCenter - (line1Size.X / 2) - 2, BubbleLine1Y + 2), Color.Black);
                spriteBatch.DrawString(Message, line1, new Vector2(BubbleXCenter - (line1Size.X / 2), BubbleLine1Y), Color.White);
            }
            else if (lines.Length == 2)
            {
                BubbleLine1Y = BubbleYCenter - BubbleLineOffset;
                BubbleLine2Y = BubbleYCenter + BubbleLineOffset;

                string line1 = lines[0];
                string line2 = lines[1];

                Vector2 line1Size = Message.MeasureString(line1);
                Vector2 line2Size = Message.MeasureString(line2);

                spriteBatch.DrawString(Message, line1, new Vector2(BubbleXCenter - (line1Size.X / 2) - 2, BubbleLine1Y + 2), Color.Black);
                spriteBatch.DrawString(Message, line2, new Vector2(BubbleXCenter - (line2Size.X / 2) - 2, BubbleLine2Y + 2), Color.Black);
                spriteBatch.DrawString(Message, line1, new Vector2(BubbleXCenter - (line1Size.X / 2), BubbleLine1Y), Color.White);
                spriteBatch.DrawString(Message, line2, new Vector2(BubbleXCenter - (line2Size.X / 2), BubbleLine2Y), Color.White);
            }
            else if (lines.Length == 3)
            {
                BubbleLine1Y = BubbleYCenter - BubbleLineOffset * 2;
                BubbleLine2Y = BubbleYCenter;
                BubbleLine3Y = BubbleYCenter + BubbleLineOffset * 2;

                string line1 = lines[0];
                string line2 = lines[1];
                string line3 = lines[2];

                Vector2 line1Size = Message.MeasureString(line1);
                Vector2 line2Size = Message.MeasureString(line2);
                Vector2 line3Size = Message.MeasureString(line3);

                spriteBatch.DrawString(Message, line1, new Vector2(BubbleXCenter - (line1Size.X / 2) - 2, BubbleLine1Y + 2), Color.Black);
                spriteBatch.DrawString(Message, line2, new Vector2(BubbleXCenter - (line2Size.X / 2) - 2, BubbleLine2Y + 2), Color.Black);
                spriteBatch.DrawString(Message, line3, new Vector2(BubbleXCenter - (line3Size.X / 2) - 2, BubbleLine3Y + 2), Color.Black);
                spriteBatch.DrawString(Message, line1, new Vector2(BubbleXCenter - (line1Size.X / 2), BubbleLine1Y), Color.White);
                spriteBatch.DrawString(Message, line2, new Vector2(BubbleXCenter - (line2Size.X / 2), BubbleLine2Y), Color.White);
                spriteBatch.DrawString(Message, line3, new Vector2(BubbleXCenter - (line3Size.X / 2), BubbleLine3Y), Color.White);
            }
        }

        public void UpdateCar(GameTime gameTime)
        {
            CarPosition = new Vector2(Lerper.Lerp(CarPosition.X, 0), 0);
        }

        public void ResetState()
        {
            CurrentState = ScreenState.HOUSE;
            TempScore = 0;
            ResetCarPosition();
            NewMessages();
        }

        public void SetTransitionTimeIfZero(GameTime gameTime, float newTransitionTime)
        {
            if (TransitionTime == TimeSpan.Zero)
            {
                TransitionTime = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + newTransitionTime);
            }
        }

        public void ResetCarPosition()
        {
            CarPosition = CarInitialPosition;
        }

        public void NewMessages()
        {
            Level1MessageNumber = TheLastSliceGame.Random.Next(0, Level1Messages.Length);
            Level2MessageNumber = TheLastSliceGame.Random.Next(0, Level2Messages.Length);
            Level3MessageNumber = TheLastSliceGame.Random.Next(0, Level3Messages.Length);
        }
    }
}
