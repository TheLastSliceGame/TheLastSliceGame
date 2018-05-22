using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace TheLastSlice.UI
{
    public enum UIType { None, MainMenu, GameHUD, GameOver, Login, LevelTransition, Leaderboard };

    public class UIScreen
    {
        protected SpriteFont Text { get; set; }
        protected MouseState PreviousMouseState { get; set; }

        protected Color ColorYellow { get; set; }
        protected Color ColorRed { get; set; }

        public UIScreen()
        {
            ColorYellow = new Color(232, 196, 32);
            ColorRed = new Color(237, 38, 38);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
        }

        public virtual void Update(GameTime time)
        {
        }

        public virtual void LoadContent()
        {
            Text = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_12");
            PreviousMouseState = Mouse.GetState();
        }
    }
}
