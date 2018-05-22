using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLastSlice.Models
{
    public class Animation
    {
        public Texture2D Texture { get; private set; }

        public int CurrentFrame { get; set; }

        public int FrameCount { get; set; }

        public int FrameHeight { get { return Texture.Height; } }

        public float FrameSpeed { get; set; }

        public int FrameWidth { get { return Texture.Width / FrameCount; } }

        public bool IsLooping { get; set; }

        public bool HasPlayedOnce { get; set; }

        public Color Color = Color.White;

        public Animation(Texture2D texture, int frameCount, float frameSpeed = 0.1f)
        {
            Texture = texture;
            FrameCount = frameCount;
            IsLooping = true;
            FrameSpeed = frameSpeed;
        }
    }

   
}
