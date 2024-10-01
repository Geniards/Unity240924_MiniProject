using UnityEngine;

[CreateAssetMenu(fileName = "NewTileMapData", menuName = "TileMap/TileMapData")]
public class TileMapData : ScriptableObject
{
    public int width;
    public int height;
    public int[] tileData;  // 각 타일의 데이터를 1차원 배열로 저장 (width * height) 양을 줄이려고.

    // 맵 데이터를 초기화할 때 호출
    public void InitializeMapData()
    {
        tileData = new int[width * height];
    }

    // 타일 데이터를 저장하는 메서드
    public void SaveTileData(Tile[,] tiles)
    {
        tileData = new int[width * height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tileIndex = x + y * width;
                if (tiles[x, y].tileState == Tile.TileState.Blocked)
                {
                    tileData[tileIndex] = 1;  // 장애물
                }
                else if (tiles[x, y].hasUnit)
                {
                    tileData[tileIndex] = 2;  // 유닛이 있는 타일
                }
                else
                {
                    tileData[tileIndex] = 0;  // 빈 타일
                }
            }
        }
    }

    // 타일 데이터를 불러오는 메서드
    public void LoadTileData(Tile[,] tiles)
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int tileIndex = x + y * width;
                int tileType = tileData[tileIndex];

                if (tileType == 1)
                {
                    tiles[x, y].UpdateTileState(Tile.TileState.Blocked);  // 장애물
                }
                else if (tileType == 2)
                {
                    tiles[x, y].UpdateTileState(Tile.TileState.Normal);  // 유닛이 있는 타일
                    tiles[x, y].hasUnit = true;
                }
                else
                {
                    tiles[x, y].UpdateTileState(Tile.TileState.Normal);  // 빈 타일
                }
            }
        }
    }
}
