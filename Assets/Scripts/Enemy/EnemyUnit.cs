using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Character
{
    [SerializeField] private bool isMoving;

    public bool GetIsMoving() { return moveCoroutine != null; }
    public void StartAI()
    {
        StopCurrentMove();
        moveCoroutine = StartCoroutine(aiRoute());
    }

    private IEnumerator aiRoute()
    {
        currentStand.ClickTile(null);
        yield return new WaitForSeconds(1f);
        var target = GetNearestTile(currentStand, moveableArea, stat.MOV);
        Debug.Log($"target {target} ");

        if (target)
        {
            target.ClickTile(null);
        }
        else
        {
            Debug.Log("Å¸°Ù¾øÀ½");
        }

        while (isMoving)
            yield return null;
    }

}
