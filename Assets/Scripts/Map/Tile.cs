using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinates;    // 타일의 좌표
    public bool hasUnit;              // 유닛이 있는지 여부
    public GameObject currentUnit;    // 현재 타일에 있는 유닛
    public Tile parentTile;           // 경로 탐색을 위한 부모 타일
    public Tile previousTile;         // 이전에 방문한 타일
    public TileState tileState;       // 타일의 상태 (이동 가능/불가 등)
    public int costFromStart;         // 시작 타일부터의 이동 비용

    [SerializeField] private SpriteRenderer spriteRenderer;
    public enum TileState
    {
        Normal,
        Blocked,        // 이동 불가 상태
        Selected,       // 선택된 상태
        Path,           // 경로의 일부
        Attackable      // 공격 가능한 범위에 있는 타일
    }

    public void Init(Vector2Int coords)
    {
        coordinates = coords;
        hasUnit = false;
        currentUnit = null;
        parentTile = null;
        previousTile = null;
        tileState = TileState.Normal;
        costFromStart = 0;

        spriteRenderer = GetComponent<SpriteRenderer>();
        SetTileAlpha(0f);
    }

    public void PlaceUnit(GameObject unit)
    {
        hasUnit = true;
        currentUnit = unit;
    }

    public void RemoveUnit()
    {
        hasUnit = false;
        currentUnit = null;
    }

    public void UpdateTileState(TileState newState)
    {
        tileState = newState;
        UpdateTileVisual();
    }

    private void UpdateTileVisual()
    {
        switch (tileState)
        {
            case TileState.Normal:
                GetComponent<SpriteRenderer>().color = Color.white;
                break;
            case TileState.Blocked:
                GetComponent<SpriteRenderer>().color = Color.gray;
                break;
            case TileState.Selected:
                GetComponent<SpriteRenderer>().color = Color.green;
                break;
            case TileState.Path:
                GetComponent<SpriteRenderer>().color = Color.blue;
                break;
            case TileState.Attackable:
                GetComponent<SpriteRenderer>().color = Color.red;
                break;
        }
    }

    private void OnMouseDown()
    {
        // 빈 타일을 클릭했을 때 UI를 표시
        if (!hasUnit)
        {
            // TileInfoUI.Instance가 null이 아닌지 확인
            if (TileInfoUI.Instance != null)
            {
                TileInfoUI.Instance.ShowTileInfo(this);
            }
            else
            {
                Debug.LogError("TileInfoUI.Instance가 초기화되지 않았습니다.");
            }
        }
    }

    private void OnMouseEnter()
    {
        SetTileAlpha(0.35f);
    }

    private void OnMouseExit()
    {
        SetTileAlpha(0f);
    }

    private void SetTileAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}
