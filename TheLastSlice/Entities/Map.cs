using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TheLastSlice.Entities
{
    public class Map : Entity
    {
        public static int CellWidth = 50;
        public static int CellHeight = 50;

        public Map MapLeft { get; set; }   //The map to the left of this map
        public Map MapRight { get; set; }  //The map to the right of this map
        public Map MapDown { get; set; }   //The map below/down to this map
        public Map MapUp { get; set; }     //The map above/up to this map

        public Dictionary<Guid, Entity> Entities { get; private set; }
        public Entity[,] EntityGrid { get; private set; } //entity grids are row/column but entity positions will be defined by x,y (Vector2)
        public Dictionary<Guid, Entity> BackgroundEntities { get; private set; }
        public Dictionary<Guid, Entity> ForegroundEntities { get; private set; }

        public int NumRows { get; private set; }
        public int NumColumns { get; private set; }

        private List<Entity> EntitiesToRemove;
        private List<Pawn> MovedPawns;
        private List<Rectangle> Collisions;

        public Map(Vector2 position = default(Vector2), String assetCode = null) : base(position, assetCode)
        {
            Entities = new Dictionary<Guid, Entity>();
            EntitiesToRemove = new List<Entity>();
            MovedPawns = new List<Pawn>();
            BackgroundEntities = new Dictionary<Guid, Entity>();
            ForegroundEntities = new Dictionary<Guid, Entity>();
            IsBlocking = false;
            NumRows = TheLastSliceGame.Instance.GameHeight / TheLastSliceGame.Instance.EntityHeight - 1;
            NumColumns = TheLastSliceGame.Instance.GameWidth / TheLastSliceGame.Instance.EntityWidth;
            EntityGrid = new Entity[NumRows, NumColumns];
            Collisions = new List<Rectangle>();
        }

        public Entity GetEntityAtCell(int row, int column)
        {
            if(row < 0 || column < 0 || row > (NumRows-1) || column > (NumColumns-1))
            {
                return null;
            }

            return EntityGrid[row, column];
        }

        public void AddEntity(Entity entity)
        {
            entity.Map = this;
            Entities.Add(entity.Guid, entity);
            if (entity.Type == EntityType.Car || entity.Type == EntityType.Pickup || entity.Type == EntityType.Ingredient || entity.Type == EntityType.Obstacle)
            {
                ForegroundEntities.Add(entity.Guid, entity);
            }
            else
            {
                BackgroundEntities.Add(entity.Guid, entity);
            }

            Vector2 gridPos = entity.GetPositionOnGrid();
            Entity tempEntity = EntityGrid[(int)gridPos.Y, (int)gridPos.X];
            if(tempEntity != null && (tempEntity.IsBlocking || tempEntity is Pickup))
            {
                Debug.WriteLine(" *** MAP Entity Collision! - Entity Already exists at Column:{1},  Row:{0} {2}", (int)gridPos.Y, (int)gridPos.X, tempEntity.Type.ToString());
            }
            EntityGrid[(int)gridPos.Y, (int)gridPos.X] = entity;

            entity.OnAddedToMap();
        }

        public void AddEntity(EntityType type, int row, int column, string assetCode = null)
        {
            Vector2 pos = new Vector2(column * TheLastSliceGame.Instance.EntityWidth, TheLastSliceGame.MapManager.MapStartingYPos + (row * TheLastSliceGame.Instance.EntityHeight));
            switch (type)
            {
                case EntityType.Road:
                {
                    Road road = new Road(pos, assetCode);
                    AddEntity(road);
                    break;
                }
                case EntityType.House:
                {
                    House house = new House(pos, assetCode);
                    house.Map = this;
                    TheLastSliceGame.MapManager.Houses.Add(house);
                    AddEntity(house);
                    break;
                }
                case EntityType.Car:
                {
                    //TODO:: Are we going to even use cars? - H.E.
                    Car car = new Car(pos);
                    AddEntity(car);
                    break;
                }
                case EntityType.Ingredient:
                {     
                    Ingredient ingredient = new Ingredient(pos, assetCode);
                    //Debug.WriteLine(" Ingredient {0} - Pos On Grid: {1}, {2}", ingredient.IngredientType, column, row);
                    AddEntity(ingredient);
                    break;
                }
                case EntityType.Pickup:
                {
                    Pickup pickup = new Pickup(pos, assetCode);
                    //Debug.WriteLine(" Pickup {0} - Pos On Grid: {1}, {2}", pickup.PickupType, column, row);

                    AddEntity(pickup);
                    break;
                }
                case EntityType.Obstacle:
                {
                    Obstacle obstacle = new Obstacle(pos, assetCode);
                    AddEntity(obstacle);
                    break;
                }
                case EntityType.None:
                default:
                {
                    //Defaults to grass ¯\_(ツ)_/¯
                    Entity grass = new Entity(pos, "GA0");
                    AddEntity(grass);
                    break;
                }
            }
        }

        public override void Update(GameTime time)
        {
            //Update each entity in the map (except the Player(s))
            foreach (KeyValuePair<Guid, Entity> entity in Entities)
            {
                entity.Value.Update(time);
            }

            base.Update(time);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            //Each map is in charge of drawing it's own entities
            foreach (KeyValuePair<Guid, Entity> backgroundEntity in BackgroundEntities)
            {
                backgroundEntity.Value.Draw(spriteBatch);
            }

            foreach (KeyValuePair<Guid, Entity> foregroundEntity in ForegroundEntities)
            {
                foregroundEntity.Value.Draw(spriteBatch);
            }

            int totalCollisionWidth = 0;
            int totalCollisionHeight = 0;
            foreach(Rectangle rect in Collisions)
            {
                TheLastSliceGame.Instance.DrawDebugBorder(rect, 2, Color.LimeGreen);
                totalCollisionWidth += rect.Width;
                totalCollisionHeight += rect.Height;
            }
        }

        public void ResolveCollisions()
        {
            //TODO:: Use a quad tree for collisions instead of list of moved entities if we have time - H.E.

            Collisions.Clear();
            Rectangle overlap = new Rectangle();
            foreach (Pawn movedPawn in MovedPawns)
            {
                foreach (KeyValuePair<Guid, Entity> entityToTest in Entities)
                {
                    if (movedPawn != entityToTest.Value && entityToTest.Value.CollisionComponent != Rectangle.Empty)
                    {
                        if (movedPawn.IsColliding(entityToTest.Value, out overlap))
                        {
                            entityToTest.Value.OnCollided(movedPawn);
                            movedPawn.OnCollided(entityToTest.Value);   
                        }
                    }
                }
            }
        }

        public bool IsCollidingNearCell(Rectangle collision, int row, int column)
        {
            //TODO:: Search only 3 cells instead of 9 if we're only using this for map transitions???

            /* Searches nearby grid locations for entity collisions
            * ___________________
            *|      |      |     |
            *|______|______|_____|
            *|      |  E   |     |
            *|______|______|_____|
            *|      |      |     |
            *|______|______|_____|
            */

            int startingRow = row - 1;
            int startingColumn = column - 1;

            for (int i = startingRow; i <= startingRow + 2; i++)
            {
                if (i >= 0 && i < NumRows)
                {
                    for (int j = startingColumn; j <= startingColumn + 2; j++)
                    {
                        if (j >= 0 && j < NumColumns)
                        {
                            Entity collidingEntity = GetEntityAtCell(i, j);
                            if (collidingEntity != null && collidingEntity.IsBlocking && collision.Intersects(collidingEntity.CollisionComponent))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        public void AddEntityForRemoval(Entity entity)
        {
            EntitiesToRemove.Add(entity);
        }

        public void AddMovedPawn(Pawn pawn)
        {
            MovedPawns.Add(pawn);
        }

        public override void PostUpdate()
        {
            RemoveEntities();
        }

        public void RemoveEntities()
        {
            foreach (Entity entity in EntitiesToRemove)
            {
                Vector2 gridPos = entity.GetPositionOnGrid();
                EntityGrid[(int)gridPos.Y, (int)gridPos.X] = null;

                Entities.Remove(entity.Guid);
                if (BackgroundEntities.ContainsKey(entity.Guid))
                {
                    BackgroundEntities.Remove(entity.Guid);
                }

                if (ForegroundEntities.ContainsKey(entity.Guid))
                {
                    ForegroundEntities.Remove(entity.Guid);
                }
            }

            EntitiesToRemove.Clear();
            MovedPawns.Clear();
        }

        public void OnNextLevel()
        {
            foreach (KeyValuePair<Guid, Entity> entity in Entities)
            {
                entity.Value.LoadTexture();
            }
        }
    }
}
/* RASPI Map
 * ____________________ ____________________ ____________________ ____________________ ____________________ ______
 *|START |      |      |      |      |  GA  |      | OL   |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|      |  CH  |      |  JP  |      |      |      | OL   |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|      |      |      |      |      |      |      |      |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|      |      |      |      |      |      |      |      |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|      |      |      |  BA  |  BA  | BA   | BA   | BA   |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 *|  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |  ??  |
 *|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|______|
 */
