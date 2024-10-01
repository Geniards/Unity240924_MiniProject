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
    // ���� ������ ������ Ŭ������ ����
    public UnitStats stats;      // ������ ���� ����
    public Team team;            // ������ ���� �� (enum)

    // ���� ���� ����
    public Tile currentTile;     // ������ ���� ��ġ�� Ÿ��
    private bool isMoving = false; // ������ �̵� ������ ����
    public Vector2Int unitCoordinates;

    // ������Ʈ
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
    //     ������ ���õǾ��� �� �̵� ���� ������ Ž��
    //    List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(currentTile, stats.moveRange);

    //     Ž���� Ÿ���� ���̶���Ʈ
    //    foreach (var tile in reachableTiles)
    //    {
    //        tile.UpdateTileState(Tile.TileState.Selected);
    //    }
    //}

    //// ������ Ÿ�Ϸ� �̵���Ű�� �޼���
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

    // �̵� �ڷ�ƾ (�ִϸ��̼� ����)
    private IEnumerator MoveAlongPathCoroutine(List<Tile> path)
    {
        isMoving = true;

        foreach (Tile tile in path)
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = new Vector3(tile.coordinates.x + 0.5f, tile.coordinates.y + 0.5f, 0);

            // �ִϸ��̼� ����
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

    // �̵� ���⿡ ���� �ִϸ��̼��� ��ȯ�ϴ� �޼���
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

    // ���� ó��
    public void Attack(Unit target)
    {
        StartCoroutine(AttackRoutine(target));
    }

    // ���� �ڷ�ƾ
    private IEnumerator AttackRoutine(Unit target)
    {
        // ���� ���⿡ ���� �ִϸ��̼� ��ȯ
        Vector3 direction = target.transform.position - transform.position;
        SetAttackAnimation(direction);

        // ���� �ִϸ��̼� ��� �ð�
        yield return new WaitForSeconds(0.5f);

        int damage = Mathf.Max(0, stats.attackPower - target.stats.defensePower);
        target.TakeDamage(damage);

        // ���� �� �⺻ ���·� ����
        animator.Play("Move_Left");
    }

    // ���� ���⿡ ���� �ִϸ��̼��� ��ȯ�ϴ� �޼���
    private void SetAttackAnimation(Vector3 direction)
    {
        // �¿� ���� ó�� (�������� Flip���� ó��)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                // ���������� ���� (Flip ���)
                spriteRenderer.flipX = true;
                animator.Play("Atk_Left");
            }
            else
            {
                // �������� ����
                spriteRenderer.flipX = false;
                animator.Play("Atk_Left");
            }
        }
        // ���� ���� ó��
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

    // �������� �Ծ��� �� ó��
    public void TakeDamage(int damage)
    {
        stats.hp -= damage;

        if (stats.hp <= 0)
        {
            Die();
        }
    }

    // ��� ó��
    private void Die()
    {
        Destroy(gameObject);
    }
}
