using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private Tilemap obstacleTilemap;     // �̵� �Ұ����� Ÿ�ϸ�

    public static Vector2Int[] direction =
    {
        // �����¿�
        new Vector2Int(0, 1),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
        new Vector2Int(1, 0),

        // �밢��
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };

    public bool AStar(Vector2Int start, Vector2Int end, out List<Vector2> path)
    {
        List<ASNodeInt> openList = new List<ASNodeInt>();
        Dictionary<Vector2Int, bool> closeSet = new Dictionary<Vector2Int, bool>();
        path = new List<Vector2>();

        openList.Add(new ASNodeInt(start, null, 0, Heuristic(start, end)));

        while (openList.Count > 0)
        {
            ASNodeInt nextNode = NextNode(openList);
            openList.Remove(nextNode);
            closeSet.Add(nextNode.pos, true);

            if (Vector2Int.Distance(nextNode.pos, end) < 1)
            {
                ASNodeInt current = nextNode;
                while (current != null)
                {
                    path.Add(current.Centerpos);
                    current = current.parent;
                }
                path.Reverse();
                return true;
            }

            for (int i = 0; i < direction.Length; i++)
            {
                Vector2Int pos = nextNode.pos + direction[i];

                if (closeSet.ContainsKey(pos))
                    continue;

                if (!IsWalkable(pos))
                {
                    continue;
                }

                int g = (pos.x == nextNode.pos.x || pos.y == nextNode.pos.y) ?
                         nextNode.g + CostStraight :
                         nextNode.g + CostDiagonal;

                int h = Heuristic(pos, end);
                int f = g + h;

                ASNodeInt findNode = FindNode(openList, pos);
                if (findNode == null)
                    openList.Add(new ASNodeInt(pos, nextNode, g, h));
                else if (findNode.f > f)
                {
                    findNode.f = f;
                    findNode.g = g;
                    findNode.h = h;
                    findNode.parent = nextNode;
                }
            }
        }

        return false;
    }

    private bool IsWalkable(Vector2Int position)
    {
        Vector3 pos = new Vector3(position.x, position.y, 0);
        Vector3Int cellPosition = obstacleTilemap.WorldToCell(pos);

        // ��ֹ��� ���� ��쿡�� �̵� ����
        return !obstacleTilemap.HasTile(cellPosition);
    }

    public static int Heuristic(Vector2Int start, Vector2Int end)
    {
        return Mathf.Abs(end.x - start.x) + Mathf.Abs(end.y - start.y);
    }

    public static ASNodeInt NextNode(List<ASNodeInt> openList)
    {
        int curF = int.MaxValue;
        int curH = int.MaxValue;
        ASNodeInt minNode = null;

        for (int i = 0; i < openList.Count; i++)
        {
            if (curF > openList[i].f || (curF == openList[i].f && curH > openList[i].h))
            {
                curF = openList[i].f;
                curH = openList[i].h;
                minNode = openList[i];
            }
        }

        return minNode;
    }

    public static ASNodeInt FindNode(List<ASNodeInt> nodes, Vector2Int pos)
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].pos == pos)
                return nodes[i];
        }
        return null;
    }

    private static int CostStraight = 10;
    private static int CostDiagonal = 14;
}

public class ASNodeInt
{
    public Vector2Int pos;          // Ÿ���� ��ġ (��Ʈ��)
    public Vector2 Centerpos;       // Ÿ���� �߽���
    public ASNodeInt parent;         // �θ� ���
    public int f;                   // ��ü ���
    public int g;                   // ���� ���κ����� ���
    public int h;                   // ���� ���

    public ASNodeInt(Vector2Int pos, ASNodeInt parent, int g, int h)
    {
        this.pos = pos;
        this.Centerpos = new Vector2(pos.x + 0.5f, pos.y + 0.5f);  // �߽��� ���
        this.parent = parent;
        this.g = g;
        this.h = h;
        this.f = g + h;
    }
}
