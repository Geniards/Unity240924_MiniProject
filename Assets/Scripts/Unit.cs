using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

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

    [Header("�̵����� ����")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Tilemap moveRangeTilemap;
    [SerializeField] private TileBase moveRangeTile;    // �̵������� Ÿ���� ǥ��

    private HashSet<Vector3Int> visitedTiles = new HashSet<Vector3Int>(); // �湮�� Ÿ�� ���
    private List<Vector3Int> validMoveTiles = new List<Vector3Int>(); // �̵� ������ Ÿ�� ����Ʈ
    //private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>(); // ���� Ÿ�� ����

    private bool isMoveRangeDisplayed = false;

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

    private void DFS(Vector3Int currentPos, int remainingMoveRange)
    {
        // �����Ÿ��� 0 or �湮������ ����
        if (remainingMoveRange <= 0 || visitedTiles.Contains(currentPos)) return;

        visitedTiles.Add(currentPos);   // ���� Ÿ���� �湮����
        validMoveTiles.Add(currentPos); // �̵������� Ÿ�Ϸ� �߰�
        Debug.Log($"validMoveTiles {currentPos}");

        // ���� Ÿ�� ������ ����
        //if (!originalTiles.ContainsKey(currentPos))
        //{
        //    originalTiles[currentPos] = tilemap.GetTile(currentPos);
        //}

        // ���� Ÿ���� ���� �¿�� Ž���� �غ��Ѵ�.
        // �� ���� Ž�� (��, ��, ��, ��)
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(+1, 0, 0), 
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, +1, 0), 
            new Vector3Int(0, -1, 0) 
        };

        foreach (var direction in directions)
        {
            Vector3Int nextPos = currentPos + direction;

            // nextPos�� ��ȿ�� ��ġ���� �ľ�
            if(tilemap.HasTile(nextPos))
            {
                int tileCost = GetTileCost(nextPos);
                if (remainingMoveRange - tileCost >= 0)
                {
                    DFS(nextPos, remainingMoveRange - tileCost);
                }

            }
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

    public void ResetActionPoint() { actionPoint = 1; }

    public int GetActionPoint() { return actionPoint; }

}
