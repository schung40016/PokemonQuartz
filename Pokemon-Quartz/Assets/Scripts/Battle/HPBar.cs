using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    //Adjusts hp bar based on damage or heal.
    public void SetHP( float hpNormalized )
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
    }

    //Animates the health bar to decrease in size.
    public IEnumerator SetHPSmooth(float newHp)
    {
        float curHp = health.transform.localScale.x;
        float changeAmt = curHp - newHp;

        //Keeps scaling until damage difference is close to zero.
        while( curHp - newHp > Mathf.Epsilon)
        {
            curHp -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHp, 1f);
            yield return null;
        }

        health.transform.localScale = new Vector3(newHp, 1f);
    }
}
