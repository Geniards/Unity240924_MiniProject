using System.Collections.Generic;
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
    public bool isReachable;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public static GameObject selectedUnit;

    private Color originalColor;       // 기본 상태 색상 (흰색)
    private Color reachableColor;      // 이동 가능 색상 (녹색)
    private Color hoverColor;          // 마우스가 이동 가능 타일 위에 있을 때 색상 (흰색)
    private Color unreachableColor;    // 이동 불가 색상 (알파값 0)
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
        isReachable = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
        // 기본 색상 초기화
        originalColor = Color.white;
        reachableColor = new Color(0f, 1f, 0f, 0.5f); // 녹색 반투명
        hoverColor = Color.yellow;                    // 노란색
        unreachableColor = new Color(1f, 1f, 1f, 0f); // 알파값 0

        UpdateTileVisual();
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
        Debug.Log("타일 상태 변경: " + tileState);  // 타일 상태가 업데이트 되는지 확인
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
        originalColor = spriteRenderer.color;
    }

    // 이동 가능 타일 설정
    public void SetReachable(bool reachable)
    {
        isReachable = reachable;

        if (isReachable)
        {
            spriteRenderer.color = reachableColor; // 이동 가능 범위는 녹색
        }
        else
        {
            spriteRenderer.color = unreachableColor; // 그 외 지역은 알파값 0
        }
    }

    // 마우스가 타일 위에 있을 때
    private void OnMouseEnter()
    {
        if (isReachable)
        {
            // 이동 가능 범위 안에서는 흰색으로 변경
            spriteRenderer.color = hoverColor;
        }
        else
        {
            // 이동 불가능한 타일은 반투명하게 표시
            SetTileAlpha(0.35f);
        }

        if (hasUnit && currentUnit != null)
        {
            // 유닛이 있는 경우 유닛 정보 표시
            Unit unit = currentUnit.GetComponent<Unit>();
            TileUIManager.Instance.ShowUnitInfo(unit);
        }
    }

    // 마우스가 타일을 벗어날 때
    private void OnMouseExit()
    {
        if (isReachable)
        {
            // 이동 가능 범위로 돌아가면 다시 녹색으로 변경
            spriteRenderer.color = reachableColor;
        }
        else
        {
            SetTileAlpha(0f);
        }

        TileUIManager.Instance.HideInfo();
    }

    // 타일의 알파값 설정
    private void SetTileAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    // 마우스 클릭 시
    private void OnMouseDown()
    {
        // 선택된 유닛이 없는 경우
        if (selectedUnit == null)
        {
            // 빈 타일 클릭 시 UI 표시
            if (hasUnit && currentUnit != null)
            {
                var unit = currentUnit.GetComponent<Unit>();
                if (unit != null)
                {
                    selectedUnit = currentUnit;
                    var reachableTiles = GridManager.Instance.FindReachableTiles(this, unit.stats.moveRange);

                    foreach (Tile tile in reachableTiles)
                    {
                        tile.SetReachable(true);
                        tile.SetTileAlpha(0.5f);
                    }

                    TileUIManager.Instance.ShowUnitInfo(unit);
                }
            }
        }
        else
        {
            // 선택된 유닛이 있고, 이동 가능 타일을 클릭하면 유닛 이동 처리
            if (isReachable)
            {
                Unit unit = selectedUnit.GetComponent<Unit>();
                List<Tile> path = GridManager.Instance.FindPath(unit.currentTile, this);

                if (path != null)
                {
                    unit.MoveToTile(this);
                }
            }
        }
    }


}

