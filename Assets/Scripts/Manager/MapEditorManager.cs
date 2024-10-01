using System.IO;
using UnityEngine;

public class MapEditorManager : MonoBehaviour
{
    public TileMapData mapData;        // ScriptableObject로 맵 데이터를 저장
    public GameObject tilePrefab;      // 타일 프리팹
    public string csvFileName = "map.csv";   // CSV 파일 이름

    private Tile[,] tiles;             // 현재 생성된 타일을 저장할 배열

    private int width;
    private int height;

    private void Start()
    {
        // ScriptableObject로부터 기본 크기 설정
        width = mapData.width;
        height = mapData.height;
        GenerateGridFromScriptableObject();
    }

    // 맵을 ScriptableObject로부터 불러와서 그리드 생성
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

        // ScriptableObject로부터 타일 데이터 로드
        mapData.LoadTileData(tiles);
    }

    // 맵을 CSV로부터 불러와서 그리드 생성
    public void GenerateGridFromCSV()
    {
        ClearGrid();

        // Resources 폴더 내에서 CSV 파일을 로드
        TextAsset csvFile = Resources.Load<TextAsset>(csvFileName);
        if (csvFile == null)
        {
            Debug.LogError("CSV 파일을 찾을 수 없습니다: " + csvFileName);
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
                    tile.UpdateTileState(Tile.TileState.Blocked);  // 장애물
                }
                else if (tileType == 2)
                {
                    tile.UpdateTileState(Tile.TileState.Normal);  // 유닛이 있는 타일
                    tile.hasUnit = true;
                }
                else
                {
                    tile.UpdateTileState(Tile.TileState.Normal);  // 빈 타일
                }
            }
        }
    }

    // 맵을 ScriptableObject로 저장
    public void SaveGridToScriptableObject()
    {
        mapData.SaveTileData(tiles);
        Debug.Log("맵이 ScriptableObject로 저장되었습니다.");
    }

    // 맵을 CSV로 저장
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

        Debug.Log("맵이 CSV 파일로 저장되었습니다: " + path);
    }

    // 그리드를 삭제
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
