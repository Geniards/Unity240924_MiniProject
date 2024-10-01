using System.Collections;
using UnityEditor;
using UnityEngine;

public enum Team{ Ally,Enemy, TEAM_MAX }

[System.Serializable]
public class UnitStats
{
    public string unitName; 
    public int attackPower; 
    public int defensePower;
    public int moveRange;   
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
    private bool isMoving = false; // 유닛이 이동 중인지 여부
    public Vector2Int unitCoordinates;

    // 컴포넌트
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // 애니메이터와 스프라이트 렌더러 초기화
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 기본 애니메이션 설정 (왼쪽 이동)
        animator.Play("Move_Left");
    }

    // 유닛을 타일로 이동시키는 메서드
    public void MoveToTile(Tile targetTile)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveRoutine(targetTile));
        }
    }

    // 이동 코루틴 (애니메이션 포함)
    private IEnumerator MoveRoutine(Tile targetTile)
    {
        isMoving = true;

        // 현재 타일에서 유닛 제거
        currentTile.RemoveUnit();

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(targetTile.coordinates.x, targetTile.coordinates.y, 0);

        // 이동 방향에 따른 애니메이션 설정
        SetMoveAnimation(targetPosition - startPosition);

        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * stats.moveRange; // 이동 속도는 유닛의 이동력에 따라 결정
            yield return null;
        }

        // 이동 완료 후 위치 고정
        transform.position = targetPosition;
        targetTile.PlaceUnit(this.gameObject);
        currentTile = targetTile;

        // 이동 완료 후 기본 애니메이션(왼쪽 이동)으로 복귀
        animator.Play("Move_Left");

        isMoving = false;
    }

    // 이동 방향에 따라 애니메이션을 전환하는 메서드
    private void SetMoveAnimation(Vector3 direction)
    {
        // 좌우 이동 처리 (오른쪽은 Flip으로 처리)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                // 오른쪽으로 이동 (Flip 사용)
                spriteRenderer.flipX = true;
                animator.Play("Move_Left"); // 왼쪽 애니메이션을 Flip
            }
            else
            {
                // 왼쪽으로 이동
                spriteRenderer.flipX = false;
                animator.Play("Move_Left");
            }
        }
        // 상하 이동 처리
        else
        {
            if (direction.y > 0)
            {
                animator.Play("Move_Up");
            }
            else
            {
                animator.Play("Move_Down");
            }
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

        yield return new WaitForSeconds(0.5f); // 공격 애니메이션 대기 시간

        // 타겟에게 데미지 적용
        int damage = Mathf.Max(0, stats.attackPower - target.stats.defensePower); // 방어력 계산 후 데미지 적용
        target.TakeDamage(damage);

        // 공격 후 기본 상태로 복귀
        animator.Play("Move_Left");
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
        stats.mana -= damage;  // 체력 대신 마나로 대체한 예시

        if (stats.mana <= 0)
        {
            Die();
        }
    }

    // 사망 처리
    private void Die()
    {
        // 사망 처리 로직
        Destroy(gameObject);  // 유닛 제거
    }
}
