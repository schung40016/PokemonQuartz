using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Dialog
{
    [SerializeField] List<string> lines;

    //Allows us to store all the dialog through this class.
    public List<string> Lines
    {
        get { return lines; }
    }
}
