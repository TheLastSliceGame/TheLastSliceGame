using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using TheLastSlice.Managers;
using TheLastSlice.Models;

namespace TheLastSlice.Entities
{
    public class Level : Entity
    {
        public House CurrentDelivery { get; private set; }
        public int TotalNumDeliveries { get; private set; }
        public int CompletedNumDeliveries { get; private set; }
        public List<Pickup> Pickups { get; set; }
        public List<Obstacle> Obstacles { get; set; }
        public Dictionary<Vector3, Entity> Entities { get; set; }
        public bool LevelLoading { get; private set; } //Pssstttt...It's not really loading.
        public SoundEffectInstance SFXInstance { get; private set; }

        private List<House> Deliveries { get; set; }
        private List<Vector2> DeliveryIngredientPositions { get; set; }
        private SoundEffect DeliverySound { get; set; }
        private SoundEffect WrongDeliverySound1 { get; set; }
        private SoundEffect WrongDeliverySound2 { get; set; }
        private SoundEffect FrogDeliverySound1 { get; set; }
        private SoundEffect FrogDeliverySound2 { get; set; }
        private SoundEffect LevelCompleteSound { get; set; }
        private TimeSpan TransitionTimer { get; set; }
        private bool LevelFailed { get; set; }
        private bool DeliveredFrog { get; set; }
        private List<SoundEffect> WrongDeliverySounds { get; set; }
        private List<SoundEffect> LevelLoadingSounds { get; set; }

        public Level(Vector2 position = default(Vector2)) : base(position)
        {
            Animations = new Dictionary<string, Animation>();
            Animations.Add("Mark", new Animation(TheLastSliceGame.Instance.Content.Load<Texture2D>("UI/HouseMarker"), 6)); //Oh hai Mark
            AnimationManager = new AnimationManager(Animations.First().Value);
            Height = (Animations.First().Value.FrameHeight);
            Width = (Animations.First().Value.FrameWidth);

            Pickups = new List<Pickup>();
            Deliveries = new List<House>();
            Obstacles = new List<Obstacle>();
            Entities = new Dictionary<Vector3, Entity>();
            DeliveryIngredientPositions = new List<Vector2>();

            WrongDeliverySounds = new List<SoundEffect>();
            LevelLoadingSounds = new List<SoundEffect>();

            DeliverySound = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/Deliver");

            WrongDeliverySound1 = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/fail1");
            WrongDeliverySound2 = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/fail2");
            FrogDeliverySound1 = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/failfrog1");
            FrogDeliverySound2 = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/failfrog2");

            LevelLoadingSounds.Add(TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/levelstart1"));
            LevelLoadingSounds.Add(TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/levelstart2"));
            LevelLoadingSounds.Add(TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/levelstart3"));

            LevelCompleteSound = TheLastSliceGame.Instance.Content.Load<SoundEffect>("Sounds/LevelComplete");

            TransitionTimer = TimeSpan.Zero;
            LevelFailed = false;
            DeliveredFrog = false;
            LevelLoading = false;
        }

        public void CreateRandomDeliveries()
        {
            int numDeliveres = TheLastSliceGame.Random.Next(1, 3);
            if(numDeliveres > TheLastSliceGame.MapManager.Houses.Count)
            {
                numDeliveres = TheLastSliceGame.Random.Next(1, TheLastSliceGame.MapManager.Houses.Count);
            }

            for (int i = 0; i < numDeliveres;)
            {
                House house = TheLastSliceGame.MapManager.Houses.ElementAt(TheLastSliceGame.Random.Next(1, TheLastSliceGame.MapManager.Houses.Count - 1));
                if (house.DeliveryState == PizzaDeliveryState.None)
                {
                    int numIngredients = TheLastSliceGame.Random.Next(1, 4);
                    List<Ingredient> pizza = new List<Ingredient>();
                    for (int j = 0; j < numIngredients; j++)
                    {
                        IngredientType ingredientType = (IngredientType)TheLastSliceGame.Random.Next(0, Enum.GetNames(typeof(IngredientType)).Length);
                        Ingredient ingredient = new Ingredient(new Vector2(house.Position.X, house.Position.Y), ingredientType.ToString());
                        ingredient.Map = house.Map;
                        ingredient.Hidden = true;
                        pizza.Add(ingredient);
                    }
                   
                    AddDelivery(house, pizza);
                    i++;
                }
            }
        }

        public void CreateRandomPickups()
        {
            foreach (Map map in TheLastSliceGame.MapManager.Maps)
            {
                int numPickups = TheLastSliceGame.Random.Next(2, 4);
                int numIngredients = TheLastSliceGame.Random.Next(6, 12);

                for (int i = 0; i < numIngredients; i++)
                {
                    IngredientType type = (IngredientType)TheLastSliceGame.Random.Next(0, Enum.GetNames(typeof(IngredientType)).Length);
                    
                    Vector2 cell = GetRandomValidCell(map);
                    if (!cell.Equals(Vector2.Zero))
                    {
                        map.AddEntity(EntityType.Ingredient, (int)cell.Y, (int)cell.X, type.ToString());
                    }
                }

                for (int i = 0; i < numPickups; i++)
                {
                    PickupType type = (PickupType)TheLastSliceGame.Random.Next(1, Enum.GetNames(typeof(PickupType)).Length);
                    Vector2 cell = GetRandomValidCell(map);
                    if (!cell.Equals(Vector2.Zero))
                    {
                        map.AddEntity(EntityType.Pickup, (int)cell.Y, (int)cell.X, type.ToString());
                    }
                }
            }
        }

        public Vector2 GetRandomValidCell(Map map)
        {
            Vector2 cell = Vector2.Zero;
            int row = -1;
            int column = -1;
            while (true)
            {
                //Don't spawn anything on the edges of a map
                row = TheLastSliceGame.Random.Next(1, map.NumRows - 1);
                column = TheLastSliceGame.Random.Next(1, map.NumColumns - 1);
                Entity entity = map.GetEntityAtCell(row, column);
                if ((entity != null && (entity.IsBlocking  ||  (entity is Pickup)) || HasDeliveryIngredientOnCell(new Vector2(column, row))))
                {
                    //Can't sit here. Seats taken.
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (row >= 0 && column >= 0)
            {
                cell.X = column;
                cell.Y = row;
            }

            return cell;
        }

        public Vector2 GetRandomValidCell(Map map, int mapNumber)
        {
            return GetRandomValidCell(map, mapNumber, new List<Ingredient>());
        }

        public Vector2 GetRandomValidCell(Map map, int mapNumber, List<Ingredient> currentPizza)
        {
            Vector2 cell = Vector2.Zero;
            int row = -1;
            int column = -1;

            List<Vector2> currentPizzaIngredientLocations = new List<Vector2>();

            foreach(Ingredient ingredient in currentPizza)
            {
                currentPizzaIngredientLocations.Add(ingredient.GetPositionOnGrid());
            }

            while (true)
            {
                //Don't spawn anything on the edges of a map
                row = TheLastSliceGame.Random.Next(1, map.NumRows - 1);
                column = TheLastSliceGame.Random.Next(1, map.NumColumns - 1);
                Entity entity = map.GetEntityAtCell(row, column);
                Vector2 thisCell = new Vector2(column, row);
                if (entity != null &&  
                    (entity.IsBlocking  ||  (entity is Pickup)) || HasDeliveryIngredientOnCell(thisCell) || HasPickupOrObstacleOnCell(mapNumber, thisCell) || currentPizzaIngredientLocations.Contains(thisCell))
                {
                    //Can't sit here. Seats taken.
                    continue;
                }
                else
                {
                    break;
                }
            }

            if (row >= 0 && column >= 0)
            {
                cell.X = column;
                cell.Y = row;
            }

            return cell;
        }

        public bool HasDeliveryIngredientOnCell(Vector2 cell)
        {
            foreach(Vector2 ingredientPos in DeliveryIngredientPositions)
            {
                if(cell.X == ingredientPos.X && cell.Y == ingredientPos.Y)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasPickupOrObstacleOnCell(int map, Vector2 cell)
        {
            if (Entities.ContainsKey(new Vector3(cell.X, cell.Y, map)))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void AddDelivery(House house, List<Ingredient> pizza)
        {
            house.OnPizzaOrdered(pizza);
            Deliveries.Add(house);
            TotalNumDeliveries++;

            foreach (Ingredient ingredient in pizza)
            {
                //Debug.WriteLine(" Ingredient Pos On Grid = {0} ", ingredient.GetPositionOnGrid());
                DeliveryIngredientPositions.Add(ingredient.GetPositionOnGrid());
            }
        }

        public void NextDelivery()
        {
            if(CompletedNumDeliveries == 0)
            {
                LevelLoading = true;
                SFXInstance = LevelLoadingSounds[TheLastSliceGame.LevelManager.CurrentLevelNum-1].CreateInstance();
                SFXInstance.Play();
            }

            if (!IsLevelComplete())
            {
                CurrentDelivery = Deliveries[CompletedNumDeliveries];
                CurrentDelivery.SetAsCurrentDelivery();
                TheLastSliceGame.Instance.HUD.OnNewDelivery();

                //Fill the map with the delivery ingredients - H.E.
                foreach (Ingredient ingredient in CurrentDelivery.Pizza)
                {
                    ingredient.Map.AddEntity(ingredient);
                }
            }
        }

        public void DeliverPizza(House house, List<Ingredient> deliveredPizza)
        {
            if(house != null)
            {
                List<Ingredient> orderedPizza = house.Pizza;

                bool deliveryCompleted = true;

                //Make sure the delivered pizza has the same ingredients as the pizza that was ordered - H.E.
                if(orderedPizza.Count == deliveredPizza.Count)
                {
                    foreach(Ingredient ingredientInOrder in orderedPizza)
                    {
                        bool ingredientFound = false;
                        foreach(Ingredient ingredientInDelivery in deliveredPizza)
                        {
                            if(ingredientInOrder.IngredientType == ingredientInDelivery.IngredientType)
                            {
                                ingredientFound = true;
                                break;
                            }
                        }
                        
                        if(ingredientFound == false)
                        {
                            //The princess is in another castle. Err, the pizza is in another castle? The princess's pizza is in another castle? - H.E.
                            deliveryCompleted = false;
                            break;
                        }
                    }
                }
                else
                {
                    deliveryCompleted = false;
                }

                //Debug.WriteLine("*** Ordered Pizza *** ");
                //foreach(Ingredient ingredientInOrder in orderedPizza)
                //{
                //    Debug.WriteLine(ingredientInOrder.IngredientType.ToString());
                //}

                //Debug.WriteLine("*** Delivered Pizza *** ");
                //foreach (Ingredient ingredientInOrder in deliveredPizza)
                //{
                //    Debug.WriteLine(ingredientInOrder.IngredientType.ToString());
                //}

                if (deliveryCompleted == false)
                {
                    TheLastSliceGame.Instance.Player.OnDeliveryFailed();
                    LevelFailed = true;
                    TheLastSliceGame.Instance.StopMusic();

                    foreach (Ingredient ingredientInDelivery in deliveredPizza)
                    {
                        if(ingredientInDelivery.IsFrog())
                        {
                            DeliveredFrog = true;
                            break;
                        }
                    }

                    int random = TheLastSliceGame.Random.Next(100);
                    if(random < 50)
                    {
                        if(DeliveredFrog == true)
                        {
                            SFXInstance = FrogDeliverySound1.CreateInstance();
                        }
                        else
                        {
                            SFXInstance = WrongDeliverySound1.CreateInstance();
                        }
                    }
                    else
                    {
                        if (DeliveredFrog == true)
                        {
                            SFXInstance = FrogDeliverySound2.CreateInstance();
                        }
                        else
                        {
                            SFXInstance = WrongDeliverySound2.CreateInstance();
                        }
                    }

                    SFXInstance.Play();
                }
                else
                {
                    CompleteDelivery();
                }
            }
        }

        public void CompleteDelivery()
        {
            CurrentDelivery.DeliveryComplete(true);
            CompletedNumDeliveries++;

            if (!IsLevelComplete())
            {
                SFXInstance = DeliverySound.CreateInstance();
                SFXInstance.Play();
                TheLastSliceGame.Instance.Player.OnDeliveryComplete();
                TheLastSliceGame.Instance.HUD.OnDeliveryComplete();
                NextDelivery();
            }
            else
            {
                SFXInstance = LevelCompleteSound.CreateInstance();
                SFXInstance.Play();
                TheLastSliceGame.Instance.Player.OnLevelComplete();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (CurrentDelivery != null && CurrentDelivery.Map == TheLastSliceGame.MapManager.CurrentMap)
            {
                AnimationManager.Draw(spriteBatch);
            }
        }

        public void GeneratePickupsAndObstacles()
        {
            //Fill the map with the pickups and ingredients - H.E.
            foreach (Pickup pickup in Pickups)
            {
                pickup.Map.AddEntity(pickup);
            }

            //Fill the map with the pickups ingredients - H.E.
            foreach (Obstacle obstacle in Obstacles)  
            {
                obstacle.Map.AddEntity(obstacle);
            }
        }

        private bool IsLevelComplete()
        {
            return CompletedNumDeliveries >= Deliveries.Count;
        }

        public void ClearLevel()
        {
            //Place level back to original state
            CompletedNumDeliveries = 0;
            CurrentDelivery = null;
            LevelFailed = false;
            DeliveredFrog = false;
            TransitionTimer = TimeSpan.Zero;
            foreach (House house in Deliveries)
            {
                house.ResetDeliveryState();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (LevelFailed || IsLevelComplete() || LevelLoading)
            {
                if(TransitionTimer == TimeSpan.Zero)
                {
                    TransitionTimer = TimeSpan.FromMilliseconds(gameTime.TotalGameTime.TotalMilliseconds + 2000);
                }
                else if(gameTime.TotalGameTime.TotalMilliseconds > TransitionTimer.TotalMilliseconds)
                {
                    if (LevelFailed)
                    {
                        if(DeliveredFrog)
                        {
                            TheLastSliceGame.Instance.GameOver(GameOverReason.WrongDeliveryFrog);
                        }
                        else
                        {
                            TheLastSliceGame.Instance.GameOver(GameOverReason.WrongDelivery);
                        }
                    }
                    else if(IsLevelComplete())
                    {
                        TheLastSliceGame.LevelManager.OnLevelComplete();
                    }
                    else if(LevelLoading)
                    {
                        LevelLoading = false;
                        TheLastSliceGame.Instance.HUD.OnLevelLoaded();
                        TheLastSliceGame.Instance.Player.OnLevelLoaded();
                    }
                    TransitionTimer = TimeSpan.Zero;
                }
            }

            if (CurrentDelivery != null && CurrentDelivery.Map == TheLastSliceGame.MapManager.CurrentMap)
            {
                AnimationManager.Position = CurrentDelivery.Position;
                AnimationManager.Update(gameTime);
            }
        }
    }
}
