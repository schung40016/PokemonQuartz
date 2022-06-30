using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSettings : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] Color regColor;

    public Color HighlightedColor => highlightedColor;
    public Color RegColor => regColor;

    public static GlobalSettings i { get; private set; }

    private void Awake()
    {
        i = this;
    }
}
