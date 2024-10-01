using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

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

    [SerializeField] private float moveSpeed = 2f;

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animator.Play("Move_Left");
    }

    //public void OnUnitSelected()
    //{
    //     유닛이 선택되었을 때 이동 가능 범위를 탐지
    //    List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(currentTile, stats.moveRange);

    //     탐지된 타일을 하이라이트
    //    foreach (var tile in reachableTiles)
    //    {
    //        tile.UpdateTileState(Tile.TileState.Selected);
    //    }
    //}

    //// 유닛을 타일로 이동시키는 메서드
    //public void MoveToTile(Tile targetTile)
    //{
    //    if (!isMoving)
    //    {
    //        List<Tile> path = GridManager.Instance.FindPath(currentTile, targetTile);
    //        if(path != null)
    //            StartCoroutine(MoveAlongPath(path));
    //    }
    //}

    public void MoveAlongPath(List<Tile> path)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveAlongPathCoroutine(path));
        }
    }

    // 이동 코루틴 (애니메이션 포함)
    private IEnumerator MoveAlongPathCoroutine(List<Tile> path)
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
    }

    // 이동 방향에 따라 애니메이션을 전환하는 메서드
    private void SetMoveAnimation(Vector3 direction)
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

        int damage = Mathf.Max(0, stats.attackPower - target.stats.defensePower);
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
        stats.hp -= damage;

        if (stats.hp <= 0)
        {
            Die();
        }
    }

    // 사망 처리
    private void Die()
    {
        Destroy(gameObject);
    }
}
