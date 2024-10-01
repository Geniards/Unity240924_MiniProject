using UnityEngine;

[CreateAssetMenu(fileName = "NewTileMapData", menuName = "TileMap/TileMapData")]
public class TileMapData : ScriptableObject
{
    public int width;
    public int height;
    public int[] tileData;  // �� Ÿ���� �����͸� 1���� �迭�� ���� (width * height) ���� ���̷���.

    // �� �����͸� �ʱ�ȭ�� �� ȣ��
    public void InitializeMapData()
    {
        tileData = new int[width * height];
    }

    // Ÿ�� �����͸� �����ϴ� �޼���
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
                    tileData[tileIndex] = 1;  // ��ֹ�
                }
                else if (tiles[x, y].hasUnit)
                {
                    tileData[tileIndex] = 2;  // ������ �ִ� Ÿ��
                }
                else
                {
                    tileData[tileIndex] = 0;  // �� Ÿ��
                }
            }
        }
    }

    // Ÿ�� �����͸� �ҷ����� �޼���
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
                    tiles[x, y].UpdateTileState(Tile.TileState.Blocked);  // ��ֹ�
                }
                else if (tileType == 2)
                {
                    tiles[x, y].UpdateTileState(Tile.TileState.Normal);  // ������ �ִ� Ÿ��
                    tiles[x, y].hasUnit = true;
                }
                else
                {
                    tiles[x, y].UpdateTileState(Tile.TileState.Normal);  // �� Ÿ��
                }
            }
        }
    }
}
