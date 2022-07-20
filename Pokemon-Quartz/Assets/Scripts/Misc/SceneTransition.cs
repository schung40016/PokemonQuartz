using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField]
    private float delayBeforeLoading = 3.55f;
    [SerializeField]
    private string sceneNameToLoad;

    private float timeElaspsed;
    public Animator transitionAnim;

    private void Update()
    {
        timeElaspsed += Time.deltaTime;
        if (timeElaspsed > delayBeforeLoading)
        {
            StartCoroutine(LoadScene());
            Debug.Log("hello");
        }
        else if (Input.anyKeyDown)
        {
            SceneManager.LoadScene(sceneNameToLoad);
        }
    }

    IEnumerator LoadScene()
    {
        transitionAnim.SetTrigger("fade");
        yield return new WaitForSeconds(3.55f);
        SceneManager.LoadScene(sceneNameToLoad);
    }
}