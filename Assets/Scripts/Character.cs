using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CharaterSide { Alliance, Enemy, CHARATERSIDE_MAX }

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
            prevMoveTile.Insert(0, tile);
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
        // 이동할 타일 리스트
        List<Tile> path = new List<Tile>(prevMoveTile);
        //path.Reverse(); // 경로를 반대로 만들어서 시작 타일부터 이동

        foreach (Tile tile in path)
        {
            currentStand.ReleaseCharacter();
            yield return StartCoroutine(MoveStep(tile));

            // 타일 도착 후 현재 스탠딩 위치 업데이트
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
            tile.ReleaseSelected();  // 선택된 상태 해제
        }

        moveableArea.Clear();   // 이동 가능 범위 리스트 초기화
        prevMoveTile.Clear();   // 이동한 타일 목록 초기화
    }

    private IEnumerator MoveStep(Tile nextTile)
    {
        Vector2 startPos = transform.position;
        Vector2 targetPos = nextTile.transform.position;
        Vector2 direction = targetPos - startPos;

        // 애니메이션 방향 설정
        SetCharacterAnimation(direction);

        // 일정 시간 동안 한 타일로 이동
        float duration = 0.3f;  // 한 칸 이동에 걸리는 시간
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector2.Lerp(startPos, targetPos, elapsed / duration);
            yield return null;
        }

        // 타일에 도착 후 위치 정확하게 맞춤
        transform.position = targetPos;

    }

    private void SetCharacterAnimation(Vector2 direction)
    {
        Animator animator = GetComponent<Animator>();

        if (direction.x > 0.1f)
        {
            // 오른쪽으로 이동
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
            // 위로 이동
            animator.Play("Move_Up");
        }
        else if (direction.y < -0.1f)
        {
            animator.Play("Move_Down");
        }
        else
        {
            // 대각선 처리
            if (direction.x > 0)
            {
                // 오른쪽 위 대각선
                animator.Play("Move_Up");
            }
            else if (direction.x < 0)
            {
                // 왼쪽 아래 대각선
                animator.Play("Move_Down");
            }
        }
    }

    private List<Tile> BFS(Tile start)
    {
        Map.Instance.ClearSearch();

        // reachableTiles = path역활
        List<Tile> reachableTiles = new List<Tile>();

        Queue<Tile> queue = new Queue<Tile>();
        HashSet<Tile> visited = new HashSet<Tile>();

        // 시작하기 전 위치 저장
        queue.Enqueue(start);
        visited.Add(start);
        reachableTiles.Add(start);
        start.tileInfo.distance = 0;

        // 상하좌우 방향으로 이동
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


}




