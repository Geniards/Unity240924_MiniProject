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

    // 이동완료 후 행동창 드는 이벤트
    public event Action OnMoveComplete;

    [Header("이동범위 세팅")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Tilemap moveRangeTilemap;
    [SerializeField] private TileBase moveRangeTile;    // 이동가능한 타일을 표시

    private HashSet<Vector3Int> visitedTiles = new HashSet<Vector3Int>(); // 방문한 타일 기록
    private List<Vector3Int> validMoveTiles = new List<Vector3Int>(); // 이동 가능한 타일 리스트
    //private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>(); // 원래 타일 저장

    private bool isMoveRangeDisplayed = false;

    // A* Setting
    [Header("A* Setting")]
    [SerializeField] private PathFinder pathFinder;
    private bool isMoving = false;


    private void Awake()
    {
        name = "조조";
        className = "군웅";
        level = 1;
        maxHp = 100;
        Hp = maxHp;
        moveRange = 4;
        actionPoint = 1;

        // 이동가능범위 : 너비 우선탐색
        // 문서화

        // 코루틴 몬스터 한턴한턴 움직임 가능

        pathFinder = GetComponent<PathFinder>(); // PathFinder 컴포넌트를 가져옴
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
        queue.Enqueue((startPos, maxMoveRange)); // 시작 위치와 남은 이동 범위 추가
        visitedTiles.Add(startPos);
        validMoveTiles.Add(startPos);

        // 네 방향 탐색 (상, 하, 좌, 우)
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

                    // 장애물인 경우 탐색 X
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

    // 타일에 따른 비용계산
    private int GetTileCost(Vector3Int tilePos)
    {
        TileBase tile = tilemap.GetTile(tilePos);
        TileBase obstacleTile = obstacleTilemap.GetTile(tilePos);

        if (obstacleTile)
        {
            return int.MaxValue; // 장애물인 경우 비용을 무한으로 설정
        }

        if (tile && (tile.name.Contains("Mountain") || tile.name.Contains("River")))
        {
            return 2;
        }

        return 1;
    }

    // 이동 가능한 타일을 시각적으로 표시
    private void HighlightMoveRange()
    {
        foreach (var tilePos in validMoveTiles)
        {
            moveRangeTilemap.SetTile(tilePos, moveRangeTile);
        }
    }

    // 이동 범위를 초기화, 타일을 원래대로 되돌림
    private void ClearMoveRange()
    {
        foreach (var tilePos in validMoveTiles)
        {
            moveRangeTilemap.SetTile(tilePos, null);
        }

        validMoveTiles.Clear();  // 이동 가능한 타일 목록 초기화
        visitedTiles.Clear();    // 방문한 타일 기록 초기화
    }

    // A* 알고리즘으로 이동
    public void MoveTo(Vector3Int targetPos)
    {
        if (validMoveTiles.Contains(targetPos) && !isMoving)
        {
            List<Vector2> path = null;
            Vector2Int start = new Vector2Int((int)transform.position.x, (int)transform.position.y);
            Vector2Int end = new Vector2Int(targetPos.x, targetPos.y);

            // A* 경로 탐색
            if (pathFinder.AStar(start, end, out path))
            {
                StartCoroutine(MoveAlongPath(path));
            }
            else
            {
                Debug.Log($"실패");
            }
        }
    }

    // 유닛을 A* 경로를 따라 이동
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

        // 이동이 끝나면 이벤트 발생
        isMoving = false;
        ClearMoveRange();
        OnMoveComplete?.Invoke();  // 이동 완료 후 이벤트 호출
    }

    public List<Vector3Int> GetvalidMoveTiles() { return validMoveTiles; }

    // ActionPoint 사용여부 (TurnManager에서 사용)
    public void ResetActionPoint() { actionPoint = 1; }

    public int GetActionPoint() { return actionPoint; }

}
