using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum TurnState { PLAYERTURN, ENEMYTURN, TURNSTATE_MAX }

    public static TurnManager instance;
    public TurnState currentTurn;

    public List<PlayerUnit> playerUnits; // �Ʊ� ���� ����Ʈ
    public List<EnemyUnit> enemyUnits; // ���� ���� ����Ʈ

    // �׽�Ʈ��.
    [SerializeField] private GameObject testButton;

    private void Awake()
    {
        instance = this;
        Initialize();
    }

    private void Initialize()
    {
        playerUnits = new List<PlayerUnit>();
        enemyUnits = new List<EnemyUnit>();
    }

    private void Start()
    {
        currentTurn = TurnState.PLAYERTURN;
        testButton.SetActive(true);

        Debug.Log("�÷��̾� ��");
    }

    // �� ���� ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    public void EndTurn()
    {
        if (currentTurn == TurnState.PLAYERTURN)
        {
            StartCoroutine(StartEnemyTurn());
        }
    }

    // ���� �� ����
    private IEnumerator StartEnemyTurn()
    {
        testButton.SetActive(false);
        currentTurn = TurnState.ENEMYTURN;
        Debug.Log("���� ��");

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            EnemyUnit enemyUnit = enemyUnits[i];
            enemyUnit.StartAI();

            // �����̴� ���̶�� �������� ���� ����
            while(enemyUnit.GetIsMoving())
            {
                yield return null;
            }
        }

        currentTurn = TurnState.PLAYERTURN;
        Debug.Log("�÷��̾� ��");

        testButton.SetActive(true);
    }
}
