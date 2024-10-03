using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using static TurnManager;

public enum Team{ Ally,Enemy, TEAM_MAX }

[System.Serializable]
public class UnitStats
{
    public string unitName; 
    public int atk; 
    public int def;
    public int moveRange;
    public int attackRange;
    public int hp;        
    public int maxHp;        
    public int mana;        
    public int maxMana;        
}

public class Unit : MonoBehaviour
{
    // 유닛 스탯을 별도의 클래스로 관리
    public UnitStats stats;      // 유닛의 스탯 정보
    public Team team;            // 유닛이 속한 팀 (enum)

    // 유닛 상태 관리
    public Tile currentTile;     // 유닛이 현재 위치한 타일
    public bool hasMoved = false;
    public bool isMoving = false;
    public bool hasEndedTurn = false;
    public Vector2Int unitCoordinates;

    // 이동 전 위치와 타일을 저장하기 위한 필드
    private Vector3 originalPosition;
    private Tile originalTile;

    // 색상을 기억하기 위한 변수
    private Color originalColor;

    // 컴포넌트
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    [SerializeField] protected float moveSpeed = 2f;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animator.Play("Move_Left");

        // 초기 위치와 타일을 저장
        originalColor = spriteRenderer.color;
        originalPosition = transform.position;
        originalTile = currentTile;
    }

    public List<Tile> OnUnitSelected(Unit unit = null)
    {
        List<Tile> reachableTiles;
        if (unit)
        {
            reachableTiles = GridManager.Instance.FindReachableTiles(unit.currentTile, unit.stats.moveRange);
        }
        else
        {//유닛이 선택되었을 때 이동 가능 범위를 탐지
            reachableTiles = GridManager.Instance.FindReachableTiles(currentTile, stats.moveRange);
        }

        ////탐지된 타일을 하이라이트
        //foreach (var tile in reachableTiles)
        //{
        //    tile.UpdateTileState(Tile.TileState.Selected);
        //}

        return reachableTiles;
    }

    // 유닛을 타일로 이동시키는 메서드
    public void MoveToTile(Tile targetTile)
    {
        if (!isMoving)
        {
            // 이동 전 위치 저장
            originalPosition = transform.position;
            originalTile = currentTile;

            if (currentTile == targetTile && this.team == Team.Ally)
            {
                Debug.Log("자기 자신의 타일을 클릭했습니다. 이동을 생략합니다.");
                hasMoved = true;  // 이동 완료로 처리
                TurnManager.Instance.ChangeState(TurnManager.TurnState.ActionUISelect);
                return;
            }

            List<Tile> path = GridManager.Instance.FindPath(currentTile, targetTile);
            if (path != null)
                StartCoroutine(MoveAlongPathCoroutine(path));

        }
    }

    // 유닛을 원래 위치로 되돌리는 메서드
    public void CancelMove()
    {
        if (!isMoving)
        {
            StopAllCoroutines();
            transform.position = originalPosition;
            currentTile.RemoveUnit(); 
            currentTile = originalTile;
            currentTile.PlaceUnit(gameObject);
            isMoving = false;
            hasMoved = false;
            TileUIManager.Instance.HideActionMenu();
        }
    }

    public void MoveAlongPath(List<Tile> path)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveAlongPathCoroutine(path));
        }
    }

    // 이동 코루틴 (애니메이션 포함)
    public IEnumerator MoveAlongPathCoroutine(List<Tile> path)
    {
        isMoving = true;

        foreach (Tile tile in path)
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = new Vector3(tile.coordinates.x + 0.5f, tile.coordinates.y + 0.5f, 0);

            // 애니메이션 설정
            SetMoveAnimation(targetPosition - startPosition);

            float elapsedTime = 0f;
            float journeyLength = Vector3.Distance(startPosition, targetPosition);

            while (elapsedTime < journeyLength / moveSpeed)
            {
                transform.position = Vector3.Lerp(startPosition, targetPosition, (elapsedTime * moveSpeed) / journeyLength);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = targetPosition;
        }

        currentTile.RemoveUnit();
        currentTile = path[path.Count - 1];
        currentTile.PlaceUnit(this.gameObject);


        isMoving = false;

        // 턴 매니저에게 이동이 완료되었음을 알림
        if (this.team == Team.Ally)
        {
            TurnManager.Instance.ChangeState(TurnManager.TurnState.ActionUISelect);
        }
    }

    // 턴이 종료되면 상태 초기화
    public void ResetMovement()
    {
        hasMoved = false;
    }

    // 이동 방향에 따라 애니메이션을 전환하는 메서드
    protected void SetMoveAnimation(Vector3 direction)
    {
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            spriteRenderer.flipX = direction.x > 0;
            animator.Play("Move_Left");
        }
        else
        {
            animator.Play(direction.y > 0 ? "Move_Up" : "Move_Down");
        }
    }

    // 공격 처리
    public void Attack(Unit target)
    {
        StartCoroutine(AttackRoutine(target));
    }

    // 공격 코루틴
    private IEnumerator AttackRoutine(Unit target)
    {
        // 공격 방향에 따라 애니메이션 전환
        Vector3 direction = target.transform.position - transform.position;
        SetAttackAnimation(direction);

        // 공격 애니메이션 대기 시간
        yield return new WaitForSeconds(0.5f);

        int damage = Mathf.Max(0, stats.atk - target.stats.def);
        target.TakeDamage(damage);

        // 공격 후 기본 상태로 복귀
        animator.Play("Move_Left");

        // 공격 모션이 완료된 후에 턴 종료
        yield return new WaitForSeconds(0.5f);  // 공격 후 잠시 대기

        // 턴 매니저에게 공격이 끝났음을 알리고 턴 종료
        TurnManager.Instance.ChangeState(TurnManager.TurnState.EndTurn);
    }

    // 공격 방향에 따라 애니메이션을 전환하는 메서드
    private void SetAttackAnimation(Vector3 direction)
    {
        // 좌우 공격 처리 (오른쪽은 Flip으로 처리)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                // 오른쪽으로 공격 (Flip 사용)
                spriteRenderer.flipX = true;
                animator.Play("Atk_Left");
            }
            else
            {
                // 왼쪽으로 공격
                spriteRenderer.flipX = false;
                animator.Play("Atk_Left");
            }
        }
        // 상하 공격 처리
        else
        {
            if (direction.y > 0)
            {
                animator.Play("Atk_Up");
            }
            else
            {
                animator.Play("Atk_Down");
            }
        }
    }

    // 데미지를 입었을 때 처리
    public void TakeDamage(int damage)
    {
        Debug.Log($"TakeDamage");
        stats.hp -= damage;

        // 피격 애니메이션 재생
        StartCoroutine(PlayDamageAnimation());

        if (stats.hp <= 0)
        {
            Die();
        }
    }

    // 피격 애니메이션 재생 코루틴
    private IEnumerator PlayDamageAnimation()
    {
        animator.Play("Damage");

        StartCoroutine(FlashRed());

        // 피격 애니메이션이 끝날 때까지 대기
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.Play("Move_Left");
    }

    // 빨간색으로 변하고 다시 원래 색으로 돌아오는 코루틴
    private IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;          

        yield return new WaitForSeconds(0.2f);     

        spriteRenderer.color = originalColor;      
    }

    // 사망 처리
    private void Die()
    {
        if (currentTile != null)
        {
            currentTile.RemoveUnit();  // 유닛이 사망할 때 타일에서 유닛 정보 제거
        }

        if (team == Team.Ally)
        {
            TurnManager.Instance.RemoveAllyUnit(this);
        }
        else if (team == Team.Enemy)
        {
            TurnManager.Instance.RemoveEnemyUnit(this);
        }


        Destroy(gameObject);
    }

    // 유닛이 턴을 종료할 때 호출 (색상 어둡게)
    public void EndTurn()
    {
        hasEndedTurn = true;
        StartCoroutine(DarkenColor());
    }

    // 유닛의 턴이 시작할 때 호출 (색상 밝게)
    public void StartTurn()
    {
        hasEndedTurn = false;
        StartCoroutine(RestoreOriginalColor());
    }

    // 색상을 어둡게 만드는 코루틴
    private IEnumerator DarkenColor()
    {
        Color darkColor = originalColor * 0.8f;
        spriteRenderer.color = darkColor;

        yield return null;
    }

    // 원래 색으로 복구하는 코루틴
    private IEnumerator RestoreOriginalColor()
    {
        spriteRenderer.color = originalColor;

        yield return null;
    }
}
