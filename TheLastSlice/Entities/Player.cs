using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using TheLastSlice.Managers;
using TheLastSlice.Models;

namespace TheLastSlice.Entities
{
    public class Player : Pawn
    {
        public bool CanMove { get; set; }
        public List<Pickup> Inventory { get; private set; }
        public List<Ingredient> PizzaIngredients { get; private set; }  //Ingredients get cleared after each delivery
        public int Score { get; private set; }
        public int Gas { get; private set; }
        public int MaxGas { get; private set; }
        public int LowGasThreshold { get; private set; }
        public bool HeartMode { get; set; }

        private TimeSpan GasTimer { get; set; }
        private TimeSpan DeathTimer { get; set; }       //The timer that gets set when this entity dies and changes the game state when reached
        private TimeSpan BoxTimer { get; set; }
        private TimeSpan LowGasTimer { get; set; }
        private TimeSpan HeartTimer { get; set; }
        private int GasTimerMS { get; set; }
        private Vector2 InitialPosition { get; set; }
        private bool IsDead { get; set; }
        private bool IsBox { get; set; }
        private bool HitPB { get; set; }
        private float BoxSpeed { get; set; }
        private SoundEffect CollideSound { get; set; }
        private SoundEffectInstance CollideEffectInstance { get; set; }
        private SoundEffect LowGasSound { get; set; }
        private SoundEffectInstance LowGasEffectInstance { get; set; }
        private List<Rectangle> tempCollisions { get; set; }
        private bool IsChangingMap { get; set; } //Are we changing maps this frame?

        private const string UP = "UP";
        private const string DOWN = "DOWN";
        private const string LEFT = "LEFT";
        private const string RIGHT = "RIGHT";
        private const string BXUP = "BXUP";
        private const string BXDOWN = "BXDOWN";
        private const string BXLEFT = "BXLEFT";
        private const string BXRIGHT = "BXRIGHT";

        public Player(Vector2 position, String assetCode = null) : base(position, assetCode)
        {
            //We only have one type of car right now
            Animations = new Dictionary<string, Animation>();
            AssetCode = assetCode;
            SoundEffect = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Explosion");
            CollideSound = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Collide");
            CollideEffectInstance = CollideSound.CreateInstance();
            LowGasSound = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/LowGas");
            LowGasEffectInstance = LowGasSound.CreateInstance();
            Vector2 posAtGrid = GetPositionOnGrid();
            int x = ((int)posAtGrid.X * Map.CellWidth) + (Map.CellWidth / 2) - HalfWidth;
            Position = new Vector2(x, Position.Y);
            InitialPosition = Position;
            GasTimer = TimeSpan.Zero;
            DeathTimer = TimeSpan.Zero;
            BoxTimer = TimeSpan.Zero;
            LowGasTimer = TimeSpan.Zero;
            HeartTimer = TimeSpan.Zero;
            Score = 0;
            Speed = 200.0f;
            BoxSpeed = 100.0f;
            Inventory = new List<Pickup>();
            PizzaIngredients = new List<Ingredient>();
            MaxGas = 646;
            Gas = MaxGas;
            LowGasThreshold = 200;
            IsDead = false;
            IsBox = false;
            GasTimerMS = 130;
            HeartMode = false;
            CanMove = false;
            tempCollisions = new List<Rectangle>();
            HitPB = false;
        }

        public override void LoadTexture()
        {
            int currentLevel = TheLastSliceGame.LevelManager.CurrentLevelNum;
            String assetPath = "Entity/Level" + currentLevel.ToString() + "/Vehicles";
            Animations.Clear();

            Animations.Add(DOWN, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath +  "/truck_down"), 4));
            Animations.Add(LEFT, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/truck_left"), 4));
            Animations.Add(RIGHT, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/truck_right"), 4));
            Animations.Add(UP, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/truck_up"), 4));

            Animations.Add(BXLEFT, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/BOX_LEFT"), 20, 0.01f));
            Animations.Add(BXRIGHT, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/BOX_RIGHT"), 20, 0.01f));
            Animations.Add(BXDOWN, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/BXD"), 1));
            Animations.Add(BXUP, new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/BXU"), 1));

            Animations.Add("EX", new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>(assetPath + "/EX"), 8));
            AnimationManager = new AnimationManager(Animations.First().Value);
            Height = (Animations.First().Value.FrameHeight);
            Width = (Animations.First().Value.FrameWidth);

            Position = InitialPosition;
            Vector2 posAtGrid = GetPositionOnGrid();
            int x = ((int)posAtGrid.X * Map.CellWidth) + (Map.CellWidth / 2) - AnimationManager.Animation.FrameWidth / 2;
            int y = ((int)posAtGrid.Y * Map.CellHeight) + (Map.CellHeight / 2) - AnimationManager.Animation.FrameHeight / 2 + TheLastSliceGame.MapManager.MapStartingYPos;
            Position = new Vector2(x, y);
            InitialPosition = Position;
        }

        public void Reset()
        {
            TheLastSliceGame.MapManager.ChangeMap(TheLastSliceGame.MapManager.Maps[0]);
            Position = InitialPosition;
            Inventory.RemoveAll(pickup => pickup is Ingredient);
            PizzaIngredients.Clear();
        }

        public void OnChangeMap()
        {
            IsChangingMap = true;
        }

        public override Vector2 Position
        {
            get { return m_Position; }
            set
            {
                //When the position changes we need to update the collision component as well. - H.E.
                m_Position = value;
          
                if (AnimationManager != null)
                {
                    Animation currentAnim = AnimationManager.Animation;
                    if (currentAnim != null)
                    {
                        if(!IsBox)
                        {
                            //Collision is at the front of the car
                            if (currentAnim == Animations[RIGHT])
                            {
                                CollisionComponent = new Rectangle((int)value.X + HalfWidth, (int)value.Y, HalfWidth, Height);
                            }
                            else if (currentAnim == Animations[LEFT])
                            {
                                CollisionComponent = new Rectangle((int)value.X, (int)value.Y, HalfWidth, Height);
                            }
                            else if (currentAnim == Animations[UP])
                            {
                                CollisionComponent = new Rectangle((int)value.X, (int)value.Y, Width, HalfHeight);
                            }
                            else if (currentAnim == Animations[DOWN])
                            {
                                CollisionComponent = new Rectangle((int)value.X, (int)value.Y + HalfHeight, Width, HalfHeight);
                            }
                        }
                        else
                        {
                            if (currentAnim == Animations[BXRIGHT])
                            {
                                CollisionComponent = new Rectangle((int)value.X + HalfWidth, (int)value.Y + HalfHeight/4, HalfWidth, HalfHeight);
                            }
                            else if (currentAnim == Animations[BXLEFT])
                            {
                                CollisionComponent = new Rectangle((int)value.X, (int)value.Y + HalfHeight / 4, HalfWidth, HalfHeight);
                            }
                            else if (currentAnim == Animations[BXUP])
                            {
                                CollisionComponent = new Rectangle((int)value.X, (int)value.Y, Width, HalfHeight);
                            }
                            else if (currentAnim == Animations[BXDOWN])
                            {
                                CollisionComponent = new Rectangle((int)value.X, (int)value.Y + HalfHeight, Width, HalfHeight);
                            }
                        }
                    }

                    AnimationManager.Position = m_Position;
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            IsChangingMap = false;
            if (!IsBox && CanMove && gameTime.TotalGameTime.TotalMilliseconds > GasTimer.TotalMilliseconds)
            {
                UpdateGas(-1);
                GasTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + GasTimerMS);

                if(Gas < LowGasThreshold && !IsDead)
                {
                    if(LowGasTimer == TimeSpan.Zero)
                    {
                        LowGasEffectInstance.Play();
                        LowGasTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 2000);
                    }
                    else if(gameTime.TotalGameTime.TotalMilliseconds > LowGasTimer.TotalMilliseconds)
                    {
                        LowGasEffectInstance.Play();
                        if (Gas < 100)
                        {
                            LowGasTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 500);
                        }
                        else if (Gas < 150)
                        {
                            LowGasTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 1000);
                        }
                        else
                        {
                            LowGasTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 2000);
                        }
                    }    
                }
            }

            if (IsDead && DeathTimer == TimeSpan.Zero)
            {
                DeathTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 3000);
            }

            if (IsBox)
            {
                if(BoxTimer == TimeSpan.Zero)
                {
                    BoxTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 60000);
                }
                else if(gameTime.TotalGameTime.TotalMilliseconds > BoxTimer.TotalMilliseconds)
                {
                    IsBox = false;
                }
            }
            
            if(HeartMode)
            {
                if (HeartTimer == TimeSpan.Zero)
                {
                    Speed = 400;
                    HeartTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 45000);
                }
                else if (gameTime.TotalGameTime.TotalMilliseconds > HeartTimer.TotalMilliseconds)
                {
                    HeartMode = false;
                    Speed = 200;
                }
            }

            if (IsDead && gameTime.TotalGameTime.TotalMilliseconds > DeathTimer.TotalMilliseconds)
            {
                if (Gas <= 0)
                {
                    TheLastSliceGame.Instance.GameOver(GameOverReason.Gas);
                }
                else if(HitPB)
                {
                    TheLastSliceGame.Instance.GameOver(GameOverReason.DeathByPaperBoy);
                }
                else
                {
                    TheLastSliceGame.Instance.GameOver(GameOverReason.Death);
                }
            }

            if(CanMove)
            {
                Move(gameTime);
            }

            AnimationManager.Update(gameTime);
        }

        public void OnLevelComplete()
        {
            Score += Gas;
            Gas = MaxGas;
            IsBox = false;
            CanMove = false;
            Score += 200;
        }

        private void UpdateGas(int gasAmount)
        {
            if (IsDead || !CanMove)
            {
                return;
            }

            if(HeartMode)
            {
                gasAmount *= 3;
            }

            Gas += gasAmount;
            Gas = Math.Min(Gas, MaxGas);
            if(Gas <= 0)
            {
                //Game over sucka - H.E.
                OnKilled();
            }

            if(Gas > LowGasThreshold && LowGasTimer != TimeSpan.Zero)
            {
                LowGasTimer = TimeSpan.Zero;
            }
        }

        private void AttemptDelivery()
        {
            /* Houses can only face downwards/south because we live in a nightmare neighborhood and the player can only deliver to houses that are a row above them on the map - H.E.
            * ___________________
            *|      |   H  |  H  |
            *|______|______|_____|
            *|      |  C   |     |
            *|______|______|_____|
            */

            //TODO:: Make sure we're not on the cell itself
            Vector2 gridPos = GetPositionOnGrid();
            int row = (int)gridPos.Y - 1;
            if (row >= 0) //Make sure the row above is a valid row
            {
                int column = (int)gridPos.X - 1;
                for (int i = column; i <= column + 2; i++)
                {
                    if(i >= 0 && i <= TheLastSliceGame.MapManager.CurrentMap.NumColumns -1)
                    {
                        House house = TheLastSliceGame.MapManager.CurrentMap.GetEntityAtCell(row, i) as House;
                        if (house != null)
                        {
                            float distance = Vector2.Distance(Position, house.Position);
                            int validDistance = (house.Height + house.HalfHeight);
                            if(distance < validDistance && house == TheLastSliceGame.LevelManager.CurrentLevel.CurrentDelivery)
                            {
                                TheLastSliceGame.LevelManager.CurrentLevel.DeliverPizza(house, PizzaIngredients);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void Move(GameTime gameTime)
        {
            if(IsDead || !CanMove) //Stay a while and listen.
            {
                return;
            }
            
            //Make the controls frustrating for the player because reasons
            KeyboardState keyboardState = Keyboard.GetState();
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
            OldVelocity = Velocity;
            float currentSpeed = Speed;

            if (IsBox)
            {
                currentSpeed = BoxSpeed;
            }

            if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Enter))
            {
                AttemptDelivery();
            }
            else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Home))
            {
                TheLastSliceGame.Instance.ShowDebugHUD = !TheLastSliceGame.Instance.ShowDebugHUD;
            }
            else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Up))
            {
                Velocity = new Vector2(0, -currentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Down))
            {
                Velocity = new Vector2(0, currentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Left))
            {
                Velocity = new Vector2(-currentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
            }
            else if (TheLastSliceGame.InputManager.IsInputPressed(Keys.Right))
            {
                Velocity = new Vector2(currentSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds, 0);
            }

            SetAnimations();
            Position = ResolveMapEdgeCollisions(Position + Velocity);

            //Pac man controls/physics ? - H.E.
            //Did you know that the original name for Pac - Man was Puck - Man ? You'd think it was because he looks like a hockey puck but it actually comes from the 
            //Japanese phrase 'Paku - Paku, ' which means to flap one's mouth open and closed. They changed it because they thought Puck - Man would be too easy to 
            //vandalize, you know, like people could just scratch off the P and turn it into an F or whatever.

            base.Move(gameTime);
        }

        protected Vector2 ResolveMapEdgeCollisions(Vector2 positionNextFrame)
        {
            //Since the player is the only one who can travel between maps, we need to check the collision on the map the player is about to go to. 
            //Why did we make the grid system this way you ask? ¯\_(ツ)_/¯ - H.E.
            Map currentMap = TheLastSliceGame.MapManager.CurrentMap;
            Vector2 resolvedPosition = positionNextFrame;
            Vector2 positionOnGrid = GetPositionOnGrid();
            //TODO:: Check collisions with other cars? 

            //If the player is at an edge of the screen and there is no map in the direction they are going we stop them at the edge.
            //If the player is at an edge and there is a valid map in the direction they are going, we let them go a little bit past the edge to show them there is something in that direction.
            //If the player is at an edge and there is a valid map in in the direction they are going, we need to check for collisions on the other side of the map.
            if (positionNextFrame.X > TheLastSliceGame.Instance.GameWidth - Width) //Right
            {
                if (currentMap.MapRight == null)
                {
                    RevertPosition();
                    PlayCollideSFX();
                    return Position;
                }
                else if (positionNextFrame.X > (TheLastSliceGame.Instance.GameWidth - HalfWidth))
                {
                    //If we're going off the edge of the map we need to check to make sure the position on the map we're going to isn't blocked by an entity
                    if (currentMap.MapRight.IsCollidingNearCell(new Rectangle(0, (int)Position.Y, Width, Height), (int)positionOnGrid.Y, 0))
                    {
                        PlayCollideSFX();
                        return Position;
                    }
                    else
                    {
                        resolvedPosition = new Vector2(0, Position.Y);
                        TheLastSliceGame.MapManager.ChangeMap(currentMap.MapRight); //GET OVER HERE
                    }
                }
            }
            else if (positionNextFrame.X < 0) //Left
            {
                if (currentMap.MapLeft == null)
                {
                    RevertPosition();
                    PlayCollideSFX();
                    return Position;
                }
                else if (positionNextFrame.X < (0 - HalfWidth))
                {
                    //If we're going off the edge of the map we need to check to make sure the position on the map we're going to isn't blocked by an entity
                    if (currentMap.MapLeft.IsCollidingNearCell(new Rectangle(TheLastSliceGame.Instance.GameWidth - Width, (int)Position.Y, Width, Height), (int)positionOnGrid.Y, currentMap.MapLeft.NumColumns - 1))
                    {
                        PlayCollideSFX();
                        resolvedPosition = Position;
                    }
                    else
                    {
                        resolvedPosition = new Vector2(TheLastSliceGame.Instance.GameWidth - HalfWidth, Position.Y);
                        TheLastSliceGame.MapManager.ChangeMap(currentMap.MapLeft);  //GET OVER HERE
                    }
                }
            }
            else if (positionNextFrame.Y < TheLastSliceGame.MapManager.MapStartingYPos) //Up
            {
                int heightPadding = IsBox ? Height : HalfHeight;
                if (currentMap.MapUp == null)
                {
                    RevertPosition();
                    PlayCollideSFX();
                    return Position;
                }
                else if (positionNextFrame.Y < (TheLastSliceGame.MapManager.MapStartingYPos - heightPadding))
                {
                    //If we're going off the edge of the map we need to check to make sure the position on the map we're going to isn't blocked by an entity
                    if (currentMap.MapUp.IsCollidingNearCell(new Rectangle((int)Position.X, TheLastSliceGame.Instance.GameHeight - Height, Width, Height), currentMap.MapUp.NumRows - 1, (int)positionOnGrid.X))
                    {
                        PlayCollideSFX();
                        return Position;
                    }
                    else
                    {
                        resolvedPosition = new Vector2(Position.X, TheLastSliceGame.Instance.GameHeight - heightPadding);
                        TheLastSliceGame.MapManager.ChangeMap(currentMap.MapUp);  //GET OVER HERE
                    }
                }
            }
            else if (positionNextFrame.Y > TheLastSliceGame.Instance.GameHeight - Height) //Down
            {
                int heightPadding = IsBox ? Height : HalfHeight;

                if (currentMap.MapDown == null)
                {
                    RevertPosition();
                    PlayCollideSFX();
                    return Position;
                }
                else if (positionNextFrame.Y > (TheLastSliceGame.Instance.GameHeight - heightPadding))
                {
                    //If we're going off the edge of the map we need to check to make sure the position on the map we're going to isn't blocked by an entity
                    if (currentMap.MapDown.IsCollidingNearCell(new Rectangle((int)Position.X, TheLastSliceGame.MapManager.MapStartingYPos, Width, Height), 0, (int)positionOnGrid.X))
                    {
                        PlayCollideSFX();
                        return Position;
                    }
                    else
                    {
                        resolvedPosition = new Vector2(Position.X, TheLastSliceGame.MapManager.MapStartingYPos);
                        TheLastSliceGame.MapManager.ChangeMap(currentMap.MapDown);  //GET OVER HERE
                    }
                }
            }

            return resolvedPosition;
        }

        private void PlayCollideSFX()
        {
            CollideEffectInstance.Play();
        }

        public override void OnBlockingCollisionResolvePosition(Entity collidedWith)
        {
            Vector2 initialCollisionPos = Position;
            Vector2 collidedWithGridPos = collidedWith.GetPositionOnGrid();
            Entity tempEntity = null;
            bool foundValidPosition = false;
            Rectangle overlap = new Rectangle();

            if (Math.Abs(Velocity.X) > 0 && (AnimationManager.PreviousAnimation == Animations[DOWN] || AnimationManager.PreviousAnimation == Animations[BXDOWN] ||
               AnimationManager.PreviousAnimation == Animations[UP] || AnimationManager.PreviousAnimation == Animations[BXUP]))
            {
                //Collision "sweep" one grid position down
                Position = new Vector2(initialCollisionPos.X, initialCollisionPos.Y + CollisionComponent.Height);
                tempEntity = TheLastSliceGame.MapManager.CurrentMap.GetEntityAtCell((int)collidedWithGridPos.Y + 1, (int)collidedWithGridPos.X);
                if (!IsColliding(collidedWith, out overlap) && (tempEntity == null || !tempEntity.IsBlocking || !IsColliding(tempEntity, out overlap)))
                {
                    foundValidPosition = true;
                }
                else
                {
                    //Collision "sweep" one grid position up
                    Position = new Vector2(initialCollisionPos.X, initialCollisionPos.Y - CollisionComponent.Height);
                    tempEntity = TheLastSliceGame.MapManager.CurrentMap.GetEntityAtCell((int)collidedWithGridPos.Y - 1, (int)collidedWithGridPos.X);
                    if (!IsColliding(collidedWith, out overlap) && (tempEntity == null || !tempEntity.IsBlocking || !IsColliding(tempEntity, out overlap)))
                    {
                        foundValidPosition = true;
                    }
                }
            }
            else if (Math.Abs(Velocity.Y) > 0 && (AnimationManager.PreviousAnimation == Animations[RIGHT] || AnimationManager.PreviousAnimation == Animations[BXRIGHT] ||
                    AnimationManager.PreviousAnimation == Animations[LEFT] || AnimationManager.PreviousAnimation == Animations[BXLEFT]))
            {
                //Collision "sweep" one grid position right
                Position = new Vector2(initialCollisionPos.X + CollisionComponent.Width, initialCollisionPos.Y);
                tempEntity = TheLastSliceGame.MapManager.CurrentMap.GetEntityAtCell((int)collidedWithGridPos.Y, (int)collidedWithGridPos.X + 1);
                if (!IsColliding(collidedWith, out overlap) && (tempEntity == null || !tempEntity.IsBlocking || !IsColliding(tempEntity, out overlap)))
                {
                    foundValidPosition = true;
                }
                else
                {
                    //Collision "sweep" one grid position left
                    Position = new Vector2(initialCollisionPos.X - CollisionComponent.Width, initialCollisionPos.Y);
                    tempEntity = TheLastSliceGame.MapManager.CurrentMap.GetEntityAtCell((int)collidedWithGridPos.Y, (int)collidedWithGridPos.X - 1);
                    if (!IsColliding(collidedWith, out overlap) && (tempEntity == null || !tempEntity.IsBlocking || !IsColliding(tempEntity, out overlap)))
                    {
                        foundValidPosition = true;
                    }
                }
            }

            if (!foundValidPosition)
            {
                RevertPosition();
            }
        }

        private void RevertPosition()
        {
            Velocity = Vector2.Zero; // -PAC MAN - H.E.
            //Velocity = OldVelocity;
            AnimationManager.PlayPreviousAnimation();

            if (AnimationManager != null && AnimationManager.Animation != null)
            {
                Width = AnimationManager.Animation.FrameWidth;
                Height = AnimationManager.Animation.FrameHeight;
            }

            //Do a barrel roll!
            Position = OldPosition; // + Velocity; -PAC MAN - H.E.
        }

        public bool IsInventoryFull()
        {
            return (PizzaIngredients.Count >= 4);
        }

        public override void OnCollided(Entity collidedWith)
        {
            if (collidedWith.IsBlocking && collidedWith.Type != EntityType.Obstacle)
            {
                OnBlockingCollisionResolvePosition(collidedWith);
            }

            Pickup pickup = collidedWith as Pickup;
            if (pickup != null)
            {
                Ingredient ingredient = pickup as Ingredient;

                if (ingredient != null)
                {
                    if(!IsInventoryFull() && !(ingredient.IsFrog() && HeartMode))
                    {
                        PizzaIngredients.Add(ingredient);
                        Score += pickup.GetScore();
                    }
                }
                else
                {
                    if (pickup.PickupType == PickupType.GS)
                    {
                        UpdateGas(200);
                    }
                    else if (pickup.PickupType == PickupType.TC)
                    {
                        //It's super effective
                        Dictionary<IngredientType, Ingredient> ingredientsToKeep = new Dictionary<IngredientType, Ingredient>();
                        foreach (Ingredient currentIngredient in TheLastSliceGame.LevelManager.CurrentLevel.CurrentDelivery.Pizza)
                        {
                            foreach (Ingredient deliveryIngredient in PizzaIngredients)
                            {
                                if(deliveryIngredient.IngredientType == currentIngredient.IngredientType && !ingredientsToKeep.ContainsKey(deliveryIngredient.IngredientType))
                                {
                                    ingredientsToKeep.Add(currentIngredient.IngredientType, currentIngredient);
                                }
                            }
                        }

                        PizzaIngredients.Clear();

                        foreach (KeyValuePair<IngredientType, Ingredient> ingredientToKeep in ingredientsToKeep)
                        {
                            PizzaIngredients.Add(ingredientToKeep.Value);
                        }

                        IsBox = false;
                        HeartMode = false;
                        Speed = 200;
                    }
                    else if (pickup.PickupType == PickupType.CU)
                    {
                        //It’s dangerous to go alone, take this
                        PizzaIngredients.Clear();
                        foreach (Ingredient pizzaIngredient in TheLastSliceGame.LevelManager.CurrentLevel.CurrentDelivery.Pizza)
                        {
                            PizzaIngredients.Add(pizzaIngredient);
                        }
                    }
                    else if (pickup.PickupType == PickupType.BX)
                    {
                        IsBox = true;
                        BoxTimer = TimeSpan.Zero;
                        HeartMode = false;
                        HeartTimer = TimeSpan.Zero;
                    }
                    else if (pickup.PickupType == PickupType.HE)
                    {
                        IsBox = false;
                        BoxTimer = TimeSpan.Zero;
                        HeartMode = true;
                        HeartTimer = TimeSpan.Zero;
                    }
                    else
                    {
                        Inventory.Add(pickup);
                    }

                    Score += pickup.GetScore();
                }
            }
            else if (!IsDead)
            {
                Obstacle obstacle = collidedWith as Obstacle;
                if(obstacle != null)
                {
                    if(obstacle.ObstacleType == ObstacleType.PB)
                    {
                        HitPB = true;
                    }

                    OnCollidedWithObstacle(obstacle);
                }
            }
        }

        private void OnCollidedWithObstacle(Obstacle obstacle)
        {
            switch (obstacle.ObstacleType)
            {
                //TODO:: Swap texture? - H.E.
                //War, war never changes.
                case ObstacleType.PB:
                {
                    if(!HeartMode)
                    {
                        OnKilled();
                    }
                    break;
                }
                case ObstacleType.PH:
                case ObstacleType.FI:
                default:
                {
                    OnKilled();
                    break;
                }
            }
        }

        private void OnKilled()
        {
            //YOU DIED
            SoundEffect.Play();
            IsDead = true;
            Animation prevAmin = AnimationManager.Animation;
            Animation anim = Animations["EX"];
            Vector2 prevCenter = new Vector2(Position.X + (prevAmin.FrameWidth / 2), Position.Y + (prevAmin.FrameHeight / 2));
            Position = new Vector2(prevCenter.X - (anim.FrameWidth / 2), prevCenter.Y - (anim.FrameHeight / 2));
            anim.IsLooping = false;
            AnimationManager.Play(anim);
            CanMove = false;
        }

        public void OnDeliveryComplete()
        {
            Inventory.RemoveAll(pickup => pickup is Ingredient);
            PizzaIngredients.Clear();
            Score += 50;
        }

        public void OnDeliveryFailed()
        {
            CanMove = false;
        }

        public void OnGameOver()
        {
            //I used to be an adventurer like you, until I took an arrow to the knee.
            Velocity = Vector2.Zero;
            Gas = MaxGas;
            Score = 0;
            Inventory.Clear();
            PizzaIngredients.Clear();
            Position = InitialPosition;
            GasTimer = TimeSpan.Zero;
            HeartTimer = TimeSpan.Zero;
            BoxTimer = TimeSpan.Zero;
            DeathTimer = TimeSpan.Zero;
            IsDead = false;
            IsBox = false;
            HeartMode = false;
            Animation anim = Animations[DOWN];
            Width = anim.FrameWidth;
            Height = anim.FrameHeight;
            AnimationManager.Play(anim);
            Speed = 200;
            CanMove = true;
            HitPB = false;
        }

        protected override void SetAnimations()
        {
            //Set the animation based on the velocity. If the player is trying to turn, make a new velocity moving in both the x and y direction to smooth the turning.
            Animation anim = null;

            if (Velocity.X > 0)
            {
                if (IsBox)
                {
                    anim = Animations[BXRIGHT];
                }
                else
                {
                    anim = Animations[RIGHT];
                }

                if (AnimationManager.Animation == Animations[DOWN] || AnimationManager.Animation == Animations[BXDOWN])
                {
                    Velocity = new Vector2(Velocity.X, anim.FrameHeight/2);
                }
            }
            else if (Velocity.X < 0)
            {
                if (IsBox)
                {
                    anim = Animations[BXLEFT];
                }
                else
                {
                    anim = Animations[LEFT];
                }

                if (AnimationManager.Animation == Animations[DOWN] || AnimationManager.Animation == Animations[BXDOWN])
                {
                    Velocity = new Vector2(Velocity.X - anim.FrameWidth/2, anim.FrameHeight/2);
                }
                else if (AnimationManager.Animation == Animations[UP] || AnimationManager.Animation == Animations[BXUP])
                {
                    Velocity = new Vector2(Velocity.X - anim.FrameWidth/2, 0);
                }
            }
            else if (Velocity.Y > 0)
            {
                if (IsBox)
                {
                    anim = Animations[BXDOWN];
                }
                else
                {
                    anim = Animations[DOWN];
                }
                    
                if (AnimationManager.Animation == Animations[RIGHT] || AnimationManager.Animation == Animations[BXRIGHT])
                {
                    Velocity = new Vector2(HalfWidth, Velocity.Y);
                }
            }
            else if (Velocity.Y < 0)
            {
                if(IsBox)
                {
                    anim = Animations[BXUP];
                }
                else
                {
                    anim = Animations[UP];
                }

                if (AnimationManager.Animation == Animations[RIGHT] || AnimationManager.Animation == Animations[BXRIGHT])
                {
                    Velocity = new Vector2(HalfWidth, Velocity.Y - anim.FrameHeight/2);
                }
                else if (AnimationManager.Animation == Animations[LEFT] || AnimationManager.Animation == Animations[BXLEFT])
                {
                    Velocity = new Vector2(0, Velocity.Y - anim.FrameHeight/2);
                }
            }

            if (anim != null)
            {
                AnimationManager.Play(anim);
                Width = anim.FrameWidth;
                Height = anim.FrameHeight;
            }
        }

        private void CenterPositionOnGrid()
        {
            //Snap the player to the middle of the grid - H.E.
            Vector2 posAtGrid = GetPositionOnGrid();

            if (Math.Abs(Velocity.Y) > 0)
            {
                int x = ((int)posAtGrid.X * Map.CellWidth) + (Map.CellWidth / 2) - AnimationManager.Animation.FrameWidth/2;
                Position = new Vector2(x, Position.Y);
            }

            if (Math.Abs(Velocity.X) > 0)
            {
                int y = ((int)posAtGrid.Y * Map.CellHeight) + (Map.CellHeight / 2) - AnimationManager.Animation.FrameHeight / 2 + TheLastSliceGame.MapManager.MapStartingYPos;
                Position = new Vector2(Position.X, y);
            }
            
            //Make sure we didn't get snapped into an invalid location
            if(!IsChangingMap && Position.Y > (TheLastSliceGame.Instance.GameHeight - Height / 2))
            {
                RevertPosition();
            }
        }

        public override void PostUpdate()
        {
            CenterPositionOnGrid();

            if (IsBox && !(Math.Abs(Velocity.X) > 0 || Math.Abs(Velocity.Y) > 0))
            {
                AnimationManager.Stop();
            }

            base.PostUpdate();
        }

        public void OnLevelLoaded()
        {
            CanMove = true;
        }
    }
}
