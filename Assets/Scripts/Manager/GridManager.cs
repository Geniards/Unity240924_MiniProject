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
        // 싱글톤 패턴 적용
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

                // 타일 오브젝트를 'Map' 오브젝트의 자식으로 설정
                tileObject.transform.parent = mapParent.transform;
                var tile = tileObject.GetComponent<Tile>();

                tile.Init(new Vector2Int(x, y));
                tiles[x, y] = tile;
            }
        }
    }

    // 모든 타일을 반환하는 메서드
    public List<Tile> GetAllTiles()
    {
        List<Tile> allTiles = new List<Tile>();

        // 그리드에서 모든 타일을 리스트에 추가
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                allTiles.Add(tiles[x, y]);
            }
        }

        return allTiles;
    }

    // 주어진 좌표에 해당하는 타일을 반환하는 메서드 추가
    public Tile GetTileAtPosition(Vector2Int position)
    {
        // 좌표가 그리드 내에 있는지 확인
        if (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height)
        {
            return tiles[position.x, position.y];
        }
        return null;
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
                        // 유닛이 있는 경우, 지나가는 것은 가능하지만 이동은 불가능
                        if (neighbor.hasUnit)
                        {
                            // 유닛이 있는 타일은 이동할 수 없지만 탐지는 가능
                            reachableTiles.Add(neighbor);

                            // 해당 유닛 타일 뒤로 탐지 가능
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

            // 그리드 범위 내에 있는지 확인
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
        if (targetTile.hasUnit)
        {
            Debug.Log("유닛이 있는 타일로는 이동할 수 없습니다.");
            return null; // 유닛이 있는 타일로는 경로를 찾지 않음
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

    // 이동 경로 제한
    public List<Tile> GetLimitedPath(List<Tile> fullPath, int moveRange)
    {
        // 전체 경로에서 이동력 범위 내의 경로만 반환
        List<Tile> limitedPath = new List<Tile>();

        for (int i = 0; i < Mathf.Min(moveRange, fullPath.Count); i++)
        {
            limitedPath.Add(fullPath[i]);
        }

        return limitedPath;
    }

    // 적군이 가장 가까운 아군을 찾는 로직
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

    // 아군 근처에서 이동 가능한 타일을 찾는 메서드
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

        // 상하좌우 먼저 탐색
        foreach (Vector2Int dir in primaryDirections)
        {
            Tile candidateTile = GridManager.Instance.GetTileAtPosition(targetTile.coordinates + dir);
            if (candidateTile != null && !candidateTile.hasUnit && candidateTile.tileState != Tile.TileState.Blocked)
            {
                candidateTiles.Add(candidateTile);
            }
        }

        // 상하좌우에서 이동할 타일을 찾지 못했을 경우 대각선 탐색
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

        // 이동할 수 있는 타일이 있으면 반환
        if (candidateTiles.Count > 0)
        {
            return candidateTiles[0];
        }

        return null;
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

    // 타일에서 유닛 배치 상태 업데이트
    public void UpdateTileUnitStatus(Tile tile, bool hasUnit)
    {
        tile.hasUnit = hasUnit;  // 타일의 유닛 상태 업데이트
    }

    // 이동 가능 범위를 초기화하는 메서드
    public void ClearMoveHighlight()
    {
        foreach (Tile tile in GetAllTiles())
        {
            tile.SetReachable(false);
        }
        Tile.selectedUnit = null;
    }
}
