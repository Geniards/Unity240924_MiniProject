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

    // �Ʊ� �Ͽ��� ���� �̵��� ����Ǿ����� üũ�ϴ� �޼���
    public void NotifyUnitMovementFinished(Unit unit)
    {
        unitsFinishedMovement++;

        if (unitsFinishedMovement >= allyUnits.Count)
        {
            TileUIManager.Instance.ShowActionMenu(unit.transform.position);
            unitsFinishedMovement = 0;
        }
    }

    // �Ʊ� �� ����
    public void StartAllyTurn()
    {
        currentTurn = TurnState.AllyTurn;
        Debug.Log("�Ʊ� �� ����");
    }

    // �� ���� ��ư Ŭ�� �� ȣ��
    public void EndAllyTurn()
    {
        Debug.Log("�Ʊ� �� ����");
        StartCoroutine(StartEnemyTurn());
        TileUIManager.Instance.HideActionMenu();
    }

    // ������ �ൿ �ڷ�ƾ
    // ���� �� ����
    private IEnumerator StartEnemyTurn()
    {
        currentTurn = TurnState.EnemyTurn;
        Debug.Log("���� �� ����");

        foreach (Unit enemy in enemyUnits)
        {
            yield return StartCoroutine(TakeTurn(enemy));  // ���� AI ����
            yield return new WaitForSeconds(0.5f);  // ������ �̵� �� �ణ�� ���
        }

        yield return new WaitForSeconds(1.0f);
        Debug.Log("���� �� ����");
        StartAllyTurn();

    }

    private IEnumerator TakeTurn(Unit unit)
    {
        Debug.Log($"{name}�� �� ����");

        Unit closestAlly = GridManager.Instance.FindClosestAlly(unit);
        if (closestAlly != null)
        {
            // 1. �̵� ������ Ÿ�� Ž��
            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(unit.currentTile, unit.stats.moveRange);

            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(true);
            }

            yield return new WaitForSeconds(0.5f);


            // 2. �Ʊ� �ֺ��� �̵� ������ Ÿ�� ����
            Tile targetTile = GridManager.Instance.FindValidTileNearTarget(closestAlly);

            if (targetTile)
            {
                // 3. ��� Ž�� �� �̵�
                List<Tile> path = GridManager.Instance.FindPath(unit.currentTile, targetTile);
                List<Tile> limitedPath = GridManager.Instance.GetLimitedPath(path, unit.stats.moveRange);

                if (limitedPath != null)
                {
                    yield return StartCoroutine(unit.MoveAlongPathCoroutine(limitedPath));  // ���� �̵�
                }
            }

            // ������ �̵��� �� ���̶���Ʈ�� ����
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(false);
            }
        }
    }

    // �Ʊ��� ���� �߰� �޼���
    public void AddAllyUnit(Unit unit) => allyUnits.Add(unit);
    public void AddEnemyUnit(Unit unit) => enemyUnits.Add(unit);

}
