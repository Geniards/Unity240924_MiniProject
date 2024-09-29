using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Unit : MonoBehaviour
{
    [Header("Unit Setting")]
    [SerializeField] private string name;
    [SerializeField] private string className;
    
    [SerializeField] private int level;

    [SerializeField] private int Hp;
    [SerializeField] private int maxHp;

    [SerializeField] private int moveRange;
    [SerializeField] private int actionPoint;

    // �̵��Ϸ� �� �ൿâ ��� �̺�Ʈ
    public event Action OnMoveComplete;

    [Header("�̵����� ����")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Tilemap moveRangeTilemap;
    [SerializeField] private TileBase moveRangeTile;    // �̵������� Ÿ���� ǥ��

    private HashSet<Vector3Int> visitedTiles = new HashSet<Vector3Int>(); // �湮�� Ÿ�� ���
    private List<Vector3Int> validMoveTiles = new List<Vector3Int>(); // �̵� ������ Ÿ�� ����Ʈ
    //private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>(); // ���� Ÿ�� ����

    private bool isMoveRangeDisplayed = false;

    // A* Setting
    [Header("A* Setting")]
    [SerializeField] private PathFinder pathFinder;
    private bool isMoving = false;


    private void Awake()
    {
        name = "����";
        className = "����";
        level = 1;
        maxHp = 100;
        Hp = maxHp;
        moveRange = 4;
        actionPoint = 1;

        // �̵����ɹ��� : �ʺ� �켱Ž��
        // ����ȭ

        // �ڷ�ƾ ���� �������� ������ ����

        pathFinder = GetComponent<PathFinder>(); // PathFinder ������Ʈ�� ������
    }

    public string GetStatusName()
    {
        return $"{name}\n{className}\t\t\t\t\tLV : {level}";
    }

    public void PathSearch(Vector3Int selectUnitPos)
    {
        if (isMoveRangeDisplayed)
        {
            ClearMoveRange();
            isMoveRangeDisplayed = false;
        }
        else
        {
            //DFS(selectUnitPos, moveRange);
            BFS(selectUnitPos, moveRange);
            HighlightMoveRange();
            isMoveRangeDisplayed = true;
        }
    }

    private void BFS(Vector3Int startPos, int maxMoveRange)
    {
        Queue<(Vector3Int, int)> queue = new Queue<(Vector3Int, int)>();
        queue.Enqueue((startPos, maxMoveRange)); // ���� ��ġ�� ���� �̵� ���� �߰�
        visitedTiles.Add(startPos);
        validMoveTiles.Add(startPos);

        // �� ���� Ž�� (��, ��, ��, ��)
        Vector3Int[] directions = new Vector3Int[]
        {
        new Vector3Int(+1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(0, +1, 0),
        new Vector3Int(0, -1, 0)
        };

        while (queue.Count > 0)
        {
            var (currentPos, remainingMoveRange) = queue.Dequeue();

            if (remainingMoveRange <= 0) continue;

            foreach (var direction in directions)
            {
                Vector3Int nextPos = currentPos + direction;

                if (tilemap.HasTile(nextPos) && !visitedTiles.Contains(nextPos))
                {
                    int tileCost = GetTileCost(nextPos);

                    // ��ֹ��� ��� Ž�� X
                    if (tileCost == int.MaxValue) continue; 

                    if (remainingMoveRange - tileCost >= 0)
                    {
                        visitedTiles.Add(nextPos);
                        validMoveTiles.Add(nextPos);

                        queue.Enqueue((nextPos, remainingMoveRange - tileCost));
                    }
                }
            }
        }
    }

    // Ÿ�Ͽ� ���� �����
    private int GetTileCost(Vector3Int tilePos)
    {
        TileBase tile = tilemap.GetTile(tilePos);
        TileBase obstacleTile = obstacleTilemap.GetTile(tilePos);

        if (obstacleTile)
        {
            return int.MaxValue; // ��ֹ��� ��� ����� �������� ����
        }

        if (tile && (tile.name.Contains("Mountain") || tile.name.Contains("River")))
        {
            return 2;
        }

        return 1;
    }

    // �̵� ������ Ÿ���� �ð������� ǥ��
    private void HighlightMoveRange()
    {
        foreach (var tilePos in validMoveTiles)
        {
            moveRangeTilemap.SetTile(tilePos, moveRangeTile);
        }
    }

    // �̵� ������ �ʱ�ȭ, Ÿ���� ������� �ǵ���
    private void ClearMoveRange()
    {
        foreach (var tilePos in validMoveTiles)
        {
            moveRangeTilemap.SetTile(tilePos, null);
        }

        validMoveTiles.Clear();  // �̵� ������ Ÿ�� ��� �ʱ�ȭ
        visitedTiles.Clear();    // �湮�� Ÿ�� ��� �ʱ�ȭ
    }

    // A* �˰������� �̵�
    public void MoveTo(Vector3Int targetPos)
    {
        if (validMoveTiles.Contains(targetPos) && !isMoving)
        {
            List<Vector2> path = null;
            Vector2Int start = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            Vector2Int end = new Vector2Int(targetPos.x, targetPos.y);

            // A* ��� Ž��
            if (pathFinder.AStar(start, end, out path))
            {
                StartCoroutine(MoveAlongPath(path));
            }
            else
            {
                Debug.Log($"����");
            }
        }
    }

    // ������ A* ��θ� ���� �̵�
    private IEnumerator MoveAlongPath(List<Vector2> path)
    {
        isMoving = true;

        foreach (Vector3 point in path)
        {
            Vector3 targetPos = new Vector3(point.x, point.y, 0);

            while (Vector3.Distance(transform.position, targetPos) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, Time.deltaTime * 3f);
                yield return null;
            }
        }

        // �̵��� ������ �̺�Ʈ �߻�
        isMoving = false;
        ClearMoveRange();
        OnMoveComplete?.Invoke();  // �̵� �Ϸ� �� �̺�Ʈ ȣ��
    }

    public List<Vector3Int> GetvalidMoveTiles() { return validMoveTiles; }

    // ActionPoint ��뿩�� (TurnManager���� ���)
    public void ResetActionPoint() { actionPoint = 1; }

    public int GetActionPoint() { return actionPoint; }

}
