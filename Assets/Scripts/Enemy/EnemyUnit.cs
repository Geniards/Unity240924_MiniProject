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
//        Debug.Log($"{name}�� �� ����");

//        Unit closestAlly = GridManager.Instance.FindClosestAlly(this);
//        if (closestAlly != null)
//        {
//            // 1. �̵� ������ Ÿ�� Ž��
//            List<Tile> reachableTiles = GridManager.Instance.FindReachableTiles(currentTile, stats.moveRange);

//            foreach (Tile tile in reachableTiles)
//            {
//                tile.SetReachable(true);
//            }

//            yield return new WaitForSeconds(0.5f);


//            // 2. �Ʊ� �ֺ��� �̵� ������ Ÿ�� ����
//            Tile targetTile = GridManager.Instance.FindValidTileNearTarget(closestAlly);

//            if (targetTile)
//            {
//                // 3. ��� Ž�� �� �̵�
//                List<Tile> path = GridManager.Instance.FindPath(currentTile, targetTile);
//                List<Tile> limitedPath = GridManager.Instance.GetLimitedPath(path, stats.moveRange);

//                if (limitedPath != null)
//                {
//                    yield return StartCoroutine(MoveAlongPathCoroutine(limitedPath));  // ���� �̵�
//                }
//            }

//            // ������ �̵��� �� ���̶���Ʈ�� ����
//            foreach (Tile tile in reachableTiles)
//            {
//                tile.SetReachable(false);
//            }
//        }
//    }
//}

