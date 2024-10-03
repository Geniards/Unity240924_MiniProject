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
    // ���� ������ ������ Ŭ������ ����
    public UnitStats stats;      // ������ ���� ����
    public Team team;            // ������ ���� �� (enum)

    // ���� ���� ����
    public Tile currentTile;     // ������ ���� ��ġ�� Ÿ��
    public bool hasMoved = false;
    public bool isMoving = false;
    public bool hasEndedTurn = false;
    public Vector2Int unitCoordinates;

    // �̵� �� ��ġ�� Ÿ���� �����ϱ� ���� �ʵ�
    private Vector3 originalPosition;
    private Tile originalTile;

    // ������ ����ϱ� ���� ����
    private Color originalColor;

    // ������Ʈ
    protected Animator animator;
    protected SpriteRenderer spriteRenderer;

    [SerializeField] protected float moveSpeed = 2f;

    protected virtual void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animator.Play("Move_Left");

        // �ʱ� ��ġ�� Ÿ���� ����
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
        {//������ ���õǾ��� �� �̵� ���� ������ Ž��
            reachableTiles = GridManager.Instance.FindReachableTiles(currentTile, stats.moveRange);
        }

        ////Ž���� Ÿ���� ���̶���Ʈ
        //foreach (var tile in reachableTiles)
        //{
        //    tile.UpdateTileState(Tile.TileState.Selected);
        //}

        return reachableTiles;
    }

    // ������ Ÿ�Ϸ� �̵���Ű�� �޼���
    public void MoveToTile(Tile targetTile)
    {
        if (!isMoving)
        {
            // �̵� �� ��ġ ����
            originalPosition = transform.position;
            originalTile = currentTile;

            if (currentTile == targetTile && this.team == Team.Ally)
            {
                Debug.Log("�ڱ� �ڽ��� Ÿ���� Ŭ���߽��ϴ�. �̵��� �����մϴ�.");
                hasMoved = true;  // �̵� �Ϸ�� ó��
                TurnManager.Instance.ChangeState(TurnManager.TurnState.ActionUISelect);
                return;
            }

            List<Tile> path = GridManager.Instance.FindPath(currentTile, targetTile);
            if (path != null)
                StartCoroutine(MoveAlongPathCoroutine(path));

        }
    }

    // ������ ���� ��ġ�� �ǵ����� �޼���
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

    // �̵� �ڷ�ƾ (�ִϸ��̼� ����)
    public IEnumerator MoveAlongPathCoroutine(List<Tile> path)
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

        // �� �Ŵ������� �̵��� �Ϸ�Ǿ����� �˸�
        if (this.team == Team.Ally)
        {
            TurnManager.Instance.ChangeState(TurnManager.TurnState.ActionUISelect);
        }
    }

    // ���� ����Ǹ� ���� �ʱ�ȭ
    public void ResetMovement()
    {
        hasMoved = false;
    }

    // �̵� ���⿡ ���� �ִϸ��̼��� ��ȯ�ϴ� �޼���
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

        int damage = Mathf.Max(0, stats.atk - target.stats.def);
        target.TakeDamage(damage);

        // ���� �� �⺻ ���·� ����
        animator.Play("Move_Left");

        // ���� ����� �Ϸ�� �Ŀ� �� ����
        yield return new WaitForSeconds(0.5f);  // ���� �� ��� ���

        // �� �Ŵ������� ������ �������� �˸��� �� ����
        TurnManager.Instance.ChangeState(TurnManager.TurnState.EndTurn);
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
        Debug.Log($"TakeDamage");
        stats.hp -= damage;

        // �ǰ� �ִϸ��̼� ���
        StartCoroutine(PlayDamageAnimation());

        if (stats.hp <= 0)
        {
            Die();
        }
    }

    // �ǰ� �ִϸ��̼� ��� �ڷ�ƾ
    private IEnumerator PlayDamageAnimation()
    {
        animator.Play("Damage");

        StartCoroutine(FlashRed());

        // �ǰ� �ִϸ��̼��� ���� ������ ���
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.Play("Move_Left");
    }

    // ���������� ���ϰ� �ٽ� ���� ������ ���ƿ��� �ڷ�ƾ
    private IEnumerator FlashRed()
    {
        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = Color.white;          

        yield return new WaitForSeconds(0.2f);     

        spriteRenderer.color = originalColor;      
    }

    // ��� ó��
    private void Die()
    {
        if (currentTile != null)
        {
            currentTile.RemoveUnit();  // ������ ����� �� Ÿ�Ͽ��� ���� ���� ����
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

    // ������ ���� ������ �� ȣ�� (���� ��Ӱ�)
    public void EndTurn()
    {
        hasEndedTurn = true;
        StartCoroutine(DarkenColor());
    }

    // ������ ���� ������ �� ȣ�� (���� ���)
    public void StartTurn()
    {
        hasEndedTurn = false;
        StartCoroutine(RestoreOriginalColor());
    }

    // ������ ��Ӱ� ����� �ڷ�ƾ
    private IEnumerator DarkenColor()
    {
        Color darkColor = originalColor * 0.8f;
        spriteRenderer.color = darkColor;

        yield return null;
    }

    // ���� ������ �����ϴ� �ڷ�ƾ
    private IEnumerator RestoreOriginalColor()
    {
        spriteRenderer.color = originalColor;

        yield return null;
    }
}
