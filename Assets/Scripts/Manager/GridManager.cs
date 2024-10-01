using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width, height;                // ���� ũ��
    public GameObject tilePrefab;            // Ÿ�� ������
    public Tile[,] tiles;                    // Ÿ�� �迭
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

                // Ÿ�� ������Ʈ�� 'Map' ������Ʈ�� �ڽ����� ����
                tileObject.transform.parent = mapParent.transform;
                var tile = tileObject.GetComponent<Tile>();

                tile.Init(new Vector2Int(x, y));
                tiles[x, y] = tile;
            }
        }
    }

    // BFS�� �̿��Ͽ� ������ �̵� ���� ���� Ž��
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

    // ������ Ÿ�ϵ��� ��ȯ�ϴ� �޼���
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

    // A* �˰����� ����Ͽ� ��θ� ã�� �޼���
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

    // ��θ� �����Ͽ� ��ȯ�ϴ� �޼���
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

    // ���� ������ Ž���ϴ� �޼���
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
