using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int coordinates;    // Ÿ���� ��ǥ
    public bool hasUnit;              // ������ �ִ��� ����
    public GameObject currentUnit;    // ���� Ÿ�Ͽ� �ִ� ����
    public Tile parentTile;           // ��� Ž���� ���� �θ� Ÿ��
    public Tile previousTile;         // ������ �湮�� Ÿ��
    public TileState tileState;       // Ÿ���� ���� (�̵� ����/�Ұ� ��)
    public int costFromStart;         // ���� Ÿ�Ϻ����� �̵� ���
    public bool isReachable;

    [SerializeField] private SpriteRenderer spriteRenderer;

    public static GameObject selectedUnit;

    private Color originalColor;       // �⺻ ���� ���� (���)
    private Color reachableColor;      // �̵� ���� ���� (���)
    private Color hoverColor;          // ���콺�� �̵� ���� Ÿ�� ���� ���� �� ���� (���)
    private Color unreachableColor;    // �̵� �Ұ� ���� (���İ� 0)
    public enum TileState
    {
        Normal,
        Blocked,        // �̵� �Ұ� ����
        Selected,       // ���õ� ����
        Path,           // ����� �Ϻ�
        Attackable      // ���� ������ ������ �ִ� Ÿ��
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
        // �⺻ ���� �ʱ�ȭ
        originalColor = Color.white;
        reachableColor = new Color(0f, 1f, 0f, 0.5f); // ��� ������
        hoverColor = Color.yellow;                    // �����
        unreachableColor = new Color(1f, 1f, 1f, 0f); // ���İ� 0

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
        Debug.Log("Ÿ�� ���� ����: " + tileState);  // Ÿ�� ���°� ������Ʈ �Ǵ��� Ȯ��
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

    // �̵� ���� Ÿ�� ����
    public void SetReachable(bool reachable)
    {
        isReachable = reachable;

        if (isReachable)
        {
            spriteRenderer.color = reachableColor; // �̵� ���� ������ ���
        }
        else
        {
            spriteRenderer.color = unreachableColor; // �� �� ������ ���İ� 0
        }
    }

    // ���콺�� Ÿ�� ���� ���� ��
    private void OnMouseEnter()
    {
        if (isReachable)
        {
            // �̵� ���� ���� �ȿ����� ������� ����
            spriteRenderer.color = hoverColor;
        }
        else
        {
            // �̵� �Ұ����� Ÿ���� �������ϰ� ǥ��
            SetTileAlpha(0.35f);
        }

        if (hasUnit && currentUnit != null)
        {
            // ������ �ִ� ��� ���� ���� ǥ��
            Unit unit = currentUnit.GetComponent<Unit>();
            TileUIManager.Instance.ShowUnitInfo(unit);
        }
    }

    // ���콺�� Ÿ���� ��� ��
    private void OnMouseExit()
    {
        if (isReachable)
        {
            // �̵� ���� ������ ���ư��� �ٽ� ������� ����
            spriteRenderer.color = reachableColor;
        }
        else
        {
            SetTileAlpha(0f);
        }

        TileUIManager.Instance.HideInfo();
    }

    // Ÿ���� ���İ� ����
    private void SetTileAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }

    // ���콺 Ŭ�� ��
    private void OnMouseDown()
    {
        // ���õ� ������ ���� ���
        if (selectedUnit == null)
        {
            // �� Ÿ�� Ŭ�� �� UI ǥ��
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
            // ���õ� ������ �ְ�, �̵� ���� Ÿ���� Ŭ���ϸ� ���� �̵� ó��
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

