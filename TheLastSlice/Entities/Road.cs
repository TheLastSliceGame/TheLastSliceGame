using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace TheLastSlice.Entities
{
    public class Road : Entity
    {
        //Where we're going.... we don't need roads...
        //But seriously do we need a Road class?
        public Road(Vector2 position, String assetCode) : base(position)
        {
            Type = EntityType.Road;
            IsBlocking = false;
            AssetCode = assetCode;
        }

        public override void LoadTexture()
        {
            int currentLevel = TheLastSliceGame.LevelManager.CurrentLevelNum;
            String assetPath = "Entity/Level" + currentLevel.ToString() + "/Tiles/" + AssetCode;
            Texture2D roadTexture = TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath);

            if (roadTexture != null)
            {
                Texture = roadTexture;
                Height = roadTexture.Height;
                Width = roadTexture.Width;
            }
            else
            {
                Debug.WriteLine(" *** ERROR - No asset found for asset code {0}", AssetCode);
            }
        }
    }
}
