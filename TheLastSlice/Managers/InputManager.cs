using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TheLastSlice.Managers
{
    public class InputManager
    {
        public KeyboardState PreviousKeyboardState { get; private set; }
        public GamePadState PreviousGamePadState { get; private set; }

        public void Update()
        {
            PreviousKeyboardState = Keyboard.GetState();
            PreviousGamePadState = GamePad.GetState(PlayerIndex.One);
        }

        public bool IsInputPressed(Keys key)
        {
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);

            //Enter/Accept/Debug has the same logic for UI and in game.
            if (key == Keys.Enter && ((keyboardState.IsKeyDown(Keys.Space) && !PreviousKeyboardState.IsKeyDown(Keys.Space)) ||
                (keyboardState.IsKeyDown(Keys.Enter) && !PreviousKeyboardState.IsKeyDown(Keys.Enter)) ||
                (gamePadState.IsButtonDown(Buttons.A) && !PreviousGamePadState.IsButtonDown(Buttons.A)) ||
                (gamePadState.IsButtonDown(Buttons.Start) && !PreviousGamePadState.IsButtonDown(Buttons.Start))))
            {
                return true;
            }
            else if (key == Keys.Home && ((keyboardState.IsKeyDown(Keys.Home) && !PreviousKeyboardState.IsKeyDown(Keys.Home) ||
                    (gamePadState.IsButtonDown(Buttons.Back) && !PreviousGamePadState.IsButtonDown(Buttons.Back)))))
            {
                return true;
            }
            else if (TheLastSliceGame.Instance.GameState == GameState.Menu)
            {
                //For menus we need to check the previous state of the input.
                if (key == Keys.Up && ((keyboardState.IsKeyDown(Keys.Up) && !PreviousKeyboardState.IsKeyDown(Keys.Up)) || 
                    (keyboardState.IsKeyDown(Keys.W) && !PreviousKeyboardState.IsKeyDown(Keys.W)) ||
                    (gamePadState.IsButtonDown(Buttons.DPadUp) && !PreviousGamePadState.IsButtonDown(Buttons.DPadUp)) ||
                    (gamePadState.ThumbSticks.Left.Y > 0.2 && PreviousGamePadState.ThumbSticks.Left.Y < 0.2)))
                {
                    return true;
                }
                else if (key == Keys.Down && ((keyboardState.IsKeyDown(Keys.Down) && !PreviousKeyboardState.IsKeyDown(Keys.Down)) ||
                    (keyboardState.IsKeyDown(Keys.S) && !PreviousKeyboardState.IsKeyDown(Keys.S)) ||
                    (gamePadState.IsButtonDown(Buttons.DPadDown) && !PreviousGamePadState.IsButtonDown(Buttons.DPadDown)) ||
                    (gamePadState.ThumbSticks.Left.Y < -0.2 && PreviousGamePadState.ThumbSticks.Left.Y > -0.2)))
                {
                    return true;
                }
                else if (keyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key))
                {
                    return true;
                }
            }
            else
            {
                if (key == Keys.Up && (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W) ||
                    gamePadState.IsButtonDown(Buttons.DPadUp) || gamePadState.ThumbSticks.Left.Y > 0.2))
                {
                    return true;
                }
                else if (key == Keys.Down && (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S) ||
                        gamePadState.IsButtonDown(Buttons.DPadDown) || gamePadState.ThumbSticks.Left.Y < -0.2))
                {
                    return true;
                }
                else if (key == Keys.Left && (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A) ||
                        gamePadState.IsButtonDown(Buttons.DPadLeft) || gamePadState.ThumbSticks.Left.X < -0.2))
                {
                    return true;
                }
                else if (key == Keys.Right && (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D) ||
                        gamePadState.IsButtonDown(Buttons.DPadRight) || gamePadState.ThumbSticks.Left.X > 0.2))
                {
                    return true;
                }
                else if (keyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
