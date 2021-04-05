using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalControl : MonoBehaviour
{
    private Sprite playerSprite;
    private RuntimeAnimatorController playerAnimation;

    //Keeps track of which "save" is the real "save", and not a copy.
    static GlobalControl current;

    public void Start()
    {
        //If there is another singleton destroy this class.
        if( current != null)
        {
            Destroy(this.gameObject);
            return;
        }

        //Create singleton.
        current = this;
        GameObject.DontDestroyOnLoad(this.gameObject);
    }
    
    //Setters
    public void SetPlayerSprite(Sprite playerSprite)
    {
        this.playerSprite = playerSprite;
    }

    public void SetPlayerAnim(RuntimeAnimatorController playerAnimation)
    {
        this.playerAnimation = playerAnimation;
    }

    //Getters
    public Sprite getPlayerSprite()
    {
        return this.playerSprite;
    }

    public RuntimeAnimatorController getPlayerAnim()
    {
        return this.playerAnimation;
    }
}
