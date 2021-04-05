using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;

    public bool IsPlayerUnit
    {
        get
        {
            return isPlayerUnit;
        }
    }

    public BattleHud Hud
    {
        get
        {
            return hud;
        }
    }

    public Pokemon Pokemon { get; set; }

    // Just an easy way to store a reference for the sprites.
    Image image;

    // Original position of the pokemon
    Vector3 originalPos;

    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;
        if(isPlayerUnit)
        {
            image.sprite = Pokemon.Base.BackSprite;
        }
        else
        {
            image.sprite = Pokemon.Base.FrontSprite;
        }

        hud.gameObject.SetActive(true);

        // Set up user hud.
        hud.SetData(pokemon);

        transform.localScale = new Vector3(1, 1, 1);

        // Resets color of pokemon for more battles.
        image.color = originalColor;

        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    // Plays animation of pokemon entering battle.
    public void PlayEnterAnimation()
    {
        // If-else statement gets both pokemons off screen.
        if(isPlayerUnit)
        {
            image.transform.localPosition = new Vector3(-510, originalPos.y);
        }
        else
        {
            image.transform.localPosition = new Vector3(510, originalPos.y);
        }

        // First input tells destination, second input tells how long it should take.
        image.transform.DOLocalMoveX(originalPos.x, 1f);
    }

    // Plays attack animations for the pokemons.
    public void PlayAttackAnimation()
    {
        // Allows multiple animation to play after each other.
        var sequence = DOTween.Sequence();
        if(isPlayerUnit)
        {
            sequence.Append( image.transform.DOLocalMoveX(originalPos.x + 50, 0.25f) );
        }
        else
        {
            sequence.Append( image.transform.DOLocalMoveX(originalPos.x - 50, 0.25f) );
        }

        // Moves pokemon back to their original position.
        sequence.Append(image.transform.DOLocalMoveX(originalPos.x, 0.25f));
    }

    // Plays the knockback animation of the pokemon.
    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.grey, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    // Plays faint animation.
    public void PlayFaintAnimation()
    {
        var sequence = DOTween.Sequence();

        //Lowers pokemon's position.
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150, 0.5f));

        //Fades pokemon.
        sequence.Join(image.DOFade(0f, 0.5f)); // Append animates one by one, Join animates seperate animations at the same time.
    }

    // Plays capture animation.
    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(0.3f, 0.3f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }

    // Play breakout animation.
    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(transform.DOLocalMoveY(originalPos.y, 0.5f));
        sequence.Join(transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
}
