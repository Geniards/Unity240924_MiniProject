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

    public int distance;

    public void ClearSearchInfo()
    {
        prev = null;
        distance = int.MaxValue;
    }

    public void SetSearchInfo(TileInfo info)
    {
        prev = info;
        distance = info.distance + 1;
    }

    public TileInfo(Vector2Int coord, TileState state)
    {
        this.coord = coord;
        this.tileState = state;
        ClearSearchInfo();
    }
}
