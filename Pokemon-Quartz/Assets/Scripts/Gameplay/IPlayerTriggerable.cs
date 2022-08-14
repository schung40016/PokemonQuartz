using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerTriggerable
{
    void onPlayerTriggerable(PlayerController player);

    bool TriggerRepeatedly { get; }
}
