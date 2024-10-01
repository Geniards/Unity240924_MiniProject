using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject SelectedUnit;           // 선택된 유닛
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
        Debug.Log("유닛 선택됨: " + unit.name);  // 유닛이 제대로 선택되었는지 확인
    }
}
