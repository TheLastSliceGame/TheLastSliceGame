using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TheLastSlice.Models;

namespace TheLastSlice.Managers
{
    public class AnimationManager
    {
        public Animation Animation { get; private set; }
        public Animation PreviousAnimation { get; private set; }
        public Vector2 Position { get; set; }

        private bool IsPlaying { get; set; }
        private float Timer;

        public AnimationManager(Animation animation)
        {
            Animation = animation;
            IsPlaying = true;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (Animation != null)
            {
                if (!Animation.IsLooping && Animation.HasPlayedOnce)
                {
                    return;
                }

                spriteBatch.Draw(Animation.Texture,
                                 Position,
                                 new Rectangle(Animation.CurrentFrame * Animation.FrameWidth,
                                               0,
                                               Animation.FrameWidth,
                                               Animation.FrameHeight),
                                 Animation.Color);
            }
        }

        public void Play(Animation animation)
        {
            PreviousAnimation = Animation;
            Animation.HasPlayedOnce = false;
            IsPlaying = true;
            if (Animation == animation)
                return;

            Animation = animation;
            Animation.CurrentFrame = 0;
            Timer = 0f;
        }

        public void PlayPreviousAnimation()
        {
            if(PreviousAnimation != null)
            {
                Play(PreviousAnimation);
            }
        }

        public void Stop()
        {
            Timer = 0f;
            IsPlaying = false;
        }

        public void Update(GameTime gameTime)
        {
            if(Animation != null && IsPlaying)
            {
                if (!Animation.IsLooping && Animation.HasPlayedOnce)
                {
                    return;
                }

                Timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (Timer > Animation.FrameSpeed)
                {
                    Timer = 0f;
                    Animation.CurrentFrame++;
                    if (Animation.CurrentFrame >= Animation.FrameCount)
                    {
                        if (!Animation.IsLooping)
                        {
                            Animation.HasPlayedOnce = true;
                        }
                        Animation.CurrentFrame = 0;
                    }
                }
            }
        }
    }
}
