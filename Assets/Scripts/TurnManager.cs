using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public List<PlayerUnit> playerUnits; // 아군 유닛 리스트
    public List<EnemyUnit> enemyUnits; // 적군 유닛 리스트

    public enum TurnState { PlayerTurn, EnemyTurn, TURNSTATE_MAX }
    public TurnState currentTurn;

    void Start()
    {
        StartPlayerTurn();
    }

    // 아군 턴 시작
    void StartPlayerTurn()
    {
        currentTurn = TurnState.PlayerTurn;

        // 모든 아군 유닛의 행동 포인트를 초기화
        foreach (var unit in playerUnits)
        {
            unit.ResetActionPoint();
        }

        Debug.Log("플레이어 턴 시작");
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
            // 행동 포인트가 남은 유닛이 있으면 false
            if (unit.GetActionPoint() > 0) 
            {
                return false;
            }
        }
        return true;
    }

    // 적군 턴 시작
    void StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("적군 턴 시작");

        // AI 행동 (코루틴으로 진행 예정)
    }
}
