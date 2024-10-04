using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinates;
    public bool hasUnit;
    public GameObject currentUnit;
    public Tile parentTile;

    public TileState tileState;
    public TileState originalState;
    
    public int costFromStart;
    public bool isReachable;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public static GameObject selectedUnit;

    private Color originalColor;
    private Color reachableColor;
    private Color hoverColor;
    private Color unreachableColor;

    public enum TileState
    {
        Normal,
        Blocked,
        Selected,
        Path,
        Attackable
    }

    private void Awake()
    {
        var eventTrigger = gameObject.AddComponent<EventTrigger>();

        // 마우스 클릭 이벤트
        EventTrigger.Entry clickEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerClick
        };
        clickEntry.callback.AddListener((eventData) =>
        {
            PointerEventData pointerData = (PointerEventData)eventData;
            if (pointerData.button == PointerEventData.InputButton.Left)
            {
                OnTileClicked();
            }
            else if (pointerData.button == PointerEventData.InputButton.Right)
            {
                OnRightClick();
            }
        });
        eventTrigger.triggers.Add(clickEntry);

        // 마우스 오버 이벤트
        EventTrigger.Entry hoverEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        hoverEntry.callback.AddListener((eventData) => OnTileHoverEnter());
        eventTrigger.triggers.Add(hoverEntry);

        // 마우스 오버 종료 이벤트
        EventTrigger.Entry exitEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        exitEntry.callback.AddListener((eventData) => OnTileHoverExit());
        eventTrigger.triggers.Add(exitEntry);
    }

    public void Init(Vector2Int coords)
    {
        coordinates = coords;
        hasUnit = false;
        currentUnit = null;
        parentTile = null;
        tileState = TileState.Normal;
        originalState = tileState;

        costFromStart = 0;
        isReachable = false;

        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = new Color(1f, 1f, 1f, 0f);
        reachableColor = new Color(0f, 1f, 0f, 0.5f);
        hoverColor = Color.yellow;
        unreachableColor = new Color(1f, 1f, 1f, 0f);

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
        // 원본 상태를 처음 변경될 때만 저장 (한 번만 저장)
        if (tileState == TileState.Normal || tileState == TileState.Blocked)
        {
            originalState = tileState;
        }

        tileState = newState;
        UpdateTileVisual();
    }

    private void UpdateTileVisual()
    {
        switch (tileState)
        {
            case TileState.Normal:
                spriteRenderer.color = originalColor;
                break;
            case TileState.Blocked:
                spriteRenderer.color = Color.gray;
                break;
            case TileState.Selected:
                spriteRenderer.color = Color.green;
                break;
            case TileState.Path:
                spriteRenderer.color = Color.blue;
                break;
            case TileState.Attackable:
                spriteRenderer.color = Color.red;
                break;
        }
    }

    // 마우스 클릭 시
    private void OnTileClicked()
    {
        // 유닛을 선택 전
        if (TurnManager.Instance.currentState == TurnManager.TurnState.UnitSelection && hasUnit && currentUnit != null)
        {
            Unit unit = currentUnit.GetComponent<Unit>();

            // 유닛의 턴이 종료된 경우 클릭 동작을 무시
            if (unit.hasEndedTurn)
            {
                Debug.Log("턴이 종료된 유닛입니다. 동작하지 않습니다.");
                return;
            }

            if (unit.team == Team.Ally)
            {
                TurnManager.Instance.SetSelectedUnit(unit);
                ShowMoveRange(unit);
                // 유닛 선택 후 이동 상태로 전환
                TurnManager.Instance.ChangeState(TurnManager.TurnState.UnitMove);
            }
        }
        else if (TurnManager.Instance.currentState == TurnManager.TurnState.UnitSelection && !hasUnit && !currentUnit)
        {
            TileUIManager.Instance.ShowTileInfo(this);
        }
        // 유닛을 선택 후
        else if (TurnManager.Instance.currentState == TurnManager.TurnState.UnitMove)
        {
            Unit selectedUnit = TurnManager.Instance.GetSelectedUnit();

            if (selectedUnit && isReachable)
            {
                // 유닛을 선택된 타일로 이동
                selectedUnit.MoveToTile(this);
                GridManager.Instance.ClearMoveHighlight(selectedUnit.OnUnitSelected());

                // 이동 후 UI에서 액션 버튼을 활성화
                // 공격버튼 클릭시 공격범위 활성화
            }
        }
        // 공격 상태 처리
        else if (TurnManager.Instance.currentState == TurnManager.TurnState.UnitAttack)
        {
            if (hasUnit && currentUnit != null)
            {
                Unit targetUnit = currentUnit.GetComponent<Unit>();
                Unit selectedUnit = TurnManager.Instance.GetSelectedUnit();

                // 적군만 공격 가능
                if (selectedUnit != null && targetUnit != null && targetUnit.team != selectedUnit.team)
                {
                    selectedUnit.Attack(targetUnit);
                }
                else
                {
                    Debug.Log("아무도 없음");
                    TileUIManager.Instance.ShowActionMenu(selectedUnit.transform.position, selectedUnit);
                    GridManager.Instance.ClearMoveHighlight(selectedUnit.OnUnitSelected(true));
                }
            }
        }
    }

    // 우클릭 시 선택 취소 및 상태 초기화
    private void OnRightClick()
    {
        Unit selectedUnit = TurnManager.Instance.GetSelectedUnit();
        if (selectedUnit != null)
        {
            selectedUnit.CancelMove(); // 이동 취소
            GridManager.Instance.ClearMoveHighlight(selectedUnit.OnUnitSelected());
            TurnManager.Instance.ChangeState(TurnManager.TurnState.UnitSelection);
        }
        else
        {
            Debug.Log("선택된 유닛이 없습니다. 우클릭이 무시됩니다.");
        }

    }

    // 마우스 오버 시
    private void OnTileHoverEnter()
    {
        if (tileState == TileState.Attackable)
        {
            spriteRenderer.color = Color.red;
        }
        else if (isReachable)
        {
            spriteRenderer.color = hoverColor;
        }
        else
        {
            SetTileAlpha(0.35f);
        }

        if (hasUnit && currentUnit != null)
        {
            Unit unit = currentUnit.GetComponent<Unit>();
            TileUIManager.Instance.ShowUnitInfo(unit);
        }

    }

    // 마우스 오버 종료 시
    private void OnTileHoverExit()
    {
        if (tileState == TileState.Attackable)
        {
            spriteRenderer.color = Color.red;
        }
        else if (isReachable)
        {
            spriteRenderer.color = reachableColor;
        }
        else
        {
            SetTileAlpha(0f);
        }

        TileUIManager.Instance.HideInfo();
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

    // 하이라이트 적용 색상 설정 메서드
    public void SetReachable(bool reachable)
    {
        // Blocked된 타일은 reachable 상태를 변경하지 않음
        if (tileState == TileState.Blocked)
        {
            return; // Blocked 타일은 하이라이트 적용되지 않음
        }

        isReachable = reachable;
        spriteRenderer.color = isReachable ? reachableColor : unreachableColor;
    }

    // 유닛의 이동 범위를 보여주는 메서드
    private void ShowMoveRange(Unit unit)
    {
        List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(unit.currentTile, unit.stats.moveRange);
        foreach (Tile tile in reachableTiles)
        {
            // Blocked 상태가 아닌 경우에만 하이라이트 적용
            if (tile.tileState != TileState.Blocked)
            {
                tile.SetReachable(true);
            }
        }
    }

    // 원래 타일 상태로 복원하는 메서드
    public void RestoreOriginalState()
    {
        tileState = originalState;
        UpdateTileVisual();
    }
}
