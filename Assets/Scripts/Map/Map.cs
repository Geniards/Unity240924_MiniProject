using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Map : MonoBehaviour
{
    public static Map Instance;

    private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
    public Tile currentFocus;   //현재 타일에 대한 정보를 저장하는 용도

    [Header("맵 세팅")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private int numColumns = 24;
    [SerializeField] private int numRows = 24;
    [SerializeField] private Character character;

    [Header("유닛 세팅")]
    [SerializeField] private PlayerUnit playerPrefab;
    [SerializeField] private EnemyUnit enemyPrefab;
    [SerializeField] private List<Vector2Int> playerStartPositions;
    [SerializeField] private List<Vector2Int> enemyStartPositions;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        tiles.Clear();

        // 타일 생성
        for (int i = 0; i < numColumns; i++)
        {
            GameObject columnObject = new GameObject($"Column_{i}");
            columnObject.transform.parent = transform;

            for (int j = 0; j < numRows; j++)
            {
                Tile tile = CreateTile(i, j, columnObject.transform);
                tiles[new Vector2Int(i, j)] = tile;
            }
        }

        PlaceUnits();


        //tiles[new Vector2Int(3, 3)].SetCharacter(character);
        //character.transform.position = tiles[new Vector2Int(3, 3)].transform.position;
    }

    public void ClearTile()
    {
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        transform.DetachChildren();
    }

    // 타일을 생성하는 메서드
    private Tile CreateTile(int column, int row, Transform parent)
    {
        Tile tile = Instantiate(tilePrefab, parent);

        // x 좌표 = row, y 좌표 = column
        tile.transform.localPosition = new Vector3(column + 0.5f, row + 0.5f, 0);
        tile.name = $"Tile_{column}x{row}";

        TileInfo info = new TileInfo(new Vector2Int(column, row), TileState.EMPTY);
        tile.SetTileInfo(info);

        return tile;
    }

    // 유닛 배치 메서드
    private void PlaceUnits()
    {
        Debug.Log("PlaceUnits 호출");

        // 플레이어 유닛 배치
        foreach (var position in playerStartPositions)
        {
            Debug.Log($"Placing player unit at {position}");
            Tile tile = GetTile(position);
            if (tile != null && tile.tileInfo.tileState == TileState.EMPTY)
            {
                PlayerUnit playerUnit = Instantiate(playerPrefab);
                playerUnit.transform.position = tile.transform.position;
                tile.SetCharacter(playerUnit);
                TurnManager.instance.playerUnits.Add(playerUnit.GetComponent<PlayerUnit>()); // 플레이어 유닛 추가
            }
            else
            {
                Debug.Log($"Tile not found or not empty for player at {position}");
            }

        }

        // 적군 유닛 배치
        foreach (var position in enemyStartPositions)
        {
            Debug.Log($"Placing enemy unit at {position}");
            Tile tile = GetTile(position);
            if (tile != null && tile.tileInfo.tileState == TileState.EMPTY)
            {
                EnemyUnit enemyUnit = Instantiate(enemyPrefab);
                enemyUnit.transform.position = tile.transform.position;
                tile.SetCharacter(enemyUnit);
                TurnManager.instance.enemyUnits.Add(enemyUnit.GetComponent<EnemyUnit>()); // 적군 유닛 추가
            }
            else
            {
                Debug.Log($"Tile not found or not empty for enemy at {position}");
            }
        }
    }

    // 선택한 타일의 로직 처리
    public void CheckSelectTile(Tile tile)
    {
        // 1번 클릭시 이동방향탐지, 2번 클릭시 해당지점 이동
        if (currentFocus)
        {
            Debug.Log("두번째");

            if (currentFocus.character.CheckMove(tile))
            {
                currentFocus = null;
                return;
            }
        }
        else
        {
            if (tile.hasCharcter)
            {
                Debug.Log("첫번째");

                currentFocus = tile;
                tile.character.ShowMovementRange(tile);
            }
        }
    }

    public void ClearSearch()
    {
        foreach (Tile v in tiles.Values)
            v.ClearSearch();
    }

    public Tile GetTile(Vector2Int coord)
    {
        return tiles.ContainsKey(coord) ? tiles[coord] : null;
    }

    public void SwapReference(ref Queue<Tile> a, ref Queue<Tile> b)
    {
        Queue<Tile> temp = a;
        a = b;
        b = temp;
    }

    public int GetDistance(Tile a, Tile b)
    {
        return Mathf.Abs(a.tileInfo.coord.x - b.tileInfo.coord.x) + Mathf.Abs(a.tileInfo.coord.y - b.tileInfo.coord.y)
    }

    public bool isNearTarget(Tile current, Tile target)
    {
        return GetDistance(current, target) == 1;
    }

}
