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

    [Header("이동범위 세팅")]
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private Tilemap obstacleTilemap;
    [SerializeField] private Tilemap moveRangeTilemap;
    [SerializeField] private TileBase moveRangeTile;    // 이동가능한 타일을 표시

    private HashSet<Vector3Int> visitedTiles = new HashSet<Vector3Int>(); // 방문한 타일 기록
    private List<Vector3Int> validMoveTiles = new List<Vector3Int>(); // 이동 가능한 타일 리스트
    //private Dictionary<Vector3Int, TileBase> originalTiles = new Dictionary<Vector3Int, TileBase>(); // 원래 타일 저장

    private bool isMoveRangeDisplayed = false;

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
        // 남은거리가 0 or 방문했으면 종료
        if (remainingMoveRange <= 0 || visitedTiles.Contains(currentPos)) return;

        visitedTiles.Add(currentPos);   // 현재 타일을 방문으로
        validMoveTiles.Add(currentPos); // 이동가능한 타일로 추가
        Debug.Log($"validMoveTiles {currentPos}");

        // 현재 타일 정보를 저장
        //if (!originalTiles.ContainsKey(currentPos))
        //{
        //    originalTiles[currentPos] = tilemap.GetTile(currentPos);
        //}

        // 다음 타일을 상하 좌우로 탐색을 준비한다.
        // 네 방향 탐색 (상, 하, 좌, 우)
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

            // nextPos가 유효한 위치인지 파악
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

    public void ResetActionPoint() { actionPoint = 1; }

    public int GetActionPoint() { return actionPoint; }

}
