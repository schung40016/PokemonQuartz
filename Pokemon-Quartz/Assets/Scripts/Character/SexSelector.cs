using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SexSelector : MonoBehaviour
{
    //Store all artwork.
    [SerializeField] Sprite femaleSprite;
    [SerializeField] Sprite maleSprite;
    [SerializeField] RuntimeAnimatorController femaleAnim;
    [SerializeField] RuntimeAnimatorController maleAnim;

    //Create singleton instance.
    [SerializeField] GlobalControl globalC;

    //Player chose male.
    public void SetBoy()
    {
        globalC.SetPlayerSprite(maleSprite);
        globalC.SetPlayerAnim(maleAnim);
    }

    //Player chose female.
    public void SetGirl()
    {
        globalC.SetPlayerSprite(femaleSprite);
        globalC.SetPlayerAnim(femaleAnim);
    }
}
