using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum CharaterTeam { ALLIANCE, ENEMY, CHARATERTEAM_MAX }

[Serializable]
public class Status
{
    public int Hp;
    public int ATK;
    public int DEF;
    public int MOV;
}


public class Character : MonoBehaviour
{
    public Status stat;
    public Tile currentStand;
    public List<Tile> prevMoveTile = new List<Tile>();
    protected List<Tile> moveableArea = new List<Tile>();
    protected Coroutine moveCoroutine;


    public void ShowMovementRange(Tile startTile)
    {
        List<Tile> reachableTiles = BFS(startTile);

        moveableArea.Clear();
        moveableArea = reachableTiles;

        foreach (Tile tile in moveableArea)
        {
            tile.SetSelected();
        }
    }

    public bool CheckMove(Tile tile)
    {
        if(moveableArea.Contains(tile))
        {
            prevMoveTile.Clear();
            while(tile)
            {
                prevMoveTile.Insert(0, tile);
                if (tile.tileInfo.prev != null)
                    tile = Map.Instance.GetTile(tile.tileInfo.prev.coord);
                else
                    tile = null;
            }

            StopCurrentMove();
            moveCoroutine = StartCoroutine(MoveToTile(prevMoveTile[0])); 
            return true;
        }
        return false;
    }

    public void StopCurrentMove()
    {
        if(moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        moveCoroutine = null;
    }

    private IEnumerator MoveToTile(Tile targetTile)
    {
        // �̵��� Ÿ�� ����Ʈ
        List<Tile> path = new List<Tile>(prevMoveTile);
        //path.Reverse(); // ��θ� �ݴ�� ���� ���� Ÿ�Ϻ��� �̵�

        foreach (Tile tile in path)
        {
            currentStand.ReleaseCharacter();
            yield return StartCoroutine(MoveStep(tile));

            // Ÿ�� ���� �� ���� ���ĵ� ��ġ ������Ʈ
            currentStand = tile;
            currentStand.SetCharacter(this);
            currentStand.MoveComplete();
        }
        ClearMovementRange();
        moveCoroutine = null;
    }

    private void ClearMovementRange()
    {
        foreach (Tile tile in moveableArea)
        {
            tile.ReleaseSelected();  // ���õ� ���� ����
        }

        moveableArea.Clear();   // �̵� ���� ���� ����Ʈ �ʱ�ȭ
        prevMoveTile.Clear();   // �̵��� Ÿ�� ��� �ʱ�ȭ
    }

    private IEnumerator MoveStep(Tile nextTile)
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = nextTile.transform.position;
        Vector2 direction = targetPos - startPos;

        // �ִϸ��̼� ���� ����
        SetCharacterAnimation(direction);

        // ���� �ð� ���� �� Ÿ�Ϸ� �̵�
        float duration = 0.3f;  // �� ĭ �̵��� �ɸ��� �ð�
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        // Ÿ�Ͽ� ���� �� ��ġ ��Ȯ�ϰ� ����
        transform.position = targetPos;

    }

    private void SetCharacterAnimation(Vector2 direction)
    {
        Animator animator = GetComponent<Animator>();

        if (direction.x > 0.1f)
        {
            // ���������� �̵�
            animator.Play("Move_Left");
            GetComponent<SpriteRenderer>().flipX = true;
        }
        else if (direction.x < -0.1f)
        {
            animator.Play("Move_Left");
            GetComponent<SpriteRenderer>().flipX = false;
        }
        else if (direction.y > 0.1f)
        {
            // ���� �̵�
            animator.Play("Move_Up");
        }
        else if (direction.y < -0.1f)
        {
            animator.Play("Move_Down");
        }
        else
        {
            // �밢�� ó��
            if (direction.x > 0)
            {
                // ������ �� �밢��
                animator.Play("Move_Up");
            }
            else if (direction.x < 0)
            {
                // ���� �Ʒ� �밢��
                animator.Play("Move_Down");
            }
        }
    }

    private List<Tile> BFS(Tile start)
    {
        Map.Instance.ClearSearch();

        // reachableTiles = path��Ȱ
        List<Tile> reachableTiles = new List<Tile>();

        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();

        // �����ϱ� �� ��ġ ����
        queue.Enqueue(start);
        visited.Add(start);
        reachableTiles.Add(start);
        start.tileInfo.distance = 0;

        // �����¿� �������� �̵�
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            foreach (var dir in directions)
            {
                Tile next = Map.Instance.GetTile(current.tileInfo.coord + dir);

                if (next == null || visited.Contains(next)) continue;

                if (next.tileInfo.tileState == TileState.EMPTY && current.tileInfo.distance + 1 <= stat.MOV)
                {
                    next.SetSearchInfo(current.tileInfo);
                    next.tileInfo.distance = current.tileInfo.distance + 1;
                    queue.Enqueue(next);
                    visited.Add(next);
                    reachableTiles.Add(next);
                }
            }
        }
        return reachableTiles;
    }

    private List<Tile> RetraceTilePath(Tile start, Tile end)
    {
        List<Tile> path = new List<Tile>();
        Tile current = end;

        while (current != start)
        {
            path.Add(current);
            // by taking the parent we assigned
            current = current.Parent;
        }

        //then we simply reverse the list
        path.Reverse();

        return path;
    }

    // A*�� �̿��ؼ� ����
    public Tile GetNearestTile(Tile moveStart, List<Tile> candidateTiles, int movDistance)
    {
        Debug.Log("GetNearestTile ����");
        List<Tile> openList = new List<Tile>();
        HashSet<Tile> closeList = new HashSet<Tile>();

        // ���� ���� ����
        moveStart.tileInfo.SetAStarCosts(0, 0);
        openList.Add(moveStart);

        // ��ȯ Ÿ��(������)
        Tile nearestPath = null;

        while(openList.Count > 0)
        {
            // FCost�� ������ ����
            Tile currentTile = openList[0];
            foreach (Tile tile in openList)
            {
                if (tile.tileInfo.fCost < currentTile.tileInfo.fCost)
                {
                    currentTile = tile;
                }
            }

            // �湮�� Ÿ���� ���� �� �߰�
            openList.Remove(currentTile);
            closeList.Add(currentTile);

            // �̵� ������ Ÿ�ϸ� ��ȯ
            if (candidateTiles.Contains(currentTile) && currentTile != moveStart)
            {
                nearestPath = currentTile;
                break;
            }

            // �����¿� �������� �̵�
            Vector2Int[] directions =
            {
                Vector2Int.up,
                Vector2Int.down,
                Vector2Int.left,
                Vector2Int.right
            };

            foreach (Vector2Int direction in directions)
            {
                Tile neighbor = Map.Instance.GetTile(currentTile.tileInfo.coord + direction);

                // ���� Ÿ���� �ƴϰų� �ߺ��� Ÿ���� ��� ����
                if (!neighbor || closeList.Contains(neighbor)) continue;

                // Ÿ���̵��� ���� GCost ���� ���
                int newGCost = currentTile.tileInfo.gCost + 1;

                // �̵��º��� ����
                // openList�� ���ԵǾ� �ְ�
                // �Ǵ� ���� Ÿ�Ϻ��� ������ ������
                if(newGCost <= movDistance && 
                  (!openList.Contains(neighbor) || 
                    newGCost < neighbor.tileInfo.gCost))
                {
                    neighbor.tileInfo.gCost = newGCost;

                    // ���⼱ ��ǥ Ÿ���� �ƴ� ���� Ÿ�ϰ��� �Ÿ� ���
                    neighbor.tileInfo.hCost = GetHeuristicCost(neighbor, moveStart);
                    neighbor.tileInfo.prev = currentTile.tileInfo;

                    if(!openList.Contains(neighbor))
                        openList.Add(neighbor);
                }

            }
        }

        return nearestPath;
    }

    // ���� ������ �츮�� ���ؼ� ����ư �Ÿ��� ���
    private int GetHeuristicCost(Tile neighbor, Tile moveStart)
    {
        return Mathf.Abs(neighbor.tileInfo.coord.x - moveStart.tileInfo.coord.x) + Mathf.Abs(neighbor.tileInfo.coord.y - moveStart.tileInfo.coord.y);
    }
}




