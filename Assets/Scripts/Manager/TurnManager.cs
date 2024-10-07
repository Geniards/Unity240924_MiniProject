using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance;

    // FSM�� ���� ���� ������ ����
    public enum TurnState
    {
        UnitSelection,  // ���� ����
        UnitMove,       // ���� �̵�
        UnitAttack,     // ���� ����
        ActionUISelect, // UIâ ����
        EndTurn         // �� ����
    }

    public List<Unit> allyUnits = new List<Unit>();
    public List<Unit> enemyUnits = new List<Unit>();

    public TurnState currentState;   // ���� FSM ����
    private Unit selectedUnit;       // ���� ���õ� ����
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
        // ���� ���� ���� ���·� ����
        StartTurn(TurnState.UnitSelection);
    }

    // ���¿� ���� ���� ����
    public void StartTurn(TurnState initialState)
    {
        currentState = initialState;
        Debug.Log("Current Turn State: " + currentState.ToString());

        if (currentState == TurnState.UnitSelection)
        {
            // ��� �Ʊ� ������ �̵��� �ʱ�ȭ
            foreach (var unit in allyUnits)
            {
                unit.ResetMovement();
                unit.StartTurn();
            }

            allyUnitsFinishedMovement = 0;  // �̵� �Ϸ� ���� �ʱ�ȭ
        }
    }

    // ���� ��ȯ �޼���
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
                    StartCoroutine(StartEnemyTurn());  // ���� �� ����
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

    // ���� ���� ���� ó��
    public void SelectUnit()
    {
        Debug.Log("ĳ���� ����");
        // ������ Ŭ���Ͽ� �����ϴ� ���� ���� (UI���� ������ �����ϴ� �帧)
    }

    // ���� �̵� ���� ó��
    public void MoveUnit()
    {
        Debug.Log("������ ���� �̵�");
        // ���õ� ������ �̵� ���� Ÿ���� ǥ���ϰ� �̵� ó��
        if (selectedUnit != null)
        {
            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(selectedUnit.currentTile, selectedUnit.stats.moveRange);
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(true);
            }
        }
    }

    // ���� ���� ���� ó��
    public void AttackWithUnit()
    {
        Debug.Log("������ ���� ����");
        // ���õ� ������ ���� ������ Ÿ���� ǥ���ϰ� ���� ó��
        List<Tile> attackableTiles = GridManager.Instance.FindAttackableTiles(selectedUnit.currentTile, selectedUnit.stats.attackRange);
        foreach (Tile tile in attackableTiles)
        {
            tile.UpdateTileState(Tile.TileState.Attackable);
        }
    }

    // ������ ���� �Ϸ����� �� ȣ��
    public void NotifyUnitMovementFinished(Unit unit)
    {
        allyUnitsFinishedMovement++;
        Debug.Log($"���� {unit.stats.unitName} �̵� �Ϸ�: {allyUnitsFinishedMovement} / {allyUnits.Count}");

        selectedUnit.EndTurn();
        unit.SaveOriginalPosition();  // ���� ��ġ ����

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

    // �Ʊ� ������ ���� ��� �������� Ȯ���ϰ� �� ���� ó��
    private void CheckAllyTurnCompletion()
    {
        if (AreAllAlliesFinished())
        {
            Debug.Log("��� �Ʊ��� ���� ����Ǿ����ϴ�. ������ ���� �����մϴ�.");
            allyUnitsFinishedMovement = 0; // �Ϸ�� �Ʊ� ���� �� �ʱ�ȭ

            // �Ʊ� ���� ������ ��� �Ʊ� ������ ���¸� �ʱ�ȭ
            foreach (var unit in allyUnits)
            {
                unit.ResetTurnState();
            }

            StartCoroutine(StartEnemyTurn()); // ������ �� ����
        }
        else
        {
            //NotifyUnitMovementFinished(GetSelectedUnit());
            Debug.Log("���� ���� �Ϸ���� �ʾҽ��ϴ�.");
            ChangeState(TurnState.UnitSelection);
        }
    }

    // ���� �� ó��
    private IEnumerator StartEnemyTurn()
    {
        // ���� ���� �̹� ���� ���̸� �ߺ����� �������� ����
        if (enemyTurnInProgress) yield break;

        Debug.Log("Enemy turn started.");
        enemyTurnInProgress = true;
        enemyUnitsFinishedMovement = 0;

        foreach (Unit enemy in enemyUnits)
        {
            yield return StartCoroutine(TakeTurn(enemy));
            yield return new WaitForSeconds(0.5f);  // ���� �̵� �� ��� ���
        }

        Debug.Log("���� �� ����");
        enemyTurnInProgress = false;
        StartTurn(TurnState.UnitSelection);  // ���� ���� ������ �ٽ� �Ʊ� ���� ���� ���·�
    }

    private IEnumerator TakeTurn(Unit unit)
    {
        // ������ �ı��Ǿ����� ���� Ȯ��
        if (unit == null)
        {
            Debug.Log("������ �̹� ���ŵǾ����ϴ�. ���� �ǳʶݴϴ�.");
            yield break;
        }

        Debug.Log($"{name}�� �� ����");

        // 1. ���� ���� ���� ���� �Ʊ��� �ִ��� ���� Ȯ��
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

        // 2. ���� ���� ������ �Ʊ��� ������ �̵����� �ʰ� �ٷ� ����
        if (closestAllyInRange != null)
        {
            Debug.Log("������ ���� ���� �ȿ� �Ʊ��� �߰�, ������ �õ��մϴ�.");

            // ���� ���� ���̶���Ʈ
            foreach (Tile tile in attackableTiles)
            {
                tile.UpdateTileState(Tile.TileState.Attackable);
            }

            // ��� ��� �� ����
            yield return new WaitForSeconds(0.5f);
            unit.Attack(closestAllyInRange);

            // �� ����
            yield return new WaitForSeconds(0.5f);
            unit.EndTurn();
            enemyUnitsFinishedMovement++;
            Debug.Log($"{unit.name}�� �� ����");
            yield break; // ���� �� ���� �ѱ�
        }

        // 3. ���� ���� �ȿ� �Ʊ��� ������ �̵�
        Unit closestAlly = GridManager.Instance.FindClosestAlly(unit);
        if (closestAlly != null)
        {
            // �̵� ������ Ÿ�� Ž��
            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(unit.currentTile, unit.stats.moveRange);

            // �̵� ����Ÿ�� ���̶���Ʈ On
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(true);
            }

            yield return new WaitForSeconds(0.5f);

            // ����� �Ʊ���ó Ÿ�� Ž��
            Tile closestTile = GridManager.Instance.FindClosestValidTile(unit, closestAlly);
            if (closestTile != null)
            {
                // ��� Ž�� �� �̵�
                List<Tile> path = GridManager.Instance.FindPath(unit.currentTile, closestTile);
                List<Tile> limitedPath = GridManager.Instance.GetLimitedPath(path, unit.stats.moveRange);

                if (limitedPath != null && limitedPath.Count > 0)
                {
                    yield return StartCoroutine(unit.MoveAlongPathCoroutine(limitedPath));
                }
            }
            else
            {
                Debug.Log("�ֺ��� �̵� ������ Ÿ���� �����ϴ�. �� ����.");
                yield break;  // �ֺ��� �̵� ������ Ÿ���� ������ �� ����
            }

            // ������ �̵��� �� ���̶���Ʈ�� ����
            foreach (Tile tile in reachableTiles)
            {
                tile.SetReachable(false);
            }

            // 4. �Ʊ��� ���� ���� ���� ���Դ��� Ȯ��
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

            // ���� ���� ������ �Ʊ��� ������ ����
            if (closestAllyInRange != null)
            {
                Debug.Log("������ �̵� �� ���� ���� �ȿ� �Ʊ��� �߰�, ������ �õ��մϴ�.");

                // ���� ���� Ÿ�� ���̶���Ʈ
                foreach (Tile tile in attackableTiles)
                {
                    tile.UpdateTileState(Tile.TileState.Attackable);
                }

                // 5. ���� ��� �� ����
                yield return new WaitForSeconds(0.5f);

                unit.Attack(closestAllyInRange);  // ������ �Ʊ��� ����
                yield return new WaitForSeconds(0.5f);
            }

            // �������� �ʴ��� ���̶���Ʈ�� ������ ����
            foreach (Tile tile in attackableTiles)
            {
                tile.RestoreOriginalState();
            }
        }



        enemyUnitsFinishedMovement++;
        Debug.Log($"{unit.name}�� �� ����");
    }

    // ���õ� ���� ����
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

    // �Ʊ� ���� ����
    public void RemoveAllyUnit(Unit unit)
    {
        if (allyUnits.Contains(unit))
        {
            allyUnits.Remove(unit);
            Debug.Log($"{unit.stats.unitName} ���: �Ʊ� ����Ʈ���� ����");
        }

        CheckTurnEnd();
    }

    // ���� ���� ����
    public void RemoveEnemyUnit(Unit unit)
    {
        if (enemyUnits.Contains(unit))
        {
            enemyUnits.Remove(unit);
            Debug.Log($"{unit.stats.unitName} ���: ���� ����Ʈ���� ����");
        }

        CheckTurnEnd();
    }

    // ��� �Ʊ�/������ �׾��� ��� ���� �����ϴ� �޼���
    private void CheckTurnEnd()
    {
        if (allyUnits.Count == 0)
        {
            Debug.Log("��� �Ʊ��� ����߽��ϴ�. ���� ����");
            // ���� ���� �Ǵ� �ٸ� ó��
        }
        else if (enemyUnits.Count == 0)
        {
            Debug.Log("��� ������ ����߽��ϴ�. �¸�!");
            // �¸� �Ǵ� ���� ���������� �̵�
        }
        else
            ChangeState(TurnState.UnitSelection);
    }
}
