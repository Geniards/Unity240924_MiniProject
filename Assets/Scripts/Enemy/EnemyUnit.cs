//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using static TurnManager;

//public class EnemyUnit : Unit
//{
//    protected override void Start()
//    {
//        base.Start();
//        team = Team.Enemy;
//    }

//    public IEnumerator TakeTurn()
//    {
//        Debug.Log($"{name}의 턴 시작");

//        Unit closestAlly = GridManager.Instance.FindClosestAlly(this);
//        if (closestAlly != null)
//        {
//            // 1. 이동 가능한 타일 탐색
//            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(currentTile, stats.moveRange);

//            foreach (Tile tile in reachableTiles)
//            {
//                tile.SetReachable(true);
//            }

//            yield return new WaitForSeconds(0.5f);


//            // 2. 아군 주변의 이동 가능한 타일 선택
//            Tile targetTile = GridManager.Instance.FindValidTileNearTarget(closestAlly);

//            if (targetTile)
//            {
//                // 3. 경로 탐색 및 이동
//                List<Tile> path = GridManager.Instance.FindPath(currentTile, targetTile);
//                List<Tile> limitedPath = GridManager.Instance.GetLimitedPath(path, stats.moveRange);

//                if (limitedPath != null)
//                {
//                    yield return StartCoroutine(MoveAlongPathCoroutine(limitedPath));  // 적군 이동
//                }
//            }

//            // 적군이 이동한 후 하이라이트를 끄기
//            foreach (Tile tile in reachableTiles)
//            {
//                tile.SetReachable(false);
//            }
//        }
//    }
//}

