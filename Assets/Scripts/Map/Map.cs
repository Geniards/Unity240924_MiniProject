using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Map : MonoBehaviour
{
    public static Map Instance;

    private Dictionary<Vector2Int, Tile> tiles = new Dictionary<Vector2Int, Tile>();
    public Tile currentFocus;   //���� Ÿ�Ͽ� ���� ������ �����ϴ� �뵵

    [Header("�� ����")]
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private int numColumns = 24;
    [SerializeField] private int numRows = 24;
    [SerializeField] private Character character;

    [Header("���� ����")]
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

        // Ÿ�� ����
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

    // Ÿ���� �����ϴ� �޼���
    private Tile CreateTile(int column, int row, Transform parent)
    {
        Tile tile = Instantiate(tilePrefab, parent);

        // x ��ǥ = row, y ��ǥ = column
        tile.transform.localPosition = new Vector3(column + 0.5f, row + 0.5f, 0);
        tile.name = $"Tile_{column}x{row}";

        TileInfo info = new TileInfo(new Vector2Int(column, row), TileState.EMPTY);
        tile.SetTileInfo(info);

        return tile;
    }

    // ���� ��ġ �޼���
    private void PlaceUnits()
    {
        Debug.Log("PlaceUnits ȣ��");

        // �÷��̾� ���� ��ġ
        foreach (var position in playerStartPositions)
        {
            Debug.Log($"Placing player unit at {position}");
            Tile tile = GetTile(position);
            if (tile != null && tile.tileInfo.tileState == TileState.EMPTY)
            {
                PlayerUnit playerUnit = Instantiate(playerPrefab);
                playerUnit.transform.position = tile.transform.position;
                tile.SetCharacter(playerUnit);
                TurnManager.instance.playerUnits.Add(playerUnit.GetComponent<PlayerUnit>()); // �÷��̾� ���� �߰�
            }
            else
            {
                Debug.Log($"Tile not found or not empty for player at {position}");
            }

        }

        // ���� ���� ��ġ
        foreach (var position in enemyStartPositions)
        {
            Debug.Log($"Placing enemy unit at {position}");
            Tile tile = GetTile(position);
            if (tile != null && tile.tileInfo.tileState == TileState.EMPTY)
            {
                EnemyUnit enemyUnit = Instantiate(enemyPrefab);
                enemyUnit.transform.position = tile.transform.position;
                tile.SetCharacter(enemyUnit);
                TurnManager.instance.enemyUnits.Add(enemyUnit.GetComponent<EnemyUnit>()); // ���� ���� �߰�
            }
            else
            {
                Debug.Log($"Tile not found or not empty for enemy at {position}");
            }
        }
    }

    // ������ Ÿ���� ���� ó��
    public void CheckSelectTile(Tile tile)
    {
        // 1�� Ŭ���� �̵�����Ž��, 2�� Ŭ���� �ش����� �̵�
        if (currentFocus)
        {
            Debug.Log("�ι�°");

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
                Debug.Log("ù��°");

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
