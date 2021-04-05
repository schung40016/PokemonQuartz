using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTransition : MonoBehaviour
{
    [SerializeField]
    private float delayBeforeLoading = 2.66f;

    private float timeElaspsed;
    public Animator transitionAnim;

    // Update is called once per frame
    void Update()
    {
        timeElaspsed += Time.deltaTime;
        if( timeElaspsed > delayBeforeLoading )
        {
            transitionAnim.SetTrigger("fade");
        }
    }
}
