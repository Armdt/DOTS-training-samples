﻿using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class BoardCreationSystem : SystemBase
{
    public struct CreationComplete : IComponentData
    {
    }

    protected override void OnUpdate()
    {
        Entities
        .WithNone<CreationComplete>()
        .ForEach((Entity e, ref BoardCreationAuthor boardCreationAuthor) =>
        {
            Random rand = new Random(1);
            for (int x = 0; x < boardCreationAuthor.SizeX; x++)
            {
                for (int y = 0; y < boardCreationAuthor.SizeY; y++)
                {
                    Entity tile = EntityManager.Instantiate(boardCreationAuthor.TilePrefab);
                    Tile newTile = new Tile();
                    PositionXZ tilePos = new PositionXZ();

                    // Create the outer walls & spawn points
                    if (y == 0)
                    {
                        newTile.Value = Tile.Attributes.Up;
                    }
                    else if (y == boardCreationAuthor.SizeY - 1)
                    {
                        newTile.Value = Tile.Attributes.Down;
                    }

                    if (x == 0)
                    {
                        newTile.Value = Tile.Attributes.Left;
                        if (y == 0)
                            newTile.Value = Tile.Attributes.Up | Tile.Attributes.Left | Tile.Attributes.Spawn;
                        else if (y == boardCreationAuthor.SizeY - 1)
                            newTile.Value = Tile.Attributes.Down | Tile.Attributes.Left | Tile.Attributes.Spawn;
                    }
                    else if (x == boardCreationAuthor.SizeX - 1)
                    {
                        newTile.Value = Tile.Attributes.Right;
                        if (y == 0)
                            newTile.Value = Tile.Attributes.Up | Tile.Attributes.Right | Tile.Attributes.Spawn;
                        else if (x == boardCreationAuthor.SizeX - 1 && y == boardCreationAuthor.SizeY - 1)
                            newTile.Value = Tile.Attributes.Down | Tile.Attributes.Right | Tile.Attributes.Spawn;
                    }

                    // Place Random Walls and Holes
                    if (y != 0 || x != 0 || x != boardCreationAuthor.SizeX - 1 || y != boardCreationAuthor.SizeY - 1)
                        if (rand.NextInt(0, 100) < 20)
                        {
                            var result = rand.NextInt(0, 4);
                            switch(result)
                            {
                                case 0:
                                    newTile.Value = Tile.Attributes.Hole;
                                    break;
                                case 1:
                                    newTile.Value = Tile.Attributes.Down;
                                    break;
                                case 2:
                                    newTile.Value = Tile.Attributes.Left;
                                    break;
                                case 3:
                                    newTile.Value = Tile.Attributes.Right;
                                    break;
                                case 4:
                                    newTile.Value = Tile.Attributes.Up;
                                    break;
                            }
                        }

                    // Place Goals

                    if (x == 2)
                    {
                        if(y == 2 || y == boardCreationAuthor.SizeY - 3)
                        {
                            newTile.Value = Tile.Attributes.Goal;
                            tilePos.Value = new float2(x, y+0.5f);
                            Entity goal = EntityManager.Instantiate(boardCreationAuthor.GoalPrefab);
                            EntityManager.AddComponentData(goal, tilePos);
                        }
                    }
                    if (y == 2)
                    {
                        if (x == 2 || x == boardCreationAuthor.SizeX - 3)
                        {
                            newTile.Value = Tile.Attributes.Goal;
                            tilePos.Value = new float2(x, y+0.5f);
                            Entity goal = EntityManager.Instantiate(boardCreationAuthor.GoalPrefab);
                            EntityManager.AddComponentData(goal, tilePos);
                        }
                    }


                    // Set Tile values

                    tilePos.Value = new float2(x, y);
                    EntityManager.AddComponentData(tile, newTile);
                    EntityManager.AddComponentData(tile, tilePos);


                }
            }
            EntityManager.AddComponent<CreationComplete>(e);
        }).WithStructuralChanges().Run();
    }
}
