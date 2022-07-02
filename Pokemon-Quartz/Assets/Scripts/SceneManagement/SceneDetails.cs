using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneDetails : MonoBehaviour
{
    [SerializeField] List<SceneDetails> connectedScenes;

    public bool IsLoaded { get; private set; }
    List<SavableEntity> savableEntities;

    //Triggered whenever some object(player) enters the trigger of this game object 
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") 
        {
            Debug.Log($"Entered {gameObject.name}");

            LoadScene();
            GameController.Instance.SetCurrentScene(this);

            // Load all connected scenes
            foreach (var scene in connectedScenes) 
            {
                scene.LoadScene();
            }

            // unload the scenes that are no longer connected
            var prevScene = GameController.Instance.PrevScene;
            if (GameController.Instance.PrevScene != null) 
            {
                var previouslyLoadedScenes = prevScene.connectedScenes;
                foreach (var scene in previouslyLoadedScenes) 
                {
                    if (!connectedScenes.Contains(scene) && scene != this)
                        scene.UnloadScene();
                }

                // Loads any unnecessary scenes that were loaded 
                if (!connectedScenes.Contains(prevScene))
                {
                    prevScene.UnloadScene();
                }
            }
        }
    }

    public void LoadScene() 
    {
        // this will make sure the sene is only loaded in once
        if (!IsLoaded)
        {
            // Will load the scenes additively without destroying the scenes that are currently open
            var operation = SceneManager.LoadSceneAsync(gameObject.name, LoadSceneMode.Additive);
            IsLoaded = true;

            operation.completed += (AsyncOperation op) =>
            {
                savableEntities = GetSavableEntitiesInScene();
                SavingSystem.i.RestoreEntityStates(savableEntities);
            };
        }
    }

    public void UnloadScene()
    {
        // this will make sure the sene is only loaded in once
        if (IsLoaded)
        {
            SavingSystem.i.CaptureEntityStates(savableEntities);

            // Will load the scenes additively without destroying the scenes that are currently open
            SceneManager.UnloadSceneAsync(gameObject.name);
            IsLoaded = false;
        }
    }

    List<SavableEntity> GetSavableEntitiesInScene()
    {
        var currScene = SceneManager.GetSceneByName(gameObject.name);
        var savableEntities = FindObjectsOfType<SavableEntity>().Where(x => x.gameObject.scene == currScene).ToList();

        return savableEntities;
    }
}
