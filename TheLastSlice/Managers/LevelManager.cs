using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using TheLastSlice.Entities;
using TheLastSlice.UI;

namespace TheLastSlice.Managers
{
    //Manages loading the levels via levels.txt, changing levels, etc...
    public class LevelManager
    {
        public List<Level> Levels { get; private set; }
        public Level CurrentLevel { get; private set; }
        public int CurrentLevelNum { get { return NumLevelsCompleted + 1; } }

        private int NumLevelsCompleted { get; set; }
        private DateTime LevelStartTime { get; set; }

        public LevelManager()
        {
            NumLevelsCompleted = 0;
            CurrentLevel = null;
            Levels = new List<Level>();

        }

        public void NextLevel()
        {
            //Finish the fight
            if (Levels.Count > 0)
            {
                CurrentLevel = Levels[NumLevelsCompleted];
                
                TheLastSliceGame.MapManager.OnNextLevel();
                TheLastSliceGame.Instance.Player.LoadTexture();
                TheLastSliceGame.Instance.Player.Reset();

                CurrentLevel.NextDelivery();
                CurrentLevel.GeneratePickupsAndObstacles();
                LevelStartTime = DateTime.Now;
                //CurrentLevel.CreateRandomPickups();
            }
        }

        public void OnLevelComplete()
        {
            //Remove current pickups/obstacles off current level
            TheLastSliceGame.MapManager.ClearMaps();
            NumLevelsCompleted++;
            TheLastSliceGame.Instance.AppInsights.LevelComplete(NumLevelsCompleted, LevelStartTime);
            TheLastSliceGame.Instance.ChangeState(GameState.Menu, UIType.LevelTransition);
        }

        public void OnGameOver()
        {
            TheLastSliceGame.MapManager.ClearMaps();
            NumLevelsCompleted = 0;
            CurrentLevel = null;

            foreach(Level level in Levels)
            {
                level.ClearLevel();
            }
        }

        public void LoadLevels()
        {
            Stream stream = TitleContainer.OpenStream(@"levels.txt");
            StreamReader reader = new System.IO.StreamReader(stream);

            Level currentLevel = null;
            Map currentMap = null;
            House currentDeliveryHouse = null;
            List<Ingredient> currentPizza = null;
            bool readInPickUps = false;
            bool readInObstacles = false;
            bool readInRandoms = false;
            int numDeliveries = 0;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line != null && line != string.Empty)
                {
                    if (line.ToLower().Contains("LEVEL".ToLower()))
                    {
                        if (currentLevel != null && currentDeliveryHouse != null && currentPizza != null)
                        {
                            currentLevel.AddDelivery(currentDeliveryHouse, currentPizza);
                        }

                        Level level = new Level(new Vector2(-1, -1));
                        Levels.Add(level);
                        currentLevel = level;
                        currentMap = null;
                        currentDeliveryHouse = null;
                        currentPizza = null;
                        readInPickUps = false;
                        readInObstacles = false;
                        readInRandoms = false;
                        //Debug.WriteLine("----------------------------------NEW Level ----------------------------------");
                    }
                    else if (currentLevel != null && line.ToLower().Contains("DH".ToLower()))
                    {
                        numDeliveries = numDeliveries++;
                        string[] delivery = line.Split(',');
                        currentMap = TheLastSliceGame.MapManager.Maps[Convert.ToInt32(delivery[1])];
                        int houseRow = Convert.ToInt32(delivery[2]);
                        int houseColumn = Convert.ToInt32(delivery[3]);

                        currentDeliveryHouse = currentMap.EntityGrid[houseRow, houseColumn] as House;
                        currentPizza = new List<Ingredient>();
                        if (currentDeliveryHouse == null)
                        {
                            Debug.WriteLine("!!!! The delivery house row/column is incorrect! !!!!");
                        }
                        //Debug.WriteLine("----------------------------------NEW House ----------------------------------");
                    }
                    else if (currentLevel != null && line.ToLower().Contains("PICKUPS".ToLower()))
                    {
                        if (currentDeliveryHouse != null && currentPizza != null)
                        {
                            currentLevel.AddDelivery(currentDeliveryHouse, currentPizza);
                            currentDeliveryHouse = null;
                            currentPizza = null;
                            currentMap = null;
                        }

                        readInPickUps = true;
                        //Debug.WriteLine("----------------------------------NEW Pickups ----------------------------------");
                    }
                    else if (currentLevel != null && line.ToLower().Contains("OBSTACLES".ToLower()))
                    {
                        if (currentDeliveryHouse != null && currentPizza != null)
                        {
                            currentLevel.AddDelivery(currentDeliveryHouse, currentPizza);
                            currentDeliveryHouse = null;
                            currentPizza = null;
                            currentMap = null;
                        }

                        readInObstacles = true;
                        //Debug.WriteLine("----------------------------------NEW Pickups ----------------------------------");
                    }
                    else if (currentLevel != null && line.ToLower().Contains("RANDOMS".ToLower()))
                    {
                        if (currentDeliveryHouse != null && currentPizza != null)
                        {
                            currentLevel.AddDelivery(currentDeliveryHouse, currentPizza);
                            currentDeliveryHouse = null;
                            currentPizza = null;
                            currentMap = null;
                        }

                        readInRandoms = true;
                        //Debug.WriteLine("----------------------------------NEW Pickups ----------------------------------");
                    }
                    else if (readInPickUps == true && currentLevel != null)
                    {
                        string[] pickupString = line.Split(',');
                        string assetCode = pickupString[0];
                        Map map = TheLastSliceGame.MapManager.Maps[Convert.ToInt32(pickupString[1])];
                        int row = Convert.ToInt32(pickupString[2]);
                        int column = Convert.ToInt32(pickupString[3]);
                        int xPos = (int)map.EntityGrid[row, column].Position.X;
                        int yPos = (int)map.EntityGrid[row, column].Position.Y;

                        bool isHeart = Pickup.isPickupType(PickupType.HE, assetCode);
                        bool isBox = Pickup.isPickupType(PickupType.BX, assetCode);
                        bool isCube = Pickup.isPickupType(PickupType.CU, assetCode);
                        bool isGas = Pickup.isPickupType(PickupType.GS, assetCode);
                        bool isTrash = Pickup.isPickupType(PickupType.TC, assetCode);

                        if (isHeart || isBox || isCube || isGas || isTrash)
                        {
                            Pickup pickup = new Pickup(new Vector2(xPos, yPos), assetCode);
                            pickup.Map = map;
                            currentLevel.Pickups.Add(pickup);
                        }
                        else
                        {
                            Ingredient ingredient = new Ingredient(new Vector2(xPos, yPos), assetCode);
                            ingredient.Map = map;
                            currentLevel.Pickups.Add(ingredient);
                        }
                    }
                    else if (readInObstacles == true && currentLevel != null)
                    {
                        string[] obstacleString = line.Split(',');
                        string assetCode = obstacleString[0];
                        int mapNumber = Convert.ToInt32(obstacleString[1]);
                        Map map = TheLastSliceGame.MapManager.Maps[mapNumber];
                        int row = Convert.ToInt32(obstacleString[2]);
                        int column = Convert.ToInt32(obstacleString[3]);
                        int xPos = (int)map.EntityGrid[row, column].Position.X;
                        int yPos = (int)map.EntityGrid[row, column].Position.Y;
                        Obstacle obstacle = new Obstacle(new Vector2(xPos, yPos), assetCode);
                        obstacle.Map = map;
                        if (assetCode == "PB")
                        {
                            obstacle.PBTravelDistanceX = Convert.ToInt32(obstacleString[4])*TheLastSliceGame.Instance.EntityWidth;
                        }
                        currentLevel.Obstacles.Add(obstacle);
                        currentLevel.Entities.Add(new Vector3(column, row, mapNumber), obstacle);
                    }
                    else if (readInRandoms == true && currentLevel != null)
                    {
                        string[] pickupString = line.Split(',');
                        string assetCode = pickupString[0];
                        int mapNumber = Convert.ToInt32(pickupString[1]);
                        Map map = TheLastSliceGame.MapManager.Maps[mapNumber];
                        Vector2 location = currentLevel.GetRandomValidCell(map, mapNumber);
                        int row = Convert.ToInt32(location.Y);
                        int column = Convert.ToInt32(location.X);
                        int xPos = (int)map.EntityGrid[row, column].Position.X;
                        int yPos = (int)map.EntityGrid[row, column].Position.Y;

                        bool isHeart = Pickup.isPickupType(PickupType.HE, assetCode);
                        bool isBox = Pickup.isPickupType(PickupType.BX, assetCode);
                        bool isCube = Pickup.isPickupType(PickupType.CU, assetCode);
                        bool isGas = Pickup.isPickupType(PickupType.GS, assetCode);
                        bool isTrash = Pickup.isPickupType(PickupType.TC, assetCode);

                        if (isHeart || isBox || isCube || isGas || isTrash)
                        {
                            Pickup pickup = new Pickup(new Vector2(xPos, yPos), assetCode);
                            pickup.Map = map;
                            currentLevel.Pickups.Add(pickup);
                            currentLevel.Entities.Add(new Vector3(column, row, mapNumber), pickup);
                        }
                        else
                        {
                            //Ingredients/Toppings
                            Ingredient ingredient = new Ingredient(new Vector2(xPos, yPos), assetCode);
                            ingredient.Map = map;
                            currentLevel.Pickups.Add(ingredient);
                            currentLevel.Entities.Add(new Vector3(column, row, mapNumber), ingredient);
                        }
                    }
                    else if (currentMap != null && currentLevel != null && currentDeliveryHouse != null)
                    {
                        string[] ingredientString = line.Split(',');
                        string assetCode = ingredientString[0];
                        int mapNumber = Convert.ToInt32(ingredientString[1]);
                        Map map = TheLastSliceGame.MapManager.Maps[mapNumber];
                        Vector2 ingredientLocation = currentLevel.GetRandomValidCell(map, mapNumber, currentPizza);
                        int row = Convert.ToInt32(ingredientLocation.Y);
                        int column = Convert.ToInt32(ingredientLocation.X);
                        int xPos = (int)map.EntityGrid[row, column].Position.X;
                        int yPos = (int)map.EntityGrid[row, column].Position.Y;
                        Ingredient ingredient = new Ingredient(new Vector2(xPos, yPos), assetCode);
                        ingredient.Map = map;
                        currentPizza.Add(ingredient);
                    }
                }
                else if (currentLevel != null && currentDeliveryHouse != null && currentPizza != null)
                {
                    currentLevel.AddDelivery(currentDeliveryHouse, currentPizza);
                    currentDeliveryHouse = null;
                }
                else
                {
                    readInPickUps = false;
                    readInObstacles = false;
                    readInRandoms = false;
                }
            }

            //Case where the delivery is the last thing in the txt file.
            if (currentLevel != null && currentDeliveryHouse != null && currentPizza != null)
            {
                currentLevel.AddDelivery(currentDeliveryHouse, currentPizza);
                currentDeliveryHouse = null;
            }
        }
    }
}
