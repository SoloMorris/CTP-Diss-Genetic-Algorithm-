using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject alien, leftSide, rightSide;
    int gridLength;
    int gridHeight;
    private float tileWidth;
    private float tileHeight;
     
    private Vector2 gridArray;
    private List<List<Tile>> gridTiles = new List<List<Tile>>();
    public class Tile
    {
        public enum TileState
        {
            Empty = 0,
            OccupiedByBullet = 1,
            OccupiedByAlien = 2,
            OccupiedByAlienLaser = 3,
            TargetedByAlien = 4
        }

        public TileState currentTileState = TileState.Empty;
        public enum TileDir
        {

            BottomLeft = 0,
            Bottom = 1,
            BottomRight = 2,
            Left = 3,
            Right = 4,
            TopLeft = 5,
            Top = 6,
            TopRight = 7
            
        }

        public int id;
        public Vector2 size;
        public Vector2 position;
        public Tile[] surroundingTiles = new Tile[8];
    }

    void Start()
    {
        alien = GeneticAlien._instance.alienPrefab;
        tileWidth = alien.transform.lossyScale.x;
        tileHeight = GeneticAlien._instance.GetAlienSizeMax().y;

        //Fix this, it triggers lewis
        Vector3 offset = new Vector3(-8,-4.5f,0);
        int id = 0;
        for (int i = 0; i < 21; i++)
        {
            List<Tile> tempList = new List<Tile>();
            for (int j = 0; j < 42; j++)
            {
                
                Tile _tile = new Tile();
                _tile.size = new Vector2(tileWidth, tileHeight);
                _tile.position = offset;
                _tile.id = id;
                tempList.Add(_tile);
                print(offset);
                id++;
                offset.x += tileWidth;
            }
            gridTiles.Add(tempList);

            offset.y += tileHeight;
            offset.x = -8.0f;
        }

        //Initialise tile checking
        for (int i = 0; i < gridTiles.Count; i++)
        {
            for (int j = 0; j < gridTiles[i].Count; j++)
            {
                //Bottom tiles
                if (i != 0)
                {
                    if (j != 0)
                        gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.BottomLeft] = gridTiles[i - 1][j - 1];

                    if (j < gridTiles[i].Count - 1)
                        gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.BottomRight] = gridTiles[i - 1][j + 1];

                    gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.Bottom] = gridTiles[i - 1][j];

                }
                //Top tiles
                if (i  < gridTiles.Count - 1)
                {
                    if (j != 0)
                        gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.TopLeft] = gridTiles[i + 1][j - 1];

                    if (j < gridTiles[i].Count - 1)
                        gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.TopRight] = gridTiles[i + 1][j + 1];

                    gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.Top] = gridTiles[i + 1][j];

                }
                //Middle Tiles
                if (j != 0)
                    gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.Left] = gridTiles[i][j - 1];

                if (j < gridTiles[i].Count - 1)
                    gridTiles[i][j].surroundingTiles[(int)Tile.TileDir.Right] = gridTiles[i][j + 1];

            }
        }

    }
    //AABB collision for tile checking, add min-max point
    //TODO ID checking for alien logic

    private void OnDrawGizmos()
    {
        for (int i = 0; i < gridTiles.Count; i++)
        {
            for (int j = 0; j < gridTiles[i].Count; j++)
            {
                Gizmos.DrawCube(gridTiles[i][j].position, gridTiles[i][j].size);
            }
        }

    }
    //public Vector2 GetWorldPos(Vector2 array)
    //{
    //    return new Vector3((array.x * tileWidth), (array.y * tileHeight));
    //}
}



