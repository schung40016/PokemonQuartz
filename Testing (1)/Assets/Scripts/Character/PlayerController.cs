using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    const float offSetY = 0.3f;

    public event Action OnEncountered;
    public event Action<Collider2D> OnEnterTrainersView;
    public event Action ShowHud;

    private Vector2 input;

    private Character character;

    private void Awake()
    {
        ////Find singleton script from unity and set instance.
        //GameObject go = GameObject.Find("GlobalControl");
        //GlobalControl gc = go.GetComponent<GlobalControl>();

        ////Get Sprite from singleton, and set as player's sprite.
        //this.gameObject.GetComponent<SpriteRenderer>().sprite = gc.getPlayerSprite();

        ////Get and set animator for user.    
        //this.GetComponent<Animator>().runtimeAnimatorController = gc.getPlayerAnim();

        character = GetComponent<Character>();
    }

    //Calculate and animate player's movement.
    public void HandleUpdate()
    {
        if(!character.IsMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            if (input.x != 0) input.y = 0;

            if( input != Vector2.zero)
            {
                StartCoroutine(character.Move(input, OnMoveOver));  
            }
        }

        character.HandleUpdate();

        CheckForHud();

        if(Input.GetKeyDown(KeyCode.Z))
        {
            Interact();
        }
    }

    void Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);

        //Interactable object detected.
        if( collider != null)
        {
            collider.GetComponent<Interactable>()?.Interact(transform);
        }
    }

    private void OnMoveOver()
    {
        CheckForEncounters();
        CheckIfInTrainersView();
    }

    //Checks bushes for encounters.
    private void CheckForEncounters()
    {
        // 1.0f => 2.0f
        //
        if(Physics2D.OverlapCircle(transform.position - new Vector3(0, offSetY), 0.2f, GameLayers.i.GrassLayer)  != null)
        {
            //Rolls for encounter
            if( UnityEngine.Random.Range(1, 101) <= 20 )
            {
                character.Animator.IsMoving = false;
                OnEncountered();
            }
        }
    }

    private void CheckIfInTrainersView()
    {
        var collider = Physics2D.OverlapCircle(transform.position - new Vector3(0, offSetY), 0.2f, GameLayers.i.FovLayer);
        if (collider != null)
        {
            character.Animator.IsMoving = false;
            OnEnterTrainersView?.Invoke(collider);
        }
    }

    //Prop up HUD if user wants to.
    private void CheckForHud()
    {
        //Pressing escape shows the hud.
        if ( Input.GetKeyDown(KeyCode.Q))
        {
            character.Animator.IsMoving = false;
            ShowHud();
        }
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }
}
