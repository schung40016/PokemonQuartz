using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public static DialogManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    Dialog dialog;
    Action onDialogFinished;
    
    int currentLine = 0;
    bool isTyping;

    public bool IsShowing { get; private set; }

    public IEnumerator ShowDialogText(string text, bool waitForInput = true)
    {
        IsShowing = true;
        dialogBox.SetActive(true);

        yield return TypeDialog(text);

        if (waitForInput)
        {
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Z));
        }

        dialogBox.SetActive(false);
        IsShowing = false;
    }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished=null)
    {
        //Wait for frame to pass, so that we can avoid any bugs or glitches.
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    public void HandleUpdate()
    {
        if ( Input.GetKeyDown(KeyCode.Z) && !isTyping)
        {
            ++currentLine;
            //Start/continue dialogue.
            if( currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            //End dialog.
            else
            {
                currentLine = 0;
                IsShowing = false;
                dialogBox.SetActive(false);

                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;

        //To begin animation we want to start with no letters. Then add as we animate.
        dialogText.text = "";

        //Adds each letter to the sentence a single time.
        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            Debug.Log((1f / lettersPerSecond));
            yield return new WaitForSeconds(1f / lettersPerSecond);             // Does not end for some reason!!!
            Debug.Log(line);
        }

        isTyping = false;
    }
}
