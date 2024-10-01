using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject SelectedUnit;           // ���õ� ����
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
        Debug.Log("���� ���õ�: " + unit.name);  // ������ ����� ���õǾ����� Ȯ��
    }
}
