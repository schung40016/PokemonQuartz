using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerController : MonoBehaviour, Interactable
{
    [SerializeField] string name;
    [SerializeField] Sprite sprite;
    [SerializeField] Dialog dialog;
    [SerializeField] Dialog dialogAfterBattle;
    [SerializeField] GameObject exclamation;
    [SerializeField] GameObject fov;

    Character character;

    // State for whether trainer can battle or not.
    bool battleLost = false;

    private void Awake()
    {
        character = GetComponent<Character>();
    }

    private void Start()
    {
        SetFovRotation(character.Animator.DefaultDirection);
    }

    private void Update()
    {
        character.HandleUpdate();
    }

    // Following method triggers trainer battle. 1st, by talking. 2nd, by encountering.
    public void Interact(Transform initiator)
    {
        character.LookTowards(initiator.position);

        if(!battleLost)
        {
            // Show dialog.
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog, () =>
            {
                GameController.Instance.StartTrainerBattle(this);
            }));
        }
        else
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialogAfterBattle));
        }
    }

    public IEnumerator TriggerTrainerBattle(PlayerController player)
    {
        // Display exclamation mark for 0.5 seconds.
        exclamation.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        exclamation.SetActive(false);

        // Calculate how many tiles the trainer has to move.
        var diff = player.transform.position - transform.position;
        var moveVec = diff - diff.normalized;
        moveVec = new Vector2(Mathf.Round(moveVec.x), Mathf.Round(moveVec.y));

        yield return character.Move(moveVec);

        // Show dialog.
        StartCoroutine (DialogManager.Instance.ShowDialog(dialog, () => 
        {
            GameController.Instance.StartTrainerBattle(this);
        }) );
    }

    // Disables trainer's ability to battle after loss.
    public void BattleLost()
    {
        battleLost = true;
        fov.gameObject.SetActive(false);
    }

    //Rotates FOV to the trainer's facing direction.
    public void SetFovRotation(FacingDirection dir)
    {
        float angle = 0f;
        if( dir == FacingDirection.Right)
        {
            angle = 90f;
        }
        else if (dir == FacingDirection.Up)
        {
            angle = 180f;
        }
        else if (dir == FacingDirection.Left)
        {
            angle = 270f;
        }

        fov.transform.eulerAngles = new Vector3(0, 0, angle);
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
