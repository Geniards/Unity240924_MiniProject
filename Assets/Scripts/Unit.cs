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

    private void Start()
    {
        // �ִϸ����Ϳ� ��������Ʈ ������ �ʱ�ȭ
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // �⺻ �ִϸ��̼� ���� (���� �̵�)
        animator.Play("Move_Left");
    }

    // ������ Ÿ�Ϸ� �̵���Ű�� �޼���
    public void MoveToTile(Tile targetTile)
    {
        if (!isMoving)
        {
            StartCoroutine(MoveRoutine(targetTile));
        }
    }

    // �̵� �ڷ�ƾ (�ִϸ��̼� ����)
    private IEnumerator MoveRoutine(Tile targetTile)
    {
        isMoving = true;

        // ���� Ÿ�Ͽ��� ���� ����
        currentTile.RemoveUnit();

        Vector3 startPosition = transform.position;
        Vector3 targetPosition = new Vector3(targetTile.coordinates.x, targetTile.coordinates.y, 0);

        // �̵� ���⿡ ���� �ִϸ��̼� ����
        SetMoveAnimation(targetPosition - startPosition);

        float elapsedTime = 0;
        while (elapsedTime < 1f)
        {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            elapsedTime += Time.deltaTime * stats.moveRange; // �̵� �ӵ��� ������ �̵��¿� ���� ����
            yield return null;
        }

        // �̵� �Ϸ� �� ��ġ ����
        transform.position = targetPosition;
        targetTile.PlaceUnit(this.gameObject);
        currentTile = targetTile;

        // �̵� �Ϸ� �� �⺻ �ִϸ��̼�(���� �̵�)���� ����
        animator.Play("Move_Left");

        isMoving = false;
    }

    // �̵� ���⿡ ���� �ִϸ��̼��� ��ȯ�ϴ� �޼���
    private void SetMoveAnimation(Vector3 direction)
    {
        // �¿� �̵� ó�� (�������� Flip���� ó��)
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0)
            {
                // ���������� �̵� (Flip ���)
                spriteRenderer.flipX = true;
                animator.Play("Move_Left"); // ���� �ִϸ��̼��� Flip
            }
            else
            {
                // �������� �̵�
                spriteRenderer.flipX = false;
                animator.Play("Move_Left");
            }
        }
        // ���� �̵� ó��
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

        yield return new WaitForSeconds(0.5f); // ���� �ִϸ��̼� ��� �ð�

        // Ÿ�ٿ��� ������ ����
        int damage = Mathf.Max(0, stats.attackPower - target.stats.defensePower); // ���� ��� �� ������ ����
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
        stats.mana -= damage;  // ü�� ��� ������ ��ü�� ����

        if (stats.mana <= 0)
        {
            Die();
        }
    }

    // ��� ó��
    private void Die()
    {
        // ��� ó�� ����
        Destroy(gameObject);  // ���� ����
    }
}
