using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using TheLastSlice.Entities;
using TheLastSlice.Managers;
using TheLastSlice.Models;
using TheLastSlice.UI;

namespace TheLastSlice
{
    public enum GameState { None, Menu, Game, NewGame, GameOver };
    public enum GameOverReason { Gas, Death, DeathByPaperBoy, WrongDelivery, WrongDeliveryFrog, Win };

    public class TheLastSliceGame : Game
    {
        public static System.Random Random { get; private set; }
        public static MapsMananger MapManager { get; private set; }
        public static LevelManager LevelManager { get; private set; }
        public static InputManager InputManager { get; private set; }
        public static TheLastSliceGame Instance { get; private set; }

        public bool ShowDebugHUD { get; set; }
        public int GameWidth { get; private set; }
        public int GameHeight { get; private set; }
        public int EntityWidth { get; private set; }
        public int EntityHeight { get; private set; }
        public int PlayerStartRow { get; private set; }
        public int PlayerStartColumn { get; private set; }

        public Player Player { get; private set; }
        public GameState GameState { get; private set; }
        public UIScreen CurrentScreen { get; private set; }
        public GameplayHUD HUD { get; private set; }
        public AppInsightsClient AppInsights { get; private set; }
        public GameService GameService { get; private set; }

        private GraphicsDeviceManager Graphics { get; set; }
        private SpriteBatch SpriteBatch { get; set; }
        private MainMenu MainMenu { get; set; }
        private GameOverMenu GameOverMenu { get; set; }
        private GameWinMenu GameWinMenu { get; set; }
        private LevelTransitionScreen LevelTransition { get; set; }
        private LeaderboardMenu Leaderboard { get; set; }
        private GameState NextGameState { get; set; }
        private UIType NextUIType { get; set; }  
        private SpriteFont m_DebugText;
        private Texture2D m_DebugPixel;   
        private Texture2D Border { get; set; }
        private Texture2D ScanLines { get; set; }
        private SoundEffect MenuMusic { get; set; }
        private SoundEffect GameMusic { get; set; }
        private SoundEffectInstance CurrentMusicInstance { get; set; }

        public TheLastSliceGame()
        {
            Graphics = new GraphicsDeviceManager(this);
            Graphics.IsFullScreen = false;
            Graphics.PreferMultiSampling = false;
            Graphics.SynchronizeWithVerticalRetrace = true;
            GameService = new GameService();
            Content.RootDirectory = "Content";
            Instance = this;
            Random = new System.Random();
            MapManager = new MapsMananger();
            LevelManager = new LevelManager();
            InputManager = new InputManager();
            MainMenu = new MainMenu();
            LevelTransition = new LevelTransitionScreen();
            HUD = new GameplayHUD();
            GameOverMenu = new GameOverMenu();
            GameWinMenu = new GameWinMenu();
            Leaderboard = new LeaderboardMenu();
            ChangeState(GameState.Menu, UIType.MainMenu);
            AppInsights = new AppInsightsClient();

            GameWidth = 800;
            GameHeight = 600;
            EntityWidth = 50;
            EntityHeight = 50;
            PlayerStartRow = 1;
            PlayerStartColumn = 0;

            //Setting this here means that anywhere in the game, a new TelemetryClient can be created and will get the correct ID.
            TelemetryConfiguration.Active.InstrumentationKey = "1445a599-ec52-4962-9abd-62b3cdae33b3";
        }

        public Matrix GetScaleMatrix()
        {
            float scaleX = (float)GraphicsDevice.Viewport.Width / GameWidth;
            float scaleY = (float)GraphicsDevice.Viewport.Height / GameHeight;
            return Matrix.CreateScale(scaleX, scaleY, 1.0f);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Player = new Player(new Vector2(EntityWidth * (float)PlayerStartColumn, EntityHeight * (float)(PlayerStartRow + 1)));
            m_DebugText = Content.Load<SpriteFont>("Fonts/Joystix_12");
            MapManager.LoadMaps(); //Maps must be loaded before levels - H.E.
            LevelManager.LoadLevels();
            MainMenu.LoadContent();
            LevelTransition.LoadContent();
            HUD.LoadContent();
            GameOverMenu.LoadContent();
            GameWinMenu.LoadContent();
            LevelTransition.LoadContent();
            Leaderboard.LoadContent();
            GetLeaderBoardResultsAsync();
            m_DebugPixel = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            m_DebugPixel.SetData(new[] { Color.White });

            ScanLines = Content.Load<Texture2D>("UI/ScanLines");
            Border = Content.Load<Texture2D>("UI/Border");

            MenuMusic = Content.Load<SoundEffect>("Sounds/Menu");
            GameMusic = Content.Load<SoundEffect>("Sounds/Main"); //Sounds/Last_slice_hiphop
        }

        public bool IsUserLoggedIn()
        {
            bool hasUserLoggedIn = GameService.HasUserLoggedIn();

            return hasUserLoggedIn;
        }

        protected async Task LoginAsync()
        {
            GameService.ClearUserCache();

            string token = await GameService.Login();
        }

        public async Task PostScoreAsync(string score, string initials)
        {
            string result = await GameService.PostScoreAsync(score, initials);

            GameWinMenu.SetCustomString(result);
        }

        protected async Task<JArray> GetLeaderboardAsync(bool showCurrentUser)
        {
            JArray scores = null;

            if (showCurrentUser)
            {
                scores = await GameService.GetLeaderboardWithCurrentUserAsync();
            }
            else
            {
                scores = await GameService.GetLeaderboardAsync();
            }

            return scores;   
        }

        public async void GetLeaderBoardResultsAsync()
        {
            JArray results = null;

            try
            {
                results = await GetLeaderboardAsync(IsUserLoggedIn());
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine("An problem occured while getting the leader board: " + ex.ToString());
            }
            
            Leaderboard.PopulateLeaderboard(results);
        }

        public async void GameWinAsync()
        {
            if(!IsUserLoggedIn())
            {
                await LoginAsync();
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        public void GameOver(GameOverReason gameOverReason)
        {
            GameOverMenu.GameOverReason = gameOverReason;
            GameOverMenu.Score = Player.Score;
            AppInsights.GameOver(gameOverReason);
            ChangeState(GameState.GameOver);
        }

        public void ChangeState(GameState state, UIType type = UIType.None)
        {
            NextGameState = state;
            NextUIType = type;
        }

        public void StopMusic()
        {
            if (CurrentMusicInstance != null)
            {
                CurrentMusicInstance.Stop();
            }
        }

        public void PauseMusic()
        {
            if (CurrentMusicInstance != null)
            {
                CurrentMusicInstance.Pause();
            }
        }

        public void PlayMusic()
        {
            if (CurrentMusicInstance != null)
            {
                CurrentMusicInstance.Play();
            }
        }

        private void OnStateChange()
        {
            if(CurrentMusicInstance != null)
            {
                if(!(NextGameState == GameState.Menu && GameState == GameState.Menu))
                {
                    CurrentMusicInstance.Stop();
                }
                else if(CurrentMusicInstance.State == SoundState.Paused)
                {
                    CurrentMusicInstance.Play();
                }
            }

            if (NextGameState == GameState.GameOver) //Answer me Snake...Snake?! SNAKEEEEEEE!!!"
            {
                if (GameOverMenu.GameOverReason == GameOverReason.Win)
                {
                    CurrentScreen = GameWinMenu;
                    GameWinMenu.LastPlayerScore = Player.Score;
                }
                else
                {
                    CurrentScreen = GameOverMenu;
                    GameOverMenu.Awake();
                }

                GameState = GameState.Menu;
                Player.OnGameOver();
                LevelManager.OnGameOver();
                NextGameState = GameState.None;

                CurrentMusicInstance = MenuMusic.CreateInstance();
                CurrentMusicInstance.IsLooped = true;
                CurrentMusicInstance.Play();
            }
            else
            {
                if (NextGameState == GameState.Menu)
                {
                    switch (NextUIType)
                    {
                        case UIType.MainMenu:
                        {
                            CurrentScreen = MainMenu;
                            break;
                        }
                        case UIType.LevelTransition:
                        {
                            LevelTransition.ResetState();
                            CurrentScreen = LevelTransition;
                            break;
                        }
                        case UIType.GameOver:
                        {
                            CurrentScreen = GameOverMenu;
                            break;
                        }
                        case UIType.Leaderboard:
                        {
                            CurrentScreen = Leaderboard;
                            break;
                        }
                        default:
                        {
                            break;
                        }
                    }
                }
                else if (NextGameState == GameState.Game)
                {
                    LevelManager.NextLevel();
                }
                else if (NextGameState == GameState.NewGame)
                {
                    NewGame();
                }

                if (NextGameState == GameState.Game)
                {
                    CurrentMusicInstance = GameMusic.CreateInstance();
                    CurrentMusicInstance.IsLooped = true;
                }
                else if (CurrentMusicInstance == null || (GameState == GameState.Menu && CurrentScreen != LevelTransition && !(NextGameState == GameState.Menu && GameState == GameState.Menu)))
                {
                    CurrentMusicInstance = MenuMusic.CreateInstance();
                    CurrentMusicInstance.IsLooped = true;
                    CurrentMusicInstance.Play();
                }

                GameState = NextGameState;
            }

            NextGameState = GameState.None;
            NextUIType = UIType.None;
        }

        private void NewGame()
        {
            //It's time to kick a** and chew bubblegum... and I'm all outta gum.
            MapManager = new MapsMananger();
            LevelManager = new LevelManager();
            MapManager.LoadMaps();
            LevelManager.LoadLevels();
            LevelManager.NextLevel();
            MapManager.ChangeMap(MapManager.Maps[0]);
            ChangeState(GameState.Game);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (IsActive == false)
            {
                return;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (NextGameState != GameState.None)
            {
                OnStateChange();
            }

            IsMouseVisible = false;

            if (GameState == GameState.Menu)
            {
                CurrentScreen.Update(gameTime);
            }
            else if (GameState == GameState.Game)
            {
                //Move
                //Resolve physics 
                //PostUpdate
                //Draw
                //?
                //Profit

                MapManager.CurrentMap.Update(gameTime);
                LevelManager.CurrentLevel.Update(gameTime);
                Player.Update(gameTime);
                base.Update(gameTime);
                HUD.Update(gameTime);

                MapManager.CurrentMap.ResolveCollisions();
                MapManager.CurrentMap.PostUpdate();
                Player.PostUpdate();
            }

            InputManager.Update();
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (IsActive == false)
            {
                return;
            }

            SpriteBatch.Begin(SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: GetScaleMatrix());

            if (GameState == GameState.Menu)
            {
                Graphics.GraphicsDevice.Clear(Color.Blue);
                CurrentScreen.Draw(SpriteBatch);
            }
            else if (GameState == GameState.Game)
            {
                MapManager.CurrentMap.Draw(SpriteBatch);
                LevelManager.CurrentLevel.Draw(SpriteBatch);
                Player.Draw(SpriteBatch);
                HUD.Draw(SpriteBatch);
            }

            if (ShowDebugHUD)
            {
                DrawDebugHUD(SpriteBatch);
            }

            if(LevelManager.CurrentLevelNum > 2)
            {
                SpriteBatch.Draw(ScanLines, new Vector2(0, 0), Color.White * 0.2f);
            }
            else
            {
                SpriteBatch.Draw(ScanLines, new Vector2(0, 0), Color.White);
            }

            if (GameState == GameState.Menu)
            {
                SpriteBatch.Draw(Border, new Vector2(0, 0), Color.White);
            }

            SpriteBatch.End();

            base.Draw(gameTime);
        }

        public void DrawDebugBorder(Rectangle rectangleToDraw, int thicknessOfBorder, Color borderColor)
        {
            // Draw top line
            SpriteBatch.Draw(m_DebugPixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), borderColor);

            // Draw left line
            SpriteBatch.Draw(m_DebugPixel, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), borderColor);

            // Draw right line
            SpriteBatch.Draw(m_DebugPixel, new Rectangle((rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder),
                                            rectangleToDraw.Y,
                                            thicknessOfBorder,
                                            rectangleToDraw.Height), borderColor);
            // Draw bottom line
            SpriteBatch.Draw(m_DebugPixel, new Rectangle(rectangleToDraw.X,
                                            rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder,
                                            rectangleToDraw.Width,
                                            thicknessOfBorder), borderColor);
        }


        void DrawDebugHUD(SpriteBatch spriteBatch)
        {
            Texture2D leftDebugHud = new Texture2D(Graphics.GraphicsDevice, 250, 150);
            Color[] leftDebugHUDData = new Color[250 * 150];
            for (int i = 0; i < leftDebugHUDData.Length; ++i)
            {
                leftDebugHUDData[i] = Color.Black;
            }
            leftDebugHud.SetData(leftDebugHUDData);
            SpriteBatch.Draw(leftDebugHud, new Vector2(0, 0), Color.White);

            SpriteBatch.DrawString(m_DebugText, "Score:" + Player.Score, new Vector2(0, 10), Color.Yellow);
            SpriteBatch.DrawString(m_DebugText, "Level:" + LevelManager.CurrentLevelNum, new Vector2(0, 30), Color.Yellow);
            SpriteBatch.DrawString(m_DebugText, "Num Deliveries:" + LevelManager.CurrentLevel.TotalNumDeliveries, new Vector2(0, 50), Color.Yellow);
            SpriteBatch.DrawString(m_DebugText, "Num Completed Deliveries:" + LevelManager.CurrentLevel.CompletedNumDeliveries, new Vector2(0, 70), Color.Yellow);
            SpriteBatch.DrawString(m_DebugText, "Grid Location:" + Player.GetPositionOnGrid(), new Vector2(0, 90), Color.Yellow);

            Texture2D rightDebugHud = new Texture2D(Graphics.GraphicsDevice, 250, 500);
            Color[] rightDebugHUDData = new Color[250 * 500];
            for (int i = 0; i < rightDebugHUDData.Length; ++i)
            {
                rightDebugHUDData[i] = Color.Black;
            }
            rightDebugHud.SetData(rightDebugHUDData);
            SpriteBatch.Draw(rightDebugHud, new Vector2(GameWidth - 140, 0), Color.White);

            int deliveryTextY = 10;
            SpriteBatch.DrawString(m_DebugText, "Current Delivery:", new Vector2(GameWidth - 120, deliveryTextY), Color.Yellow);
            deliveryTextY += 10;
            SpriteBatch.DrawString(m_DebugText, "----------------------------------", new Vector2(GameWidth - 120, deliveryTextY), Color.Yellow);
            deliveryTextY += 20;
            SpriteBatch.DrawString(m_DebugText, "Grid Location:" + LevelManager.CurrentLevel.CurrentDelivery.GetPositionOnGrid(), new Vector2(GameWidth - 150, deliveryTextY), Color.Yellow);
            deliveryTextY += 20;
            SpriteBatch.DrawString(m_DebugText, "----------------------------------", new Vector2(GameWidth - 120, deliveryTextY), Color.Yellow);

            foreach (Ingredient ingredient in LevelManager.CurrentLevel.CurrentDelivery.Pizza)
            {
                deliveryTextY += 20;
                SpriteBatch.DrawString(m_DebugText, ingredient.IngredientType.ToString(), new Vector2(GameWidth - 100, deliveryTextY), Color.Yellow);
            }

            deliveryTextY += 50;
            SpriteBatch.DrawString(m_DebugText, "Current Ingredients:", new Vector2(GameWidth - 120, deliveryTextY), Color.Yellow);
            deliveryTextY += 10;
            SpriteBatch.DrawString(m_DebugText, "----------------------------------", new Vector2(GameWidth - 120, deliveryTextY), Color.Yellow);
            foreach (Ingredient ingredient in Player.PizzaIngredients)
            {
                deliveryTextY += 20;
                SpriteBatch.DrawString(m_DebugText, ingredient.IngredientType.ToString(), new Vector2(GameWidth - 100, deliveryTextY), Color.Yellow);
            }
        }
    }
}
