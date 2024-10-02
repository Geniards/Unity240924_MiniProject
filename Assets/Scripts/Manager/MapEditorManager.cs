using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEditorManager : MonoBehaviour
{
    public Tilemap tilemap;                     // ����Ƽ�� Ÿ�ϸ�
    public TileMapData mapData;                 // ScriptableObject�� ������ Ÿ�� ������
    public TileBase defaultTile;                // �⺻ Ÿ�� (���߿� �ҷ��� �� ����� Ÿ��)
    public string csvFileName = "map.csv";      // CSV ���� �̸�
    public TileBase[] tileBases;                // Ÿ�� Ÿ���� �����ϱ� ���� Ÿ�Ϻ��̽� �迭 (Ÿ�� ������)

    private Tile[,] tiles;                      // ���� ������ Ÿ���� ������ �迭

    private int width;
    private int height;

    private void Start()
    {
        // Ÿ�ϸ��� ũ�⸦ ����
        BoundsInt bounds = tilemap.cellBounds;
        width = bounds.size.x;
        height = bounds.size.y;

        // �ʿ��� �۾� (����/�ҷ�����)
        SaveTilemapToScriptableObject();
        SaveTilemapToCSV();
    }

    // Ÿ�ϸ� �����͸� ScriptableObject�� ����
    public void SaveTilemapToScriptableObject()
    {
        mapData.InitializeMapData(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(tilePosition);

                int tileIndex = x + y * width;
                if (tile != null)
                {
                    mapData.tileData[tileIndex] = 1; // Ÿ���� ������ 1
                }
                else
                {
                    mapData.tileData[tileIndex] = 0; // ������ 0
                }
            }
        }

        Debug.Log("Tilemap saved to ScriptableObject.");
    }

    // Ÿ�ϸ� �����͸� CSV ���Ϸ� ����
    public void SaveTilemapToCSV()
    {
        string path = Path.Combine(Application.dataPath, csvFileName);

        using (StreamWriter writer = new StreamWriter(path))
        {
            for (int y = 0; y < height; y++)
            {
                string line = "";
                for (int x = 0; x < width; x++)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(tilePosition);

                    int tileType = tile != null ? 1 : 0; // Ÿ���� ������ 1, ������ 0
                    line += tileType.ToString();

                    if (x < width - 1)
                    {
                        line += ",";
                    }
                }
                writer.WriteLine(line);
            }
        }

        Debug.Log("Tilemap saved to CSV.");
    }

    // ScriptableObject�κ��� Ÿ�ϸ� �ҷ�����
    public void LoadTilemapFromScriptableObject()
    {
        for (int x = 0; x < mapData.width; x++)
        {
            for (int y = 0; y < mapData.height; y++)
            {
                int tileIndex = x + y * mapData.width;
                int tileType = mapData.tileData[tileIndex];

                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (tileType == 1)
                {
                    tilemap.SetTile(tilePosition, defaultTile); // Ÿ���� ��ġ
                }
                else
                {
                    tilemap.SetTile(tilePosition, null); // Ÿ���� ����
                }
            }
        }

        Debug.Log("Tilemap loaded from ScriptableObject.");
    }

    // CSV ���Ϸκ��� Ÿ�ϸ� �ҷ�����
    public void LoadTilemapFromCSV()
    {
        string path = Path.Combine(Application.dataPath, csvFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("CSV ������ ã�� �� �����ϴ�.");
            return;
        }

        string[] lines = File.ReadAllLines(path);
        for (int y = 0; y < lines.Length; y++)
        {
            string[] values = lines[y].Split(',');
            for (int x = 0; x < values.Length; x++)
            {
                int tileType = int.Parse(values[x]);
                Vector3Int tilePosition = new Vector3Int(x, y, 0);

                if (tileType == 1)
                {
                    tilemap.SetTile(tilePosition, defaultTile); // Ÿ���� ��ġ
                }
                else
                {
                    tilemap.SetTile(tilePosition, null); // Ÿ���� ����
                }
            }
        }

        Debug.Log("Tilemap loaded from CSV.");
    }

    // ���� ScriptableObject�κ��� �ҷ��ͼ� Ÿ�ϸ� ����
    public void GenerateGridFromScriptableObject()
    {
        // ���� Ÿ�ϸ� �ʱ�ȭ
        ClearGrid();

        // ScriptableObject�κ��� Ÿ�� ������ �ε�
        mapData.LoadTileData(tiles);

        // Ÿ�ϸʿ� ������ ����
        ApplyTilemapData(tiles);
    }

    // ���� CSV�κ��� �ҷ��ͼ� Ÿ�ϸ� ����
    public void GenerateGridFromCSV()
    {
        // CSV ���� �ε� �� Ÿ�� ������ ����
        LoadTilemapFromCSV();

        // Ÿ�ϸʿ� ������ ����
        ApplyTilemapData(tiles);
    }

    // Ÿ�ϸʿ� ������ ����
    private void ApplyTilemapData(Tile[,] tiles)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Ÿ�� �ε��� ��������
                int tileIndex = x + y * width;
                int tileType = mapData.tileData[tileIndex];

                // Ÿ�� Ÿ�Կ� ���� Ÿ�ϸʿ� Ÿ�� ��ġ
                if (tileType == 1)
                {
                    // Blocked tile (��ֹ�)
                    tilemap.SetTile(new Vector3Int(x, y, 0), tileBases[1]);
                }
                else if (tileType == 2)
                {
                    // ������ �ִ� Ÿ��
                    tilemap.SetTile(new Vector3Int(x, y, 0), tileBases[2]);
                }
                else
                {
                    // �⺻ �� Ÿ��
                    tilemap.SetTile(new Vector3Int(x, y, 0), tileBases[0]);
                }
            }
        }
    }

    // ���� Ÿ�ϸ��� �ʱ�ȭ�ϴ� �޼���
    private void ClearGrid()
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }
    }
}
