using System.IO;
using UnityEngine;

public class MapEditorManager : MonoBehaviour
{
    public TileMapData mapData;        // ScriptableObject�� �� �����͸� ����
    public GameObject tilePrefab;      // Ÿ�� ������
    public string csvFileName = "map.csv";   // CSV ���� �̸�

    private Tile[,] tiles;             // ���� ������ Ÿ���� ������ �迭

    private int width;
    private int height;

    private void Start()
    {
        // ScriptableObject�κ��� �⺻ ũ�� ����
        width = mapData.width;
        height = mapData.height;
        GenerateGridFromScriptableObject();
    }

    // ���� ScriptableObject�κ��� �ҷ��ͼ� �׸��� ����
    public void GenerateGridFromScriptableObject()
    {
        ClearGrid();

        tiles = new Tile[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                tileObject.name = $"Tile ({x}, {y})";
                Tile tile = tileObject.GetComponent<Tile>();
                tile.Init(new Vector2Int(x, y));
                tiles[x, y] = tile;
            }
        }

        // ScriptableObject�κ��� Ÿ�� ������ �ε�
        mapData.LoadTileData(tiles);
    }

    // ���� CSV�κ��� �ҷ��ͼ� �׸��� ����
    public void GenerateGridFromCSV()
    {
        ClearGrid();

        // Resources ���� ������ CSV ������ �ε�
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�: " + csvFileName);
            return;
        }

        string[] lines = csvFile.text.Split('\n');
        height = lines.Length;
        width = lines[0].Split(',').Length;
        tiles = new Tile[width, height];

        for (int y = 0; y < height; y++)
        {
            string[] values = lines[y].Split(',');
            for (int x = 0; x < width; x++)
            {
                GameObject tileObject = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity);
                tileObject.name = $"Tile ({x}, {y})";
                Tile tile = tileObject.GetComponent<Tile>();
                tile.Init(new Vector2Int(x, y));
                tiles[x, y] = tile;

                int tileType = int.Parse(values[x]);
                if (tileType == 1)
                {
                    tile.UpdateTileState(Tile.TileState.Blocked);  // ��ֹ�
                }
                else if (tileType == 2)
                {
                    tile.UpdateTileState(Tile.TileState.Normal);  // ������ �ִ� Ÿ��
                    tile.hasUnit = true;
                }
                else
                {
                    tile.UpdateTileState(Tile.TileState.Normal);  // �� Ÿ��
                }
            }
        }
    }

    // ���� ScriptableObject�� ����
    public void SaveGridToScriptableObject()
    {
        mapData.SaveTileData(tiles);
        Debug.Log("���� ScriptableObject�� ����Ǿ����ϴ�.");
    }

    // ���� CSV�� ����
    public void SaveGridToCSV()
    {
        string path = Path.Combine(Application.dataPath, csvFileName);

        using (StreamWriter writer = new StreamWriter(path))
        {
            for (int y = 0; y < height; y++)
            {
                string line = "";
                for (int x = 0; x < width; x++)
                {
                    int tileType = 0;
                    if (tiles[x, y].tileState == Tile.TileState.Blocked)
                    {
                        tileType = 1;
                    }
                    else if (tiles[x, y].hasUnit)
                    {
                        tileType = 2;
                    }
                    line += tileType.ToString();
                    if (x < width - 1)
                    {
                        line += ",";
                    }
                }
                writer.WriteLine(line);
            }
        }

        Debug.Log("���� CSV ���Ϸ� ����Ǿ����ϴ�: " + path);
    }

    // �׸��带 ����
    private void ClearGrid()
    {
        if (tiles != null)
        {
            foreach (var tile in tiles)
            {
                if (tile != null)
                {
                    Destroy(tile.gameObject);
                }
            }
        }
    }
}
