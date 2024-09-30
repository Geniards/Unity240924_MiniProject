//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class EnemyUnit : Character
//{
//    public bool isMoving { get { return moveCoroutine != null; } }

//    public void StartAI()
//    {
//        StopCurrentMove();
//        moveCoroutine = StartCoroutine(aiRoute());
//    }

//    private IEnumerator aiRoute()
//    {
//        currentStand.ClickTile(null);
//        yield return new WaitForSeconds(1f);
//        var target = Map.Instance.GetNearestTile(currentStand, moveableArea, stat.MOV);
//        target.ClickTile(null);

//        while (isMoving)
//            yield return null;
//    }

//}
