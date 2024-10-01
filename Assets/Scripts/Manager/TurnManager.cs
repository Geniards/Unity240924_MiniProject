using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEngine;
using static TurnManager;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    public enum TurnState { AllyTurn, EnemyTurn, TURNSTATE_MAX }
    public TurnState currentTurn;

    public List<Unit> allyUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();

    private int unitsFinishedMovement = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        currentTurn = TurnState.AllyTurn;
    }

    private void Start()
    {
        StartAllyTurn();
    }

    // 아군 턴에서 유닛 이동이 종료되었는지 체크하는 메서드
    public void NotifyUnitMovementFinished(Unit unit)
    {
        unitsFinishedMovement++;

        if (unitsFinishedMovement >= allyUnits.Count)
        {
            TileUIManager.Instance.ShowActionMenu(unit.transform.position);
            unitsFinishedMovement = 0;
        }
    }

    // 아군 턴 시작
    public void StartAllyTurn()
    {
        currentTurn = TurnState.AllyTurn;
        Debug.Log("아군 턴 시작");
    }

    // 턴 종료 버튼 클릭 시 호출
    public void EndAllyTurn()
    {
        Debug.Log("아군 턴 종료");
        StartCoroutine(StartEnemyTurn());
        TileUIManager.Instance.HideActionMenu();
    }

    // 적군의 행동 코루틴
    // 적군 턴 시작
    private IEnumerator StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("적군 턴 시작");

        foreach (Unit enemy in enemyUnits)
        {
            yield return StartCoroutine(TakeTurn(enemy));  // 적군 AI 동작
            yield return new WaitForSeconds(0.5f);  // 적군의 이동 후 약간의 대기
        }

        yield return new WaitForSeconds(1.0f);
        Debug.Log("적군 턴 종료");
        StartAllyTurn();

    }

    private IEnumerator TakeTurn(Unit unit)
    {
        Debug.Log($"{name}의 턴 시작");

        Unit closestAlly = GridManager.Instance.FindClosestAlly(unit);
        if (closestAlly != null)
        {
            // 1. 이동 가능한 타일 탐색
            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(unit.currentTile, unit.stats.moveRange);

            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(true);
            }

            yield return new WaitForSeconds(0.5f);


            // 2. 아군 주변의 이동 가능한 타일 선택
            Tile targetTile = GridManager.Instance.FindValidTileNearTarget(closestAlly);

            if (targetTile)
            {
                // 3. 경로 탐색 및 이동
                List<Tile> path = GridManager.Instance.FindPath(unit.currentTile, targetTile);
                List<Tile> limitedPath = GridManager.Instance.GetLimitedPath(path, unit.stats.moveRange);

                if (limitedPath != null)
                {
                    yield return StartCoroutine(unit.MoveAlongPathCoroutine(limitedPath));  // 적군 이동
                }
            }

            // 적군이 이동한 후 하이라이트를 끄기
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(false);
            }
        }
    }

    // 아군과 적군 추가 메서드
    public void AddAllyUnit(Unit unit) => allyUnits.Add(unit);
    public void AddEnemyUnit(Unit unit) => enemyUnits.Add(unit);

}
