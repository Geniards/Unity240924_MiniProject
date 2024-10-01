using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width, height;                // 맵의 크기
    public GameObject tilePrefab;            // 타일 프리팹
    public Tile[,] tiles;                    // 타일 배열
    public Vector3 offset;
    private GameObject mapParent;

    private void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        mapParent = new GameObject("Map");

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tileObject = Instantiate(tilePrefab, new Vector3(x + offset.x, y + offset.y, 0 + offset.z), Quaternion.identity);
                tileObject.name = $"Tile {x} {y}";

                // 타일 오브젝트를 'Map' 오브젝트의 자식으로 설정
                tileObject.transform.parent = mapParent.transform;
                var tile = tileObject.GetComponent<Tile>();

                tile.Init(new Vector2Int(x, y));
                tiles[x, y] = tile;
            }
        }
    }

    // BFS를 이용하여 유닛의 이동 가능 범위 탐지
    public List<Tile> FindReachableTiles(Tile startTile, int moveRange)
    {
        Queue<Tile> queue = new Queue<Tile>();
        List<Tile> reachableTiles = new List<Tile>();

        startTile.costFromStart = 0;
        queue.Enqueue(startTile);
        reachableTiles.Add(startTile);

        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();

            foreach (Tile neighbor in GetNeighbors(currentTile))
            {
                if (neighbor.tileState != Tile.TileState.Blocked && !reachableTiles.Contains(neighbor))
                {
                    int newCost = currentTile.costFromStart + 1;

                    if (newCost <= moveRange)
                    {
                        neighbor.costFromStart = newCost;
                        neighbor.parentTile = currentTile;
                        reachableTiles.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        return reachableTiles;
    }

    // 인접한 타일들을 반환하는 메서드
    private List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        Vector2Int[] directions = {
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, -1)
        };

        foreach (Vector2Int direction in directions)
        {
            Vector2Int neighborCoords = tile.coordinates + direction;

            if (neighborCoords.x >= 0 && neighborCoords.x < width &&
                neighborCoords.y >= 0 && neighborCoords.y < height)
            {
                neighbors.Add(tiles[neighborCoords.x, neighborCoords.y]);
            }
        }

        return neighbors;
    }

    // A* 알고리즘을 사용하여 경로를 찾는 메서드
    public List<Tile> FindPath(Tile startTile, Tile targetTile)
    {
        List<Tile> openSet = new List<Tile>();
        HashSet<Tile> closedSet = new HashSet<Tile>();

        startTile.costFromStart = 0;
        openSet.Add(startTile);

        while (openSet.Count > 0)
        {
            Tile currentTile = openSet[0];

            foreach (Tile tile in openSet)
            {
                if (tile.costFromStart < currentTile.costFromStart)
                {
                    currentTile = tile;
                }
            }

            openSet.Remove(currentTile);
            closedSet.Add(currentTile);

            if (currentTile == targetTile)
            {
                return RetracePath(startTile, targetTile);
            }

            foreach (Tile neighbor in GetNeighbors(currentTile))
            {
                if (closedSet.Contains(neighbor) || neighbor.tileState == Tile.TileState.Blocked)
                {
                    continue;
                }

                int newCostToNeighbor = currentTile.costFromStart + 1;
                if (newCostToNeighbor < neighbor.costFromStart || !openSet.Contains(neighbor))
                {
                    neighbor.costFromStart = newCostToNeighbor;
                    neighbor.parentTile = currentTile;

                    if (!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }

        return null;
    }

    // 경로를 추적하여 반환하는 메서드
    private List<Tile> RetracePath(Tile startTile, Tile endTile)
    {
        List<Tile> path = new List<Tile>();
        Tile currentTile = endTile;

        while (currentTile != startTile)
        {
            path.Add(currentTile);
            currentTile = currentTile.parentTile;
        }

        path.Reverse();
        return path;
    }

    // 공격 범위를 탐지하는 메서드
    public List<Tile> FindAttackableTiles(Tile startTile, int attackRange)
    {
        Queue<Tile> queue = new Queue<Tile>();
        List<Tile> attackableTiles = new List<Tile>();

        startTile.costFromStart = 0;
        queue.Enqueue(startTile);
        attackableTiles.Add(startTile);

        while (queue.Count > 0)
        {
            Tile currentTile = queue.Dequeue();

            foreach (Tile neighbor in GetNeighbors(currentTile))
            {
                if (!attackableTiles.Contains(neighbor))
                {
                    int newCost = currentTile.costFromStart + 1;

                    if (newCost <= attackRange)
                    {
                        neighbor.costFromStart = newCost;
                        attackableTiles.Add(neighbor);
                        queue.Enqueue(neighbor);
                        neighbor.UpdateTileState(Tile.TileState.Attackable);
                    }
                }
            }
        }

        return attackableTiles;
    }
}
