using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject SelectedUnit;           // ���õ� ����
    public GridManager gridManager;           // �׸��� ������
    private Tile selectedTile;                // ���õ� Ÿ��

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // ������ �����ϴ� �޼���
    public void SelectUnit(GameObject unit)
    {
        SelectedUnit = unit;
    }

    // Ÿ�� Ŭ�� ó��
    public void OnTileClicked(Tile tile)
    {
        if (tile.hasUnit)
        {
            Debug.Log("Ÿ�Ͽ� ������ �־� �̵� �Ұ�");
            return;
        }

        if (SelectedUnit != null)
        {
            // ���õ� ������ ������ �̵� ó��
            MoveSelectedUnitToTile(tile);
        }
    }

    // ���õ� ������ Ÿ�Ϸ� �̵�
    public void MoveSelectedUnitToTile(Tile targetTile)
    {
        var unit = SelectedUnit.GetComponent<Unit>();
        unit.MoveToTile(targetTile);

        SelectedUnit = null; // ������ �̵��� �� ���� ����
    }
}
