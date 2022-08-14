using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Teleports the player to a different position without switching the scenes
public class LocationPortal : MonoBehaviour, IPlayerTriggerable
{
   
    [SerializeField] DestinationIdentifier destinationPortal;
    [SerializeField] Transform spawnPoint;

    PlayerController player;

    public void onPlayerTriggerable(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        this.player = player;
        StartCoroutine(Teleport());
        Debug.Log("hi");
    }

    Fader fader;
    private void Start()
    {
        fader = FindObjectOfType<Fader>();
    }

    IEnumerator Teleport()
    {

        GameController.Instance.PauseGame(true);
        yield return fader.FadeIn(0.5f); // before loading new screen we will fade the screen to black


        var destPortal = FindObjectsOfType<LocationPortal>().First(x => x != this && x.destinationPortal == this.destinationPortal);
        player.Character.SetPositionAndSnapToTile(destPortal.SpawnPoint.position);

        yield return fader.FadeOut(0.5f);
        GameController.Instance.PauseGame(false);

    }

    public Transform SpawnPoint => spawnPoint;

    public bool TriggerRepeatedly => false;
}
