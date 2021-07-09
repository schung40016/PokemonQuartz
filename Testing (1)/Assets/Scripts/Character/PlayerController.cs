using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour, ISavable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    

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
        var colliders = Physics2D.OverlapCircleAll(transform.position - new Vector3(0, character.OffsetY), 0.2f, GameLayers.i.TriggerableLayers);

        foreach (var collider in colliders)
        {
            var triggerable = collider.GetComponent<IPlayerTriggerable>();
            if (triggerable != null) 
            {
                triggerable.onPLayerTriggerable(this);
                break;
            }
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

    // CaptureState is used to save data
    // object can be used to represent any type, including classes
    public object CaptureState()
    {
        float[] position = new float[] { transform.position.x, transform.position.y };
        return position;
    }

    // RestoreState is used to restore the data while the game is loading
    public void RestoreState(object state)
    {
        var position = (float[])state;
        transform.position = new Vector3(position[0], position[1]);
    }

    public string Name
    {
        get => name;
    }

    public Sprite Sprite
    {
        get => sprite;
    }

    public Character Character => character;
}
