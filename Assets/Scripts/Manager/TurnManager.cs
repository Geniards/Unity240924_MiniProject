using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    // FSM을 위한 상태 열거형 정의
    public enum TurnState
    {
        UnitSelection,  // 유닛 선택
        UnitMove,       // 유닛 이동
        UnitAttack,     // 유닛 공격
        ActionUISelect, // UI창 선택
        EndTurn         // 턴 종료
    }

    public List<Unit> allyUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();

    public TurnState currentState;   // 현재 FSM 상태
    private Unit selectedUnit;       // 현재 선택된 유닛
    private int allyUnitsFinishedMovement = 0;
    private int enemyUnitsFinishedMovement = 0;
    private bool enemyTurnInProgress = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 턴을 유닛 선택 상태로 시작
        StartTurn(TurnState.UnitSelection);
    }

    // 상태에 따라 턴을 시작
    public void StartTurn(TurnState initialState)
    {
        currentState = initialState;
        Debug.Log("Current Turn State: " + currentState.ToString());

        if (currentState == TurnState.UnitSelection)
        {
            // 모든 아군 유닛의 이동을 초기화
            foreach (var unit in allyUnits)
            {
                unit.ResetMovement();
                unit.StartTurn();
            }

            allyUnitsFinishedMovement = 0;  // 이동 완료 유닛 초기화
        }
    }

    // 상태 전환 메서드
    public void ChangeState(TurnState newState)
    {
        currentState = newState;
        Debug.Log("State changed to: " + newState.ToString());

        switch (currentState)
        {
            case TurnState.UnitSelection:
                SelectUnit();
                break;
            case TurnState.UnitMove:
                MoveUnit();
                break;
            case TurnState.UnitAttack:
                AttackWithUnit();
                break;
            case TurnState.ActionUISelect:
                TileUIManager.Instance.ShowActionMenu(selectedUnit.transform.position, selectedUnit);

                break;
            case TurnState.EndTurn:
                if (AreAllAlliesFinished())
                {
                    StartCoroutine(StartEnemyTurn());  // 적군 턴 시작
                }
                break;
        }
    }
    private bool AreAllAlliesFinished()
    {
        return allyUnitsFinishedMovement >= allyUnits.Count;
    }

    private bool AreAllEnemiesFinished()
    {
        return enemyUnitsFinishedMovement >= enemyUnits.Count;
    }

    // 유닛 선택 상태 처리
    public void SelectUnit()
    {
        Debug.Log("캐릭터 선택");
        // 유닛을 클릭하여 선택하는 로직 구현 (UI에서 유닛을 선택하는 흐름)
    }

    // 유닛 이동 상태 처리
    public void MoveUnit()
    {
        Debug.Log("선택한 유닛 이동");
        // 선택된 유닛의 이동 가능 타일을 표시하고 이동 처리
        if (selectedUnit != null)
        {
            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(selectedUnit.currentTile, selectedUnit.stats.moveRange);
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(true);
            }
        }
    }

    // 유닛 공격 상태 처리
    public void AttackWithUnit()
    {
        Debug.Log("선택한 유닛 공격");
        // 선택된 유닛이 공격 가능한 타일을 표시하고 공격 처리
        List<Tile> attackableTiles = GridManager.Instance.FindAttackableTiles(selectedUnit.currentTile, selectedUnit.stats.attackRange);
        foreach (Tile tile in attackableTiles)
        {
            tile.UpdateTileState(Tile.TileState.Attackable);
        }
    }

    // 유닛이 턴을 완료했을 때 호출
    public void NotifyUnitMovementFinished(Unit unit)
    {
        allyUnitsFinishedMovement++;
        Debug.Log($"유닛 {unit.stats.unitName} 이동 완료: {allyUnitsFinishedMovement} / {allyUnits.Count}");

        selectedUnit.EndTurn();
        unit.SaveOriginalPosition();  // 현재 위치 저장

        if (allyUnitsFinishedMovement >= allyUnits.Count)
        {
            ChangeState(TurnState.EndTurn);
            //allyUnitsFinishedMovement = 0;
        }
        else
        {
            //selectedUnit.EndTurn();
            ChangeState(TurnState.UnitSelection);
        }
    }

    // 아군 유닛의 턴이 모두 끝났는지 확인하고 턴 종료 처리
    private void CheckAllyTurnCompletion()
    {
        if (AreAllAlliesFinished())
        {
            Debug.Log("모든 아군의 턴이 종료되었습니다. 적군의 턴을 시작합니다.");
            allyUnitsFinishedMovement = 0; // 완료된 아군 유닛 수 초기화

            // 아군 턴이 끝나면 모든 아군 유닛의 상태를 초기화
            foreach (var unit in allyUnits)
            {
                unit.ResetTurnState();
            }

            StartCoroutine(StartEnemyTurn()); // 적군의 턴 시작
        }
        else
        {
            //NotifyUnitMovementFinished(GetSelectedUnit());
            Debug.Log("아직 턴이 완료되지 않았습니다.");
            ChangeState(TurnState.UnitSelection);
        }
    }

    // 적군 턴 처리
    private IEnumerator StartEnemyTurn()
    {
        // 적군 턴이 이미 진행 중이면 중복으로 실행하지 않음
        if (enemyTurnInProgress) yield break;

        Debug.Log("Enemy turn started.");
        enemyTurnInProgress = true;
        enemyUnitsFinishedMovement = 0;

        foreach (Unit enemy in enemyUnits)
        {
            yield return StartCoroutine(TakeTurn(enemy));
            yield return new WaitForSeconds(0.5f);  // 적군 이동 후 잠시 대기
        }

        Debug.Log("적군 턴 종료");
        enemyTurnInProgress = false;
        StartTurn(TurnState.UnitSelection);  // 적군 턴이 끝나면 다시 아군 유닛 선택 상태로
    }

    private IEnumerator TakeTurn(Unit unit)
    {
        // 유닛이 파괴되었는지 먼저 확인
        if (unit == null)
        {
            Debug.Log("유닛이 이미 제거되었습니다. 턴을 건너뜁니다.");
            yield break;
        }

        Debug.Log($"{name}의 턴 시작");

        // 1. 공격 가능 범위 내에 아군이 있는지 먼저 확인
        List<Tile> attackableTiles = GridManager.Instance.FindAttackableTiles(unit.currentTile, unit.stats.attackRange);
        Unit closestAllyInRange = null;

        foreach (var v in attackableTiles)
            Debug.Log($"{v.name}");

        foreach (Tile tile in attackableTiles)
        {
            if (tile.hasUnit && tile.currentUnit != null)
            {
                Unit targetUnit = tile.currentUnit.GetComponent<Unit>();
                if (targetUnit.team == Team.Ally)
                {
                    closestAllyInRange = targetUnit;
                    break;
                }
            }
        }

        // 2. 공격 가능 범위에 아군이 있으면 이동하지 않고 바로 공격
        if (closestAllyInRange != null)
        {
            Debug.Log("적군이 공격 범위 안에 아군을 발견, 공격을 시도합니다.");

            // 공격 범위 하이라이트
            foreach (Tile tile in attackableTiles)
            {
                tile.UpdateTileState(Tile.TileState.Attackable);
            }

            // 잠시 대기 후 공격
            yield return new WaitForSeconds(0.5f);
            unit.Attack(closestAllyInRange);

            // 턴 종료
            yield return new WaitForSeconds(0.5f);
            unit.EndTurn();
            enemyUnitsFinishedMovement++;
            Debug.Log($"{unit.name}의 턴 종료");
            yield break; // 공격 후 턴을 넘김
        }

        // 3. 공격 범위 안에 아군이 없으면 이동
        Unit closestAlly = GridManager.Instance.FindClosestAlly(unit);
        if (closestAlly != null)
        {
            // 이동 가능한 타일 탐색
            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(unit.currentTile, unit.stats.moveRange);

            // 이동 가능타일 하이라이트 On
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(true);
            }

            yield return new WaitForSeconds(0.5f);

            // 가까운 아군근처 타일 탐지
            Tile closestTile = GridManager.Instance.FindClosestValidTile(unit, closestAlly);
            if (closestTile != null)
            {
                // 경로 탐색 및 이동
                List<Tile> path = GridManager.Instance.FindPath(unit.currentTile, closestTile);
                List<Tile> limitedPath = GridManager.Instance.GetLimitedPath(path, unit.stats.moveRange);

                if (limitedPath != null && limitedPath.Count > 0)
                {
                    yield return StartCoroutine(unit.MoveAlongPathCoroutine(limitedPath));
                }
            }
            else
            {
                Debug.Log("주변에 이동 가능한 타일이 없습니다. 턴 종료.");
                yield break;  // 주변에 이동 가능한 타일이 없으면 턴 종료
            }

            // 적군이 이동한 후 하이라이트를 끄기
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(false);
            }

            // 4. 아군이 공격 범위 내에 들어왔는지 확인
            attackableTiles = GridManager.Instance.FindAttackableTiles(unit.currentTile, unit.stats.attackRange);
            closestAllyInRange = null;

            foreach (Tile tile in attackableTiles)
            {
                if (tile.hasUnit && tile.currentUnit != null)
                {
                    Unit targetUnit = tile.currentUnit.GetComponent<Unit>();
                    if (targetUnit.team == Team.Ally)
                    {
                        closestAllyInRange = targetUnit;
                        break;
                    }
                }
            }

            // 공격 가능 범위에 아군이 있으면 공격
            if (closestAllyInRange != null)
            {
                Debug.Log("적군이 이동 후 공격 범위 안에 아군을 발견, 공격을 시도합니다.");

                // 공격 가능 타일 하이라이트
                foreach (Tile tile in attackableTiles)
                {
                    tile.UpdateTileState(Tile.TileState.Attackable);
                }

                // 5. 공격 대기 및 실행
                yield return new WaitForSeconds(0.5f);

                unit.Attack(closestAllyInRange);  // 적군이 아군을 공격
                yield return new WaitForSeconds(0.5f);
            }

            // 공격하지 않더라도 하이라이트는 무조건 제거
            foreach (Tile tile in attackableTiles)
            {
                tile.RestoreOriginalState();
            }
        }



        enemyUnitsFinishedMovement++;
        Debug.Log($"{unit.name}의 턴 종료");
    }

    // 선택된 유닛 설정
    public void SetSelectedUnit(Unit unit)
    {
        selectedUnit = unit;
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    public void AddAllyUnit(Unit unit) => allyUnits.Add(unit);
    public void AddEnemyUnit(Unit unit) => enemyUnits.Add(unit);

    // 아군 유닛 제거
    public void RemoveAllyUnit(Unit unit)
    {
        if (allyUnits.Contains(unit))
        {
            allyUnits.Remove(unit);
            Debug.Log($"{unit.stats.unitName} 사망: 아군 리스트에서 제거");
        }

        CheckTurnEnd();
    }

    // 적군 유닛 제거
    public void RemoveEnemyUnit(Unit unit)
    {
        if (enemyUnits.Contains(unit))
        {
            enemyUnits.Remove(unit);
            Debug.Log($"{unit.stats.unitName} 사망: 적군 리스트에서 제거");
        }

        CheckTurnEnd();
    }

    // 모든 아군/적군이 죽었을 경우 턴을 종료하는 메서드
    private void CheckTurnEnd()
    {
        if (allyUnits.Count == 0)
        {
            Debug.Log("모든 아군이 사망했습니다. 게임 오버");
            // 게임 오버 또는 다른 처리
        }
        else if (enemyUnits.Count == 0)
        {
            Debug.Log("모든 적군이 사망했습니다. 승리!");
            // 승리 또는 다음 스테이지로 이동
        }
        else
            ChangeState(TurnState.UnitSelection);
    }
}
