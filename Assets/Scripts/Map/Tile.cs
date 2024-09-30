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

    // ���콺 Ŭ�� �� �߻��ϴ� �̺�Ʈ
    private void OnMouseDown()
    {
        if (GameManager.Instance.SelectedUnit != null)
        {
            // ������ ���õ� ���¿��� Ÿ���� Ŭ���� ��� ó��
            GameManager.Instance.OnTileClicked(this);
        }
    }
}
