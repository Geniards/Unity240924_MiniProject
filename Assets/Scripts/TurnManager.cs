using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public enum TurnState { PLAYERTURN, ENEMYTURN, TURNSTATE_MAX }

    public static TurnManager instance;
    public TurnState currentTurn;

    public List<PlayerUnit> playerUnits; // 아군 유닛 리스트
    public List<EnemyUnit> enemyUnits; // 적군 유닛 리스트

    // 테스트용.
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

        Debug.Log("플레이어 턴");
    }

    // 턴 종료 버튼 클릭 시 호출되는 메서드
    public void EndTurn()
    {
        if (currentTurn == TurnState.PLAYERTURN)
        {
            StartCoroutine(StartEnemyTurn());
        }
    }

    // 적군 턴 시작
    private IEnumerator StartEnemyTurn()
    {
        testButton.SetActive(false);
        currentTurn = TurnState.ENEMYTURN;
        Debug.Log("적군 턴");

        for (int i = 0; i < enemyUnits.Count; i++)
        {
            EnemyUnit enemyUnit = enemyUnits[i];
            enemyUnit.StartAI();

            // 움직이는 중이라면 한프레임 쉬고 동작
            while(enemyUnit.GetIsMoving())
            {
                yield return null;
            }
        }

        currentTurn = TurnState.PLAYERTURN;
        Debug.Log("플레이어 턴");

        testButton.SetActive(true);
    }
}
