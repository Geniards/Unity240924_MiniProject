using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width, height;                
    public GameObject tilePrefab;            
    public Tile[,] tiles;                    
    public Vector3 offset;
    public Tilemap tilemap;                  
    public UnitPlacementManager unitPlacementManager;
    private GameObject mapParent;

    private void Awake()
    {
        // �̱��� ���� ����
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        GenerateGrid();

        if (unitPlacementManager != null)
        {
            unitPlacementManager.PlaceUnitsOnGrid();
        }
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

    // ��� Ÿ���� ��ȯ�ϴ� �޼���
    public List<Tile> GetAllTiles()
    {
        List<Tile> allTiles = new List<Tile>();

        // �׸��忡�� ��� Ÿ���� ����Ʈ�� �߰�
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                allTiles.Add(tiles[x, y]);
            }
        }

        return allTiles;
    }

    // �־��� ��ǥ�� �ش��ϴ� Ÿ���� ��ȯ�ϴ� �޼��� �߰�
    public Tile GetTileAtPosition(Vector2Int position)
    {
        // ��ǥ�� �׸��� ���� �ִ��� Ȯ��
        if (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height)
        {
            return tiles[position.x, position.y];
        }
        return null;
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
                        // ������ �ִ� ���, �������� ���� ���������� �̵��� �Ұ���
                        if (neighbor.hasUnit)
                        {
                            // ������ �ִ� Ÿ���� �̵��� �� ������ Ž���� ����
                            reachableTiles.Add(neighbor);

                            // �ش� ���� Ÿ�� �ڷ� Ž�� ����
                            continue;
                        }

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

            // �׸��� ���� ���� �ִ��� Ȯ��
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
        if (targetTile.hasUnit)
        {
            Debug.Log("������ �ִ� Ÿ�Ϸδ� �̵��� �� �����ϴ�.");
            return null; // ������ �ִ� Ÿ�Ϸδ� ��θ� ã�� ����
        }

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
                    openSet.Add(neighbor);
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

    // �̵� ��� ����
    public List<Tile> GetLimitedPath(List<Tile> fullPath, int moveRange)
    {
        // ��ü ��ο��� �̵��� ���� ���� ��θ� ��ȯ
        List<Tile> limitedPath = new List<Tile>();

        for (int i = 0; i < Mathf.Min(moveRange, fullPath.Count); i++)
        {
            limitedPath.Add(fullPath[i]);
        }

        return limitedPath;
    }

    // ������ ���� ����� �Ʊ��� ã�� ����
    public Unit FindClosestAlly(Unit enemy)
    {
        Unit closestAlly = null;
        float closestDistance = float.MaxValue;

        foreach (Unit ally in TurnManager.Instance.allyUnits)
        {
            float distance = Vector2.Distance(enemy.currentTile.coordinates, ally.currentTile.coordinates);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestAlly = ally;
            }
        }

        return closestAlly;
    }

    // �Ʊ� ��ó���� �̵� ������ Ÿ���� ã�� �޼���
    public Tile FindValidTileNearTarget(Unit target)
    {
        Tile targetTile = target.currentTile;
        List<Tile> candidateTiles = new List<Tile>();

        Vector2Int[] primaryDirections =
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0),
        };

        Vector2Int[] secondaryDirections =
        {
            new Vector2Int(1, 1),
            new Vector2Int(1, -1),
            new Vector2Int(-1, 1),
            new Vector2Int(-1, -1),
        };

        // �����¿� ���� Ž��
        foreach (Vector2Int dir in primaryDirections)
        {
            Tile candidateTile = GridManager.Instance.GetTileAtPosition(targetTile.coordinates + dir);
            if (candidateTile != null && !candidateTile.hasUnit && candidateTile.tileState != Tile.TileState.Blocked)
            {
                candidateTiles.Add(candidateTile);
            }
        }

        // �����¿쿡�� �̵��� Ÿ���� ã�� ������ ��� �밢�� Ž��
        if (candidateTiles.Count == 0)
        {
            foreach (Vector2Int dir in secondaryDirections)
            {
                Tile candidateTile = GridManager.Instance.GetTileAtPosition(targetTile.coordinates + dir);
                if (candidateTile != null && !candidateTile.hasUnit && candidateTile.tileState != Tile.TileState.Blocked)
                {
                    candidateTiles.Add(candidateTile);
                }
            }
        }

        // �̵��� �� �ִ� Ÿ���� ������ ��ȯ
        if (candidateTiles.Count > 0)
        {
            return candidateTiles[0];
        }

        return null;
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

    // Ÿ�Ͽ��� ���� ��ġ ���� ������Ʈ
    public void UpdateTileUnitStatus(Tile tile, bool hasUnit)
    {
        tile.hasUnit = hasUnit;  // Ÿ���� ���� ���� ������Ʈ
    }

    // �̵� ���� ������ �ʱ�ȭ�ϴ� �޼���
    public void ClearMoveHighlight()
    {
        foreach (Tile tile in GetAllTiles())
        {
            tile.SetReachable(false);
        }
        Tile.selectedUnit = null;
    }
}
