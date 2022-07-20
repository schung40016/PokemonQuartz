using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFov : MonoBehaviour, IPlayerTriggerable
{
    public void onPlayerTriggerable(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        GameController.Instance.OnEntersTrainerView(GetComponentInParent<TrainerController>());
    }
}
