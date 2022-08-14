using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LongGrass : MonoBehaviour, IPlayerTriggerable
{
    public bool TriggerRepeatedly => true;

    public void onPlayerTriggerable(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 20)
        {
            player.Character.Animator.IsMoving = false;
            GameController.Instance.StartBattle();
        }
    }
}
