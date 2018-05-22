using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TheLastSlice.Models;

namespace TheLastSlice.UI
{
    public class GameWinMenu : UIScreen
    {
        public int LastPlayerScore { get; set; }

        private Texture2D Background { get; set; }
        private Texture2D MenuTexture { get; set; }
        private List<String> CustomWinStrings;
        private bool HasInputScore { get; set; }
        private String Initials { get; set; }
        private bool ShowCursor { get; set; }
        private TimeSpan CursorTimerMS { get; set; }
        public bool Posted { get; private set; }
        private bool HasLoggedOut { get; set; } = false;

        Keys[] ValidKeys = new Keys[] { Keys.A, Keys.B, Keys.C, Keys.D, Keys.E, Keys.F, Keys.G, Keys.H, Keys.I, Keys.J,
                                        Keys.K, Keys.L, Keys.M, Keys.N, Keys.O, Keys.P, Keys.Q, Keys.R, Keys.S, Keys.T,
                                        Keys.U, Keys.V, Keys.W, Keys.X, Keys.Y, Keys.Z };


        private string[] YouWinString = new string[] { "LOOKS LIKE YOU'VE#DELIVERED THE GOODS." };
        private string[] EnterNameString = new string[] { "ENTER YOUR INITIALS THEN#LOGIN TO VALIDATE" };
        private string[] LoginString = new string[] { "PLEASE LOGIN" };
        private string TryAgain = "Try Again.";

        private UIEntity ArrowLeft { get; set; }
        private UIEntity ArrowRight { get; set; }
        private UIEntity Menu { get; set; }

        private Animation ArrowLeftAnim { get; set; }
        private Animation ArrowRightAnim { get; set; }

        private int MenuX { get; set; }
        private int MenuY { get; set; }
        private int MenuXCenter { get; set; }
        private int MenuWidth { get; set; }

        private int ArrowPadding;
        private int ArrowLeftX { get; set; }
        private int ArrowRightX { get; set; }
        private int ArrowY { get; set; }

        protected SpriteFont YouWin { get; set; }
        protected SpriteFont Name { get; set; }

        public GameWinMenu()
        {
            Posted = false;
            Initials = "";
            HasInputScore = false;
            CustomWinStrings = new List<String>();
            CursorTimerMS = TimeSpan.Zero;
            ShowCursor = false;
            ArrowPadding = 50;
        }

        public void SetCustomString(String winString)
        {
            string[] sentences = Regex.Split(winString, @"(?<=[\.!\?])\s+");
            String appendString = "";

            foreach(String sentence in sentences)
            {
                String tempString = sentence;
                if (!String.IsNullOrEmpty(appendString))
                {
                    tempString += appendString;
                    appendString = "";
                }
                CustomWinStrings.Add(tempString);               
            }
        }

        public override void LoadContent()
        {
            base.LoadContent();
            Background = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/bluescreen");
            Text = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_24");
            YouWin = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_24");
            Name = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_40");

            ArrowLeftAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-left"), 6);
            ArrowRightAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-right"), 6);
            MenuTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/submit");

            ArrowLeft = new UIEntity(Vector2.Zero, ArrowLeftAnim);
            ArrowRight = new UIEntity(Vector2.Zero, ArrowRightAnim);
            Menu = new UIEntity(Vector2.Zero, MenuTexture);

            //line everything up
            MenuX = TheLastSliceGame.Instance.GameWidth / 2 - Menu.HalfWidth;
            MenuY = 475;
            MenuWidth = Menu.Width;
            MenuXCenter = TheLastSliceGame.Instance.GameWidth / 2;

            ArrowLeftX = MenuXCenter - MenuWidth / 2 - ArrowPadding;
            ArrowRightX = MenuXCenter + (MenuWidth / 2) + ArrowPadding - ArrowRight.Width;

            ArrowY = (MenuY + Menu.HalfHeight) - ArrowLeft.HalfHeight;

            ArrowLeft.Position = new Vector2(ArrowLeftX, ArrowY);
            ArrowRight.Position = new Vector2(ArrowRightX, ArrowY);
            Menu.Position = new Vector2(MenuX, MenuY);
        }

        public override void Update(GameTime gameTime)
        {
            //"Chief, let's see if we can find a way to open this door." - Cortana
            if (!HasInputScore)
            {
                if (TheLastSliceGame.Instance.IsUserLoggedIn() && !HasLoggedOut)
                {
                    TheLastSliceGame.Instance.GameService.Logout();
                    HasLoggedOut = true;
                }

                if (gameTime.TotalGameTime.TotalMilliseconds > CursorTimerMS.TotalMilliseconds)
                {
                    ShowCursor = !ShowCursor;
                    CursorTimerMS = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 500);
                }

                if (!String.IsNullOrEmpty(Initials) && Initials.Count() <= 3 && TheLastSliceGame.InputManager.IsInputPressed(Keys.Back))
                {
                    Initials = Initials.Remove(Initials.Length - 1);
                }

                if(Initials.Count() < 3)
                {
                    Vector2 vector = Name.MeasureString(Initials);
                    foreach (Keys key in ValidKeys)
                    {
                        if(TheLastSliceGame.InputManager.IsInputPressed(key))
                        {
                            Initials += key.ToString();
                            break;
                        }
                    }
                }
                else
                {
                    if (CustomWinStrings.Count() == 0 && TheLastSliceGame.InputManager.IsInputPressed(Keys.Enter))
                    {
                        TheLastSliceGame.Instance.GameWinAsync();
                        HasInputScore = true;
                    }
                }
            }
            else
            {
                if(TheLastSliceGame.Instance.IsUserLoggedIn() && !Posted)
                {
                    TheLastSliceGame.Instance.PostScoreAsync(LastPlayerScore.ToString(), Initials);
                    Posted = true;
                    TheLastSliceGame.Instance.AppInsights.PostScoreSuccess();
                }
            }

            ArrowLeft.Update(gameTime);
            ArrowRight.Update(gameTime);
            Menu.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            TheLastSliceGame.Instance.IsMouseVisible = false;
            if (CustomWinStrings.Count > 0)
            {
                if (TheLastSliceGame.Instance.GameService.IsSuccessStatusCode)
                {
                    DrawStrings(spriteBatch, CustomWinStrings.ToArray(), YouWin, 150, 40);
                }
                else
                {
                    YouWinString[0] = CustomWinStrings[0].Replace("\"", "") + "#" + TryAgain;
                    Posted = false;
                    Initials = "";
                    HasInputScore = false;
                    CustomWinStrings = new List<String>();
                }
                
            }
            else
            {
                DrawStrings(spriteBatch, YouWinString, YouWin, 50, 40);

                // spriteBatch.DrawString(Text, "Score: " + LastPlayerScore, new Vector2(TheLastSliceGame.GameWidth / 2 - 60, 140), ColorYellow);

                DrawStrings(spriteBatch, EnterNameString, YouWin, 200, 40);
                if (!HasInputScore && Initials.Count() >= 3)
                {
                    ArrowLeft.Draw(spriteBatch);
                    ArrowRight.Draw(spriteBatch);
                    Menu.Draw(spriteBatch);
                }
                if (HasInputScore && !TheLastSliceGame.Instance.IsUserLoggedIn())
                {
                    DrawStrings(spriteBatch, LoginString, YouWin, 500, 40);
                }

                int xPos = TheLastSliceGame.Instance.GameWidth / 2 - 110;
                for (int i = 0; i < Initials.Count(); i++)
                {
                    spriteBatch.DrawString(Name, Initials.ElementAt(i).ToString(), new Vector2(xPos, 340), Color.White);
                    if (i < 2)
                    {
                        xPos += 88;
                    }
                }

                if (ShowCursor)
                {
                    spriteBatch.DrawString(Name, "_", new Vector2(xPos, 350), Color.White);
                }
            }
        }

        public void DrawStrings(SpriteBatch spriteBatch, String[] strings,SpriteFont font, int yPosition, int yLineSpacing)
        {
            int xPos;
            int yPos = yPosition;

            foreach (String line in strings)
            {
                foreach(String subline in line.Split(new string[] { "#" }, StringSplitOptions.None))
                {
                    Vector2 vector = Text.MeasureString(subline);
                    xPos = TheLastSliceGame.Instance.GameWidth / 2 - ((int)vector.X / 2);
                    spriteBatch.DrawString(Text, subline, new Vector2(xPos, yPos), Color.White);
                    yPos += yLineSpacing;
                }
                yPos += yLineSpacing;
            }
        }
    }
}
