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

        // ���콺 Ŭ�� �̺�Ʈ
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

        // ���콺 ���� �̺�Ʈ
        EventTrigger.Entry hoverEntry = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        hoverEntry.callback.AddListener((eventData) => OnTileHoverEnter());
        eventTrigger.triggers.Add(hoverEntry);

        // ���콺 ���� ���� �̺�Ʈ
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
        // ���� ���¸� ó�� ����� ���� ���� (�� ���� ����)
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

    public void SetReachable(bool reachable)
    {
        isReachable = reachable;
        spriteRenderer.color = isReachable ? reachableColor : unreachableColor;
    }

    // ���콺 Ŭ�� ��
    private void OnTileClicked()
    {

        if (TurnManager.Instance.currentState == TurnManager.TurnState.UnitSelection && hasUnit && currentUnit != null)
        {
            Unit unit = currentUnit.GetComponent<Unit>();
            if (unit.team == Team.Ally)
            {
                TurnManager.Instance.SetSelectedUnit(unit);
                ShowMoveRange(unit);
                // ���� ���� �� �̵� ���·� ��ȯ
                TurnManager.Instance.ChangeState(TurnManager.TurnState.UnitMove);
            }
            else if(!unit)
            {
                TileUIManager.Instance.ShowTileInfo(this);
            }
        }
        else if (TurnManager.Instance.currentState == TurnManager.TurnState.UnitMove)
        {
            Unit selectedUnit = TurnManager.Instance.GetSelectedUnit();

            if (selectedUnit && isReachable)
            {
                // ������ ���õ� Ÿ�Ϸ� �̵�
                selectedUnit.MoveToTile(this);
                GridManager.Instance.ClearMoveHighlight(selectedUnit.OnUnitSelected());

                // �̵� �� UI���� �׼� ��ư�� Ȱ��ȭ
            }
        }
        // ���� ���� ó��
        else if (TurnManager.Instance.currentState == TurnManager.TurnState.UnitAttack)
        {
            if (hasUnit && currentUnit != null)
            {
                Unit targetUnit = currentUnit.GetComponent<Unit>();
                Unit selectedUnit = TurnManager.Instance.GetSelectedUnit();

                // ������ ���� ����
                if (selectedUnit != null && targetUnit != null && targetUnit.team != selectedUnit.team)
                {
                    selectedUnit.Attack(targetUnit);
                    GridManager.Instance.ClearMoveHighlight(selectedUnit.OnUnitSelected());
                    
                    // ���� �� ���� ����
                    TurnManager.Instance.ChangeState(TurnManager.TurnState.EndTurn);
                }
                else
                {
                    Debug.Log("�ƹ��� ����");
                    TileUIManager.Instance.ShowActionMenu(selectedUnit.transform.position, selectedUnit);
                }
                GridManager.Instance.ClearMoveHighlight(selectedUnit.OnUnitSelected());
            }
        }
    }

    // ��Ŭ�� �� ���� ��� �� ���� �ʱ�ȭ
    private void OnRightClick()
    {
        Unit selectedUnit = TurnManager.Instance.GetSelectedUnit();
        if (selectedUnit != null)
        {
            selectedUnit.CancelMove(); // �̵� ���
            GridManager.Instance.ClearMoveHighlight(selectedUnit.OnUnitSelected());
            TurnManager.Instance.ChangeState(TurnManager.TurnState.UnitSelection);
        }
        else
        {
            Debug.Log("���õ� ������ �����ϴ�. ��Ŭ���� ���õ˴ϴ�.");
        }

    }

    // ���콺 ���� ��
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

    // ���콺 ���� ���� ��
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

    // ������ �̵� ������ �����ִ� �޼���
    private void ShowMoveRange(Unit unit)
    {
        List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(unit.currentTile, unit.stats.moveRange);
        foreach (Tile tile in reachableTiles)
        {
            tile.SetReachable(true);
        }
    }

    // ���� Ÿ�� ���·� �����ϴ� �޼���
    public void RestoreOriginalState()
    {
        tileState = originalState;
        UpdateTileVisual();
    }
}
