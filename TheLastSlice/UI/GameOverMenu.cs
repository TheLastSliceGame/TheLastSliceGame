using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TheLastSlice.Entities;
using TheLastSlice.Models;

namespace TheLastSlice.UI
{
    public class GameOverMenu : UIScreen
    {
        public GameOverReason GameOverReason { get; set; }
        public int Score { get; set; }

        private Texture2D Background { get; set; }
        private Texture2D MenuTexture { get; set; }
        private Texture2D Bubble { get; set; }
        private Ingredient Frog { get; set; }

        private SpriteFont GameOver { get; set; }
        private int MenuIndex { get; set; }

        private Animation ArrowLeftAnim { get; set; }
        private Animation ArrowRightAnim { get; set; }

        private UIEntity ArrowLeft { get; set; }
        private UIEntity ArrowRight { get; set; }
        private UIEntity Menu { get; set; }

        private Vector2 GameOverTextSize { get; set; }
        private Vector2 NoTipForYouTextSize { get; set; }
        private Vector2 BubbleLine1TextSize { get; set; }
        private Vector2 BubbleLine2TextSize { get; set; }

        private String GameOverText { get; set; }
        private String NoTipForYouText { get; set; }
        private String BubbleLine1Text { get; set; }
        private String BubbleLine2Text { get; set; }

        private int MenuX;
        private int MenuY;
        private int MenuXCenter;
        private int Menu0Width;
        private int Menu1Width;

        private int ArrowPadding;
        private int ArrrowYOffset;
        private int ArrowLeftX0;
        private int ArrowLeftX1;
        private int ArrowRightX0;
        private int ArrowRightX1;
        private int ArrowY0;
        private int ArrowY1;

        private int BubbleX;
        private int BubbleY;
        private int BubbleYCenterOffset;
        private int BubbleXCenter;
        private int BubbleYCenter;
        private int BubbleLine1Y;
        private int BubbleLine2Y;
        private int BubbleLine3Y;

        private int BubbleLineOffset;
        private int BubbleOffsetX;
        private int BubbleOffsetY;
        private int GameOverX;
        private int GameOverY;
        private int NoTipForYouX;
        private int NoTipForYouY;

        private String[][] WrongDeliveryMessages = new string[][]
        {
            new string[] {"WHY DON'T YOU MAKE", "LIKE A PIZZA", "AND GET OUTTA HERE."},
            new string[] {"I'LL GO, I'LL GO,", "I'LL GO, I'LL GO."},
            new string[] {"STRANGE THINGS ARE", "AFOOT AT", "THE LAST SLICE."},
            new string[] {"A STRANGE GAME.", "THE ONLY WINNING MOVE", "IS NOT TO PLAY." },
        };

        private String[][] DeathMessages = new string[][]
        {
            new string[] {"LOOKS LIKE YOUR", "PICKUP", "NEEDS A PICK-UP..."},
            new string[] {"WHERE WE'RE GOING", "WE DON'T NEED ROADS"},
            new string[] {"ACHIEVEMENT UNLOCKED:", "BARREL ROLL"},
            new string[] {"SMASH MY TRUCK INTO", "PIECES, THIS IS", "MY LAST ACCORD"},
            new string[] {"IT'S THE END OF THE", "WORLD AS WE KNOW IT."},
            new string[] {"WE DIDN'T START", "THE FIRE..."},
            new string[] {"WHAT DO WE DO?", "WE DIE..."},
        };

        private String[][] OutOfGasMessages = new string[][]
        {
            new string[] {"YOU RAN OUT OF GAS.", "SRSLY, WHO DOES THAT?"},
            new string[] {"FILL 'ER UP DUDE!"},
        };

        private String[][] PaperMessages = new string[][]
        {
            new string[] {"I LIKE TO RIDE", "MY BICYCLE, I LIKE", "TO RIDE MY BIKE."},
        };

        private int WrongDeliveryMessageNumber;
        private int DeathMessageNumber;
        private int OutOfGasMessageNumber;
        private int PaperMessageNumber;

        public GameOverMenu()
        {
            
        }

        public override void LoadContent()
        {
            //Finish HIM!
            base.LoadContent();
            Background = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/GameOverScreen");
            GameOver = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_40");
            MenuTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/game-over");
            Bubble = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/Bubble");
            ArrowLeftAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-left"), 6);
            ArrowRightAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-right"), 6);

            GameOverText = "Game Over";
            NoTipForYouText = "No Tip For You.";

            ArrowPadding = 50;
            ArrrowYOffset = 45;
            BubbleYCenterOffset = 20;
            BubbleLineOffset = 10;
            BubbleOffsetX = 90;
            BubbleOffsetY = 30;
            GameOverY = 230;
            NoTipForYouY = 290;

            ArrowLeft = new UIEntity(Vector2.Zero, ArrowLeftAnim);
            ArrowRight = new UIEntity(Vector2.Zero, ArrowRightAnim);
            Menu = new UIEntity(Vector2.Zero, MenuTexture);

            //line everything up
            MenuX = TheLastSliceGame.Instance.GameWidth / 2 - MenuTexture.Width / 2;
            MenuY = 475;
            Menu0Width = 300;
            Menu1Width = 134;
            MenuXCenter = TheLastSliceGame.Instance.GameWidth / 2;

            GameOverTextSize = GameOver.MeasureString(GameOverText);
            NoTipForYouTextSize = Text.MeasureString(NoTipForYouText);

            GameOverX = (int) (MenuXCenter - GameOverTextSize.X / 2);
            NoTipForYouX = (int)(MenuXCenter - NoTipForYouTextSize.X / 2);

            BubbleX = GameOverX - BubbleOffsetX;
            BubbleY = GameOverY - Bubble.Height + BubbleOffsetY;
            BubbleXCenter = BubbleX + (Bubble.Width / 2);
            BubbleYCenter = BubbleY - BubbleYCenterOffset + (Bubble.Height / 2);

            Frog = new Ingredient(new Vector2(700, 375), "UIFRL");
            Frog.LoadTexture();

            ArrowLeftX0 = MenuXCenter - Menu0Width / 2 - ArrowPadding;
            ArrowLeftX1 = MenuXCenter - Menu1Width / 2 - ArrowPadding;
            ArrowRightX0 = MenuXCenter + (Menu0Width / 2) + ArrowPadding - ArrowRight.Width;
            ArrowRightX1 = MenuXCenter + (Menu1Width / 2) + ArrowPadding - ArrowRight.Width;

            ArrowY0 = MenuY + 16 - ArrowLeft.HalfHeight;
            ArrowY1 = ArrowY0 + ArrrowYOffset;

            MenuIndex = 0;
            ArrowLeft.Position = new Vector2(ArrowLeftX0, ArrowY0);
            ArrowRight.Position = new Vector2(ArrowRightX0, ArrowY0);
            Menu.Position = new Vector2(MenuX, MenuY);

            Score = 0;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //All your base are belong to us - H.E.%
            TheLastSliceGame.Instance.IsMouseVisible = true;

            spriteBatch.Draw(Background, new Vector2(0, 0), Color.White);
            spriteBatch.DrawString(GameOver, GameOverText, new Vector2(GameOverX, GameOverY), Color.White);
            spriteBatch.DrawString(Text, NoTipForYouText, new Vector2(NoTipForYouX, NoTipForYouY), Color.White);
            spriteBatch.Draw(Bubble, new Vector2(BubbleX, BubbleY), Color.White);

            if (GameOverReason == GameOverReason.WrongDelivery || GameOverReason == GameOverReason.Win || GameOverReason == GameOverReason.WrongDeliveryFrog)
            {
                DrawBubbleText(WrongDeliveryMessages[WrongDeliveryMessageNumber], spriteBatch);
            }
            else if (GameOverReason == GameOverReason.DeathByPaperBoy)
            {
                DrawBubbleText(PaperMessages[PaperMessageNumber], spriteBatch);
            }
            else if (GameOverReason == GameOverReason.Death)
            {
                DrawBubbleText(DeathMessages[DeathMessageNumber], spriteBatch);
            }
            else if (GameOverReason == GameOverReason.Gas)
            {
                DrawBubbleText(OutOfGasMessages[OutOfGasMessageNumber], spriteBatch);
            }

            ArrowLeft.Draw(spriteBatch);
            ArrowRight.Draw(spriteBatch);
            
            if(Frog.AnimationManager.Animation.CurrentFrame == 3)
            {
                Vector2 newFrogPos = new Vector2(Frog.Position.X - 5, Frog.Position.Y);
                Frog.Position = newFrogPos;
                Frog.AnimationManager.Position = newFrogPos;
            }

            int scoreXPos = MenuXCenter - (5 * Score.ToString().Length - 1);
            spriteBatch.DrawString(Text, "Score: " + Score.ToString(), new Vector2(scoreXPos - 40, 325), Color.White);

            if (Frog.Position.X < 0)
            {
                Frog.Position = new Vector2(TheLastSliceGame.Instance.GameWidth, 375);
            }

            Frog.Draw(spriteBatch);
            Menu.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (MenuIndex == 1 && TheLastSliceGame.InputManager.IsInputPressed(Keys.Up))
            {
                MenuIndex = 0;
                ArrowLeft.Position = new Vector2(ArrowLeftX0, ArrowY0);
                ArrowRight.Position = new Vector2(ArrowRightX0, ArrowY0);
            }
            else if (MenuIndex == 0 && TheLastSliceGame.InputManager.IsInputPressed(Keys.Down))
            {
                MenuIndex = 1;
                ArrowLeft.Position = new Vector2(ArrowLeftX1, ArrowY1);
                ArrowRight.Position = new Vector2(ArrowRightX1, ArrowY1);
            }
            else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Enter))
            {
                if(MenuIndex == 0)
                {
                    TheLastSliceGame.Instance.ChangeState(GameState.NewGame);
                }
                else if(MenuIndex == 1)
                {
                    TheLastSliceGame.Instance.ChangeState(GameState.Menu, UIType.MainMenu);
                }
            }

            Frog.Update(gameTime);
            ArrowLeft.Update(gameTime);
            ArrowRight.Update(gameTime);
            Menu.Update(gameTime);
        }

        private void DrawBubbleText(String[] lines, SpriteBatch spriteBatch)
        {
            if(lines.Length == 1)
            {
                BubbleLine1Y = BubbleYCenter;
                
                string line1 = lines[0];

                Vector2 line1Size = Text.MeasureString(line1);

                spriteBatch.DrawString(Text, line1, new Vector2(BubbleXCenter - (line1Size.X / 2) - 2, BubbleLine1Y + 2), Color.Black);
                spriteBatch.DrawString(Text, line1, new Vector2(BubbleXCenter - (line1Size.X / 2), BubbleLine1Y), Color.White);
            }
            else if (lines.Length == 2)
            {
                BubbleLine1Y = BubbleYCenter - BubbleLineOffset;
                BubbleLine2Y = BubbleYCenter + BubbleLineOffset;

                string line1 = lines[0];
                string line2 = lines[1];

                Vector2 line1Size = Text.MeasureString(line1);
                Vector2 line2Size = Text.MeasureString(line2);

                spriteBatch.DrawString(Text, line1, new Vector2(BubbleXCenter - (line1Size.X / 2) - 2, BubbleLine1Y + 2), Color.Black);
                spriteBatch.DrawString(Text, line2, new Vector2(BubbleXCenter - (line2Size.X / 2) - 2, BubbleLine2Y + 2), Color.Black);
                spriteBatch.DrawString(Text, line1, new Vector2(BubbleXCenter - (line1Size.X / 2), BubbleLine1Y), Color.White);
                spriteBatch.DrawString(Text, line2, new Vector2(BubbleXCenter - (line2Size.X / 2), BubbleLine2Y), Color.White);
            }
            else if (lines.Length == 3)
            {
                BubbleLine1Y = BubbleYCenter - BubbleLineOffset*2;
                BubbleLine2Y = BubbleYCenter;
                BubbleLine3Y = BubbleYCenter + BubbleLineOffset*2;

                string line1 = lines[0];
                string line2 = lines[1];
                string line3 = lines[2];

                Vector2 line1Size = Text.MeasureString(line1);
                Vector2 line2Size = Text.MeasureString(line2);
                Vector2 line3Size = Text.MeasureString(line3);

                spriteBatch.DrawString(Text, line1, new Vector2(BubbleXCenter - (line1Size.X / 2) - 2, BubbleLine1Y + 2), Color.Black);
                spriteBatch.DrawString(Text, line2, new Vector2(BubbleXCenter - (line2Size.X / 2) - 2, BubbleLine2Y + 2), Color.Black);
                spriteBatch.DrawString(Text, line3, new Vector2(BubbleXCenter - (line3Size.X / 2) - 2, BubbleLine3Y + 2), Color.Black);
                spriteBatch.DrawString(Text, line1, new Vector2(BubbleXCenter - (line1Size.X / 2), BubbleLine1Y), Color.White);
                spriteBatch.DrawString(Text, line2, new Vector2(BubbleXCenter - (line2Size.X / 2), BubbleLine2Y), Color.White);
                spriteBatch.DrawString(Text, line3, new Vector2(BubbleXCenter - (line3Size.X / 2), BubbleLine3Y), Color.White);
            }
        }

        public void Awake()
        {
            WrongDeliveryMessageNumber = TheLastSliceGame.Random.Next(0, WrongDeliveryMessages.Length );
            DeathMessageNumber = TheLastSliceGame.Random.Next(0, DeathMessages.Length);
            OutOfGasMessageNumber = TheLastSliceGame.Random.Next(0, OutOfGasMessages.Length);
            PaperMessageNumber = TheLastSliceGame.Random.Next(0, PaperMessages.Length);
        }
    }

}
