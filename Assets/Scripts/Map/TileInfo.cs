using UnityEngine;

public enum TileState
{
    EMPTY, UNIT, OBSTACLE, WATER, TILESTATE_MAX
}

[System.Serializable]
public class TileInfo
{
    public TileState tileState;
    public Vector2Int coord;

    public TileInfo prev;
    public TileInfo Parent;

    // �̵����ɹ����� �Ÿ�(BFS��)
    public int distance;

    // A* �ڽ�Ʈ ��
    public int gCost;   // ���� -> ���� (��κ��)
    public int hCost;   // ���� -> ��ǥ (�޸���ƽ)

    public int fCost { get { return gCost + hCost; } } // �� ��

    public void ClearSearchInfo()
    {
        prev = null;
        distance = int.MaxValue;
        gCost = 0;
        hCost = 0;
    }

    public void SetSearchInfo(TileInfo info)
    {
        prev = info;
        distance = info.distance + 1;
    }

    public void SetAStarCosts(int gCost, int hCost)
    {
        this.gCost = gCost;
        this.hCost = hCost;
    }

    public TileInfo(Vector2Int coord, TileState state)
    {
        this.coord = coord;
        this.tileState = state;
        ClearSearchInfo();
    }
}
