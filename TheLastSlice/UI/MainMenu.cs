using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using TheLastSlice.Models;
using Windows.UI.Xaml;

namespace TheLastSlice.UI
{
    public class MainMenu : UIScreen
    {
        private Texture2D Background { get; set; }
        private Texture2D MenuTexture { get; set; }

        private Vector2 StartTextSize { get; set; }
        private Vector2 LeaderboardTextSize { get; set; }
        private int MenuIndex { get; set; }

        private Animation ArrowLeftAnim { get; set; }
        private Animation ArrowRightAnim { get; set; }

        private UIEntity ArrowLeft { get; set; }
        private UIEntity ArrowRight { get; set; }
        private UIEntity Menu { get; set; }

        private int MenuX { get; set; }
        private int MenuY { get; set; }
        private int MenuXCenter { get; set; }
        private int Menu0Width { get; set; }
        private int Menu1Width { get; set; }
        private int Menu2Width { get; set; }

        private int ArrowPadding { get; set; }
        private int ArrrowYOffset { get; set; }
        private int ArrowLeftX0 { get; set; }
        private int ArrowLeftX1 { get; set; }
        private int ArrowLeftX2 { get; set; }
        private int ArrowRightX0 { get; set; }
        private int ArrowRightX1 { get; set; }
        private int ArrowRightX2 { get; set; }
        private int ArrowY0 { get; set; }
        private int ArrowY1 { get; set; }
        private int ArrowY2 { get; set; }

        private TimeSpan TransitionTimer { get; set; }
        private SoundEffect StartGame { get; set; }
        private SoundEffect Leaderboard { get; set; }

        public MainMenu()
        {

        }

        public override void LoadContent()
        {
            base.LoadContent();
            Background = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/MainMenuBackground");
            MenuTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/main-menu");
            Text = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_12");

            ArrowLeftAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-left"), 6);
            ArrowRightAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-right"), 6);
            ArrowLeft = new UIEntity(Vector2.Zero, ArrowLeftAnim);
            ArrowRight = new UIEntity(Vector2.Zero, ArrowRightAnim);
            Menu = new UIEntity(Vector2.Zero, MenuTexture);

            ArrowPadding = 50;
            ArrrowYOffset = 45;

            //line everything up
            MenuX = TheLastSliceGame.Instance.GameWidth / 2 - MenuTexture.Width / 2;
            MenuY = 375;

            Menu0Width = 182;
            Menu1Width = 400;
            Menu2Width = 146;
            MenuXCenter = TheLastSliceGame.Instance.GameWidth / 2;

            ArrowLeftX0 = MenuXCenter - Menu0Width / 2 - ArrowPadding;
            ArrowLeftX1 = MenuXCenter - Menu1Width / 2 - ArrowPadding;
            ArrowLeftX2 = MenuXCenter - Menu2Width / 2 - ArrowPadding;
            ArrowRightX0=  MenuXCenter + (Menu0Width / 2) + ArrowPadding - ArrowRight.Width - 10;
            ArrowRightX1=  MenuXCenter + (Menu1Width / 2) + ArrowPadding - ArrowRight.Width - 10;
            ArrowRightX2 = MenuXCenter + (Menu2Width / 2) + ArrowPadding - ArrowRight.Width - 10;

            ArrowY1 = MenuY + (MenuTexture.Height / 2) - ArrowLeft.HalfHeight;

            ArrowY0 = ArrowY1 - ArrrowYOffset;
            ArrowY2 = ArrowY1 + ArrrowYOffset;

            MenuIndex = 0;
            ArrowLeft.Position = new Vector2(ArrowLeftX0, ArrowY0);
            ArrowRight.Position = new Vector2(ArrowRightX0, ArrowY0);
            Menu.Position = new Vector2(MenuX, MenuY);

            StartGame = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/thelastslice");
            TransitionTimer = TimeSpan.Zero;
            Leaderboard = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/leaderboard");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Background, new Vector2(0, 0), Color.White);

            ArrowLeft.Draw(spriteBatch);
            ArrowRight.Draw(spriteBatch);
            Menu.Draw(spriteBatch);
        }

        public override void Update(GameTime time)
        {
            if (TransitionTimer != TimeSpan.Zero)
            {
                if (time.TotalGameTime.TotalMilliseconds > TransitionTimer.TotalMilliseconds)
                {
                    if(MenuIndex == 0)
                    {
                        TheLastSliceGame.Instance.ChangeState(GameState.NewGame);
                    }
                    else if(MenuIndex == 1)
                    {
                        TheLastSliceGame.Instance.ChangeState(GameState.Menu, UIType.Leaderboard);
                    }
                    TransitionTimer = TimeSpan.Zero;
                }
            }
            else
            {
                KeyboardState keyboardState = Keyboard.GetState();
                if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Up))
                {
                    if (MenuIndex == 1)
                    {
                        MenuIndex = 0;
                        ArrowLeft.Position = new Vector2(ArrowLeftX0, ArrowY0);
                        ArrowRight.Position = new Vector2(ArrowRightX0, ArrowY0);
                    }
                    else if(MenuIndex == 2)
                    {
                        MenuIndex = 1;
                        ArrowLeft.Position = new Vector2(ArrowLeftX1, ArrowY1);
                        ArrowRight.Position = new Vector2(ArrowRightX1, ArrowY1);
                    }
                }
                else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Down))
                {
                    if (MenuIndex == 0)
                    {
                        MenuIndex = 1;
                        ArrowLeft.Position = new Vector2(ArrowLeftX1, ArrowY1);
                        ArrowRight.Position = new Vector2(ArrowRightX1, ArrowY1);
                    }
                    else if(MenuIndex == 1)
                    {
                        MenuIndex = 2;
                        ArrowLeft.Position = new Vector2(ArrowLeftX2, ArrowY2);
                        ArrowRight.Position = new Vector2(ArrowRightX2, ArrowY2);
                    }
                }
                else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Enter))
                {
                    if (MenuIndex == 0)
                    {
                        TheLastSliceGame.Instance.PauseMusic();
                        StartGame.Play();
                        TransitionTimer = TimeSpan.FromMilliseconds(time.TotalGameTime.TotalMilliseconds + 3000);
                    }
                    else if (MenuIndex == 1)
                    {
                        TheLastSliceGame.Instance.PauseMusic();
                        Leaderboard.Play();
                        TransitionTimer = TimeSpan.FromMilliseconds(time.TotalGameTime.TotalMilliseconds + 3000);
                    }
                    else if (MenuIndex == 2)
                    {
                        Application.Current.Exit();
                    }
                }
            }

            ArrowLeft.Update(time);
            ArrowRight.Update(time);
            Menu.Update(time);
        }
    }
}
