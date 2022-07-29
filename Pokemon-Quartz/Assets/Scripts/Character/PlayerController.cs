using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            StartCoroutine(Interact());
        }
    }

    IEnumerator Interact()
    {
        var facingDir = new Vector3(character.Animator.MoveX, character.Animator.MoveY);
        var interactPos = transform.position + facingDir;

        //Debug.DrawLine(transform.position, interactPos, Color.green, 0.5f);

        var collider = Physics2D.OverlapCircle(interactPos, 0.3f, GameLayers.i.InteractableLayer);

        //Interactable object detected.
        if( collider != null)
        {
            yield return collider.GetComponent<Interactable>()?.Interact(transform);
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
                triggerable.onPlayerTriggerable(this);
                break;
            }
        }
    }

    //Props up HUD.
    private void CheckForHud()
    {
        //Pressing escape shows the hud.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            character.Animator.IsMoving = false;
            ShowHud();
        }
    }

    // CaptureState is used to save data
    // object can be used to represent any type, including classes
    public object CaptureState()
    {
        var saveData = new PlayerSaveData()
        {
            position = new float[] { transform.position.x, transform.position.y },
            pokemons = GetComponent<PokemonParty>().Pokemons.Select(p => p.GetSaveData()).ToList()
        };

        return saveData;
    }

    // RestoreState is used to restore the data while the game is loading
    public void RestoreState(object state)
    {
        var saveData = (PlayerSaveData)state;

        // Restore Position.
        var pos = saveData.position;
        transform.position = new Vector3(pos[0], pos[1]);

        // Restore Party.
        GetComponent<PokemonParty>().Pokemons = saveData.pokemons.Select(s => new Pokemon(s)).ToList();
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

[Serializable]
// Stores and saves player data.
public class PlayerSaveData
{
    public float[] position;
    public List<PokemonSaveData> pokemons;
}