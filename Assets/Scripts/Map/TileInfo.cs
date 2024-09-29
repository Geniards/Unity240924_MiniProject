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

    // 이동가능범위의 거리(BFS용)
    public int distance;

    // A* 코스트 값
    public int gCost;   // 시작 -> 현재 (경로비용)
    public int hCost;   // 현재 -> 목표 (휴리스틱)

    public int fCost { get { return gCost + hCost; } } // 총 합

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
