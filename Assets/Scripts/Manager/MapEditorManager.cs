using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapEditorManager : MonoBehaviour
{
    public Tilemap tilemap;                     // 유니티의 타일맵
    public TileMapData mapData;                 // ScriptableObject에 저장할 타일 데이터
    public TileBase defaultTile;                // 기본 타일 (나중에 불러올 때 사용할 타일)
    public string csvFileName = "map.csv";      // CSV 파일 이름
    public TileBase[] tileBases;                // 타일 타입을 지정하기 위한 타일베이스 배열 (타일 종류들)

    private Tile[,] tiles;                      // 현재 생성된 타일을 저장할 배열

    private int width;
    private int height;

    private void Start()
    {
        // 타일맵의 크기를 설정
        BoundsInt bounds = tilemap.cellBounds;
        width = bounds.size.x;
        height = bounds.size.y;

        // 필요한 작업 (저장/불러오기)
        SaveTilemapToScriptableObject();
        SaveTilemapToCSV();
    }

    // 타일맵 데이터를 ScriptableObject로 저장
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
                    mapData.tileData[tileIndex] = 1; // 타일이 있으면 1
                }
                else
                {
                    mapData.tileData[tileIndex] = 0; // 없으면 0
                }
            }
        }

        Debug.Log("Tilemap saved to ScriptableObject.");
    }

    // 타일맵 데이터를 CSV 파일로 저장
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

                    int tileType = tile != null ? 1 : 0; // 타일이 있으면 1, 없으면 0
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

    // ScriptableObject로부터 타일맵 불러오기
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
                    tilemap.SetTile(tilePosition, defaultTile); // 타일을 배치
                }
                else
                {
                    tilemap.SetTile(tilePosition, null); // 타일을 제거
                }
            }
        }

        Debug.Log("Tilemap loaded from ScriptableObject.");
    }

    // CSV 파일로부터 타일맵 불러오기
    public void LoadTilemapFromCSV()
    {
        string path = Path.Combine(Application.dataPath, csvFileName);
        if (!File.Exists(path))
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다.");
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
                    tilemap.SetTile(tilePosition, defaultTile); // 타일을 배치
                }
                else
                {
                    tilemap.SetTile(tilePosition, null); // 타일을 제거
                }
            }
        }

        Debug.Log("Tilemap loaded from CSV.");
    }

    // 맵을 ScriptableObject로부터 불러와서 타일맵 적용
    public void GenerateGridFromScriptableObject()
    {
        // 기존 타일맵 초기화
        ClearGrid();

        // ScriptableObject로부터 타일 데이터 로드
        mapData.LoadTileData(tiles);

        // 타일맵에 데이터 적용
        ApplyTilemapData(tiles);
    }

    // 맵을 CSV로부터 불러와서 타일맵 적용
    public void GenerateGridFromCSV()
    {
        // CSV 파일 로드 및 타일 데이터 적용
        LoadTilemapFromCSV();

        // 타일맵에 데이터 적용
        ApplyTilemapData(tiles);
    }

    // 타일맵에 데이터 적용
    private void ApplyTilemapData(Tile[,] tiles)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // 타일 인덱스 가져오기
                int tileIndex = x + y * width;
                int tileType = mapData.tileData[tileIndex];

                // 타일 타입에 따라 타일맵에 타일 배치
                if (tileType == 1)
                {
                    // Blocked tile (장애물)
                    tilemap.SetTile(new Vector3Int(x, y, 0), tileBases[1]);
                }
                else if (tileType == 2)
                {
                    // 유닛이 있는 타일
                    tilemap.SetTile(new Vector3Int(x, y, 0), tileBases[2]);
                }
                else
                {
                    // 기본 빈 타일
                    tilemap.SetTile(new Vector3Int(x, y, 0), tileBases[0]);
                }
            }
        }
    }

    // 기존 타일맵을 초기화하는 메서드
    private void ClearGrid()
    {
        if (tilemap != null)
        {
            tilemap.ClearAllTiles();
        }
    }
}
