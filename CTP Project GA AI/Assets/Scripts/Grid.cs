using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Grid : MonoBehaviour
{
    public static Grid instance;
    [SerializeField] private GameObject alien, leftSide, rightSide;
    public int gridLength { get; private set; } = 29 ;
    public int gridHeight { get; private set; } = 22;
    private float tileWidth;
    private float tileHeight;
     
    private Vector2 gridArray;
    public List<List<Tile>> gridTiles { get; private set; } = new List<List<Tile>>();

    public class Tile
    {
        public enum TileState
        {
            Empty = 0,
            OccupiedByBullet = 1,
            OccupiedByAlien = 2,
            OccupiedByAlienLaser = 3,
            TargetedByAlien = 4,
            OccupiedByPlayer = 5
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
        public int column = 0;
        public BoundingBox boundingBox;
        public Tile[] surroundingTiles = new Tile[8];

       public struct BoundingBox
        {
            public Vector2 position;
            public Vector2 dimensions; //Width = x, Height = y
            public BoundingBox(Vector2 _position, Vector2 _dimensions)
            {
                position = _position;
                dimensions = _dimensions;
            }

            public bool IsColliding(GameObject _gameobject)
            {
                if (_gameobject.transform.position.x < (position.x + (dimensions.x / 2))
                    && _gameobject.transform.position.x > (position.x - (dimensions.x / 2))
                    && _gameobject.transform.position.y < (position.y + (dimensions.y / 2))
                    && _gameobject.transform.position.y > (position.y - (dimensions.y / 2)))
                {
                    return true;
                }
                else
                    return false;
            }
        }
    }
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        alien = GeneticAlien._instance.alienPrefab;
        tileWidth = alien.transform.lossyScale.x;
        tileHeight = GeneticAlien._instance.GetAlienSizeMax().y;

        //Fix this, it triggers lewis
        Vector3 defaultPosition = (new Vector3(transform.TransformPoint(-rightSide.transform.position).x + 5.1f
            , -4.8f, 0));
        Vector3 offset = defaultPosition;
        int id = 0;

        for (int i = 0; i < gridHeight; i++)
        {
            List<Tile> tempList = new List<Tile>();
            for (int j = 0; j < gridLength; j++)
            {
                
                Tile _tile = new Tile();
                _tile.boundingBox = new Tile.BoundingBox(offset, new Vector2(tileWidth, tileHeight));
                _tile.id = id;
                _tile.column = j;
                tempList.Add(_tile);
                id++;
                offset.x += tileWidth;
            }
            gridTiles.Add(tempList);

            offset.y += tileHeight;
            offset.x = defaultPosition.x;
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

    //Used for alien logic and debugging in scene view.
    public bool GetCollisionWithObject(GameObject _object, ref Tile _occupiedTile, Tile.TileState _targetState)
    {
        foreach (var row in gridTiles)
        {
            foreach (var tile in row)
            {
                if (tile.boundingBox.IsColliding(_object))
                {
                    if (_occupiedTile != null
                        && tile != _occupiedTile)
                        _occupiedTile.currentTileState = Tile.TileState.Empty;

                    _occupiedTile = tile;
                    _occupiedTile.currentTileState = _targetState;
                    return true;
                }
            }
        }
        return false;
    }

    //  Draws coloured cubes on the scene view to help with debugging
    private void OnDrawGizmos()
    {
        for (int i = 0; i < gridTiles.Count; i++)
        {
            for (int j = 0; j < gridTiles[i].Count; j++)
            {
                //Gizmos.DrawCube(gridTiles[i][j].position, gridTiles[i][j].size);
                var offset = (tileWidth * 1.1f);
                var pos = new Vector3(gridTiles[i][j].boundingBox.position.x - offset, 
                    gridTiles[i][j].boundingBox.position.y,
                    0);
                var cubeSize = gridTiles[0][0].boundingBox.dimensions.x;

                Handles.CubeHandleCap(0, pos,
                           Quaternion.LookRotation(Vector3.forward, Vector3.up), cubeSize, EventType.Repaint);
                switch (gridTiles[i][j].currentTileState)
                { 
                    case Tile.TileState.Empty:
                        Handles.color = Color.blue;
                        Handles.Label(pos, "E");
                        break;
                    case Tile.TileState.OccupiedByBullet:
                        Handles.color = Color.yellow;
                        Handles.Label(pos, "OB");
                        break;
                    case Tile.TileState.OccupiedByAlien:
                        Handles.color = Color.red;
                        Handles.Label(pos, "OA");
                        break;
                    case Tile.TileState.OccupiedByAlienLaser:
                        Handles.color = Color.yellow;
                        Handles.Label(pos, "OAL");
                        break;
                    case Tile.TileState.TargetedByAlien:
                        Handles.color = Color.green;
                        Handles.Label(pos, "OA");
                        break;
                    case Tile.TileState.OccupiedByPlayer:
                        Handles.color = Color.green;
                        Handles.Label(pos, "OP");
                        break;
                    default:
                        break;
                }
            }
        }

    }

    public List<Tile> GetColumn(Tile targetTile)
    {
        List<Tile> column = new List<Tile>();

        for (int i = 0; i < gridHeight; i++)
        {
            column.Add(gridTiles[i][targetTile.column]);
        }
            return column;
    }
    //public Vector2 GetWorldPos(Vector2 array)
    //{
    //    return new Vector3((array.x * tileWidth), (array.y * tileHeight));
    //}
}



