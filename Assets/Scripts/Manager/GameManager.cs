using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject SelectedUnit;           // 선택된 유닛
    public GridManager gridManager;           // 그리드 관리자
    private Tile selectedTile;                // 선택된 타일

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    // 유닛을 선택하는 메서드
    public void SelectUnit(GameObject unit)
    {
        SelectedUnit = unit;
    }

    // 타일 클릭 처리
    public void OnTileClicked(Tile tile)
    {
        if (tile.hasUnit)
        {
            Debug.Log("타일에 유닛이 있어 이동 불가");
            return;
        }

        if (SelectedUnit != null)
        {
            // 선택된 유닛이 있으면 이동 처리
            MoveSelectedUnitToTile(tile);
        }
    }

    // 선택된 유닛을 타일로 이동
    public void MoveSelectedUnitToTile(Tile targetTile)
    {
        var unit = SelectedUnit.GetComponent<Unit>();
        unit.MoveToTile(targetTile);

        SelectedUnit = null; // 유닛을 이동한 후 선택 해제
    }
}
