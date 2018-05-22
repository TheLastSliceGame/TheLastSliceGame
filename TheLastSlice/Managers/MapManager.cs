using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TheLastSlice.Entities;

namespace TheLastSlice.Managers
{
    //Manages loading the maps via maps.txt file, setting up the map quadrant system, changing and clearing maps.
    public class MapsMananger
    {
        public int MapStartingYPos { get; private set; }
        public List<Map> Maps { get; private set; }
        public Map CurrentMap { get; private set; }
        public List<House> Houses { get; private set; }

        private static string VALUE_TYPE_ROAD = "R";
        private static string VALUE_TYPE_BUILDING = "B";

        public MapsMananger()
        {
            Maps = new List<Map>();
            CurrentMap = null;
            Houses = new List<House>();
            MapStartingYPos = 50;
        }

        public void LoadMaps()
        {

            /*      MAPS
            * _____________
            *|   1  |   2  |
            *|______|______|
            *|   3  |   4  |
            *|______|______|
            *|   5  |   6  |
            *|______|______|

             */

            Stream stream = TitleContainer.OpenStream(@"maps.txt");
            StreamReader reader = new System.IO.StreamReader(stream);

            Map currentMap = null;
            int row = 0, column = 0;

            while (!reader.EndOfStream)
            {
                string line = reader.ReadLine();
                if (line != null && line != string.Empty)
                {
                    if (line.ToLower().Contains("Map".ToLower()))
                    {
                        Map map = new Map(new Vector2(-1, -1));
                        Maps.Add(map);
                        currentMap = map;
                        row = 0;
                        column = 0;
                        //Debug.WriteLine("----------------------------------NEW MAP ----------------------------------");
                    }
                    else if (currentMap != null && currentMap.EntityGrid != null)
                    {
                        foreach (string value in line.Split(','))
                        {
                            string valueTrimed = value.Trim();
                            string valueType = valueTrimed.Split('_')[1];

                            if (valueType == VALUE_TYPE_ROAD)
                            {
                                currentMap.AddEntity(EntityType.Road, row, column, valueTrimed);

                            }
                            else if (valueType == VALUE_TYPE_BUILDING)
                            {
                                currentMap.AddEntity(EntityType.House, row, column, valueTrimed);

                            }
                            else
                            {
                                currentMap.AddEntity(EntityType.None, row, column);

                            }
                            //Debug.WriteLine("Entity Type = {0} at {1},{2} ", value, row, column);
                            column += 1;
                        }
                        column = 0;
                        row += 1;
                    }
                }
            }

            //TODO:: Specifiy adjacent maps through the maps.txt - Hal Emmerich
            Maps[0].MapRight = Maps[1];
            Maps[0].MapDown = Maps[2];

            Maps[1].MapLeft = Maps[0];
            Maps[1].MapDown = Maps[3];

            Maps[2].MapRight = Maps[3];
            Maps[2].MapUp = Maps[0];

            Maps[3].MapLeft = Maps[2];
            Maps[3].MapUp = Maps[1];

            //TODO: Add more maps here or in the Map.cs file? - H.E. 

            //Player will always start in map 1
            CurrentMap = Maps[0];
        }

        public void ChangeMap(Map newMap)
        {
            CurrentMap = newMap;
            TheLastSliceGame.Instance.Player.OnChangeMap();
        }

        public void OnNextLevel()
        {
            foreach (Map map in Maps)
            {
                map.OnNextLevel();
            }
        }

        public void ClearMaps()
        {
            //Clears all pickup and obstacles items from all maps.
            foreach (Map map in Maps)
            {
                foreach (KeyValuePair<Guid, Entity> entity in map.Entities)
                {
                    if (entity.Value is Pickup || entity.Value is Obstacle || entity.Value is Ingredient)
                    {
                        map.AddEntityForRemoval(entity.Value);
                    }
                }

                map.RemoveEntities();
            }
        }
    }
}
