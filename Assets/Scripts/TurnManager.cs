using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<PlayerUnit> playerUnits; // �Ʊ� ���� ����Ʈ
    public List<EnemyUnit> enemyUnits; // ���� ���� ����Ʈ

    public enum TurnState { PlayerTurn, EnemyTurn, TURNSTATE_MAX }
    public TurnState currentTurn;

    void Start()
    {
        StartPlayerTurn();
    }

    // �Ʊ� �� ����
    void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;

        // ��� �Ʊ� ������ �ൿ ����Ʈ�� �ʱ�ȭ
        foreach (var unit in playerUnits)
        {
            unit.ResetActionPoint();
        }

        Debug.Log("�÷��̾� �� ����");
    }

    public void OnPlayerUnitActed()
    {
        if (AllPlayerUnitsActed())
        {
            StartEnemyTurn();
        }
    }

    bool AllPlayerUnitsActed()
    {
        foreach (var unit in playerUnits)
        {
            // �ൿ ����Ʈ�� ���� ������ ������ false
            if (unit.GetActionPoint() > 0) 
            {
                return false;
            }
        }
        return true;
    }

    // ���� �� ����
    void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("���� �� ����");

        // AI �ൿ (�ڷ�ƾ���� ���� ����)
    }
}
