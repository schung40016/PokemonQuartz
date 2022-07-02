using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjects : MonoBehaviour
{
    // Essential Objects such as Player, BattleSystem, EventSystem, GameController and Canvas will not be destroyed when switching scene. 
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
