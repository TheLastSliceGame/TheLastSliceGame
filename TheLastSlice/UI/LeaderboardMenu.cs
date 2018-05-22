using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using TheLastSlice.Entities;
using TheLastSlice.Models;

namespace TheLastSlice.UI
{
    public class LeaderboardMenu : UIScreen
    {
        class LeaderboardEntry
        {
            public LeaderboardEntry(String initials, int challengesCompleted)
            {
                Initials = initials;
                ChallengesCompleted = challengesCompleted;

                if (ChallengesCompleted == 1)
                {
                    Challenge1 = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/pizza1");
                }
                else if(ChallengesCompleted == 2)
                {
                    Challenge1 = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/pizza1");
                    Challenge2 = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/pizza2");
                }
                else if (ChallengesCompleted == 3)
                {
                    Challenge1 = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/pizza1");
                    Challenge2 = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/pizza2");
                    Challenge3 = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/pizza3");
                }
            }

            public String Initials { get; private set; }
            public int ChallengesCompleted { get; private set; }
            public Texture2D Challenge1 { get; private set; }
            public Texture2D Challenge2 { get; private set; }
            public Texture2D Challenge3 { get; private set; }
        }

        private Entity SnakeInTheBox { get; set; }
        private Texture2D Leaderboard { get; set; }
        private Texture2D Exit { get; set; }
        private Texture2D Background { get; set; }
        private List<LeaderboardEntry> LeaderboardList { get; set; }

        private Animation ArrowLeftAnim { get; set; }
        private Animation ArrowRightAnim { get; set; }

        private UIEntity ArrowLeft { get; set; }
        private UIEntity ArrowRight { get; set; }
        private int MenuIndex { get; set; }

        public LeaderboardMenu()
        {
            LeaderboardList = new List<LeaderboardEntry>();
        }

        public override void LoadContent()
        {
            base.LoadContent();
            Leaderboard = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/leaderboard");
            Exit = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/exit");
            Background = TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/LevelTransitionScreen");
            Text = TheLastSliceGame.Instance.Content.Load<SpriteFont>("Fonts/Joystix_19");

            ArrowLeftAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-left"), 6);
            ArrowRightAnim = new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/arrow-right"), 6);
            ArrowLeft = new UIEntity(Vector2.Zero, ArrowLeftAnim);
            ArrowRight = new UIEntity(Vector2.Zero, ArrowRightAnim);
            MenuIndex = 0;

            ArrowLeft.Position = new Vector2(TheLastSliceGame.Instance.GameWidth /2 - Exit.Width/2 - ArrowLeft.Width, 525);
            ArrowRight.Position = new Vector2(TheLastSliceGame.Instance.GameWidth /2 + Exit.Width/2 - 10, 525);

            SnakeInTheBox = new Entity(new Vector2(10, 495));
            SnakeInTheBox.AddAnimations("BXRIGHT", new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("Entity/Level1/Vehicles/BOX_RIGHT"), 20, 0.01f));
        }

        public void PopulateLeaderboard(JArray listofEntries)
        {
            if(listofEntries != null)
            {
                LeaderboardList.AddRange(listofEntries.ToObject<List<LeaderboardEntry>>());
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Background, new Vector2(0,0), Color.White);
            spriteBatch.Draw(Leaderboard, new Vector2(TheLastSliceGame.Instance.GameWidth /2 - Leaderboard.Width/2, 25), Color.White);

            int yPos = 100;
            int numEntriesToShow = (LeaderboardList.Count <= 10) ? LeaderboardList.Count : 10;
            int yOffset = 10;
            for (int i=0; i < numEntriesToShow; i++)
            {
                LeaderboardEntry entry = LeaderboardList[i];
                spriteBatch.DrawString(Text, entry.Initials, new Vector2(200, yPos), ColorYellow);
                if(entry.Challenge1 != null)
                {
                    spriteBatch.Draw(entry.Challenge1, new Vector2(300, yPos- yOffset), Color.White);
                }

                if (entry.Challenge2 != null)
                {
                    spriteBatch.Draw(entry.Challenge2, new Vector2(400, yPos - yOffset), Color.White);
                }

                if (entry.Challenge3 != null)
                {
                    spriteBatch.Draw(entry.Challenge3, new Vector2(500, yPos - yOffset), Color.White);
                }
                yPos += 40;
            }

            if (SnakeInTheBox.AnimationManager.Animation.CurrentFrame % 2 == 0)
            {
                Vector2 newSnakePos = new Vector2(SnakeInTheBox.Position.X + 1, SnakeInTheBox.Position.Y);
                SnakeInTheBox.Position = newSnakePos;
                SnakeInTheBox.AnimationManager.Position = newSnakePos;
            }

            if (SnakeInTheBox.Position.X < TheLastSliceGame.Instance.GameWidth)
            {
                SnakeInTheBox.Draw(spriteBatch);
            }

            ArrowLeft.Draw(spriteBatch);
            ArrowRight.Draw(spriteBatch);
            spriteBatch.Draw(Exit, new Vector2(TheLastSliceGame.Instance.GameWidth / 2 - Exit.Width / 2, 525), Color.White);
        }

        public override void Update(GameTime time)
        {
            if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Enter))
            {
                TheLastSliceGame.Instance.ChangeState(GameState.Menu, UIType.MainMenu);
            }

            SnakeInTheBox.Update(time);
            ArrowLeft.Update(time);
            ArrowRight.Update(time);
        }
    }
}



        
