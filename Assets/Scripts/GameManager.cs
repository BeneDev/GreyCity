using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Script, making the object attached to a Singleton Object controlling the overall aspects of the game like defining the currently activated player
/// </summary>
public class GameManager : MonoBehaviour {

    #region Fields

    // Delegate for when the next player is on
    public event System.Action<GameObject> OnPlayerChanged;

    // The singleton instance of this script
    public static GameManager Instance { get; private set; }

    public Vector3 currentCheckpoint; // Stores the Checkpoint which is currently activated, to send the player there if he dies

    private Queue<GameObject> characters = new Queue<GameObject>(); // Stores the characters who could become players in the future

    private LayerMask enemiesMask; // The LayerMask for the enemies to make only them hear the noises made

    [Header("Sounds"), SerializeField] AudioSource soundNoise; // The sound being played when a noise is made by the player

    #endregion

    private void Awake()
    {
        // Make the GameManger Instance a Singleton 
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Set the first checkpoint to the starting point of the player
        currentCheckpoint = Vector3.zero;
        // Get all the possible characters into the characters queue
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            characters.Enqueue(player);
        }
        // Initialize the LayerMask for the enemies
        int layerEnemies = LayerMask.NameToLayer("Enemies");
        enemiesMask = 1 << layerEnemies;
    }

    private void Start()
    {
        // Broadcast the currently activated player for the first time
        if (OnPlayerChanged != null)
        {
            GetNextPlayer();
        }
    }

    private void Update()
    {
        // If the player deleted himself from the field in the main camera, because he died, activate the next character to be the currently activated player
        if(Camera.main.GetComponentInParent<FollowPlayer>().player == null)
        {
            GetNextPlayer();
        }
    }

    /// <summary>
    /// Takes the next player out of the characters queue and activates him
    /// </summary>
    public void GetNextPlayer()
    {
        // When the characters queue is not empty get the next character and make him the currently activated player
        if (characters.Count > 0)
        {
            GameObject nextPlayer = (GameObject)characters.Dequeue();
            nextPlayer.GetComponent<PlayerController>().enabled = true;
            if (OnPlayerChanged != null)
            {
                OnPlayerChanged(nextPlayer);
            }
        }
        // Otherwise it's GameOver
        else
        {
            // TODO make a gameover screen
            print("Gameover");
        }
    }

    /// <summary>
    /// "Makes noise" actually just giving the reached enemies the position, the noise is emitted from, to travel there. Also plays a soundqueue to warn the player
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="emitterPos"></param>
    public void MakeNoise(float radius, Vector3 emitterPos)
    {
        Collider2D[] enemiesToAlert = Physics2D.OverlapCircleAll(emitterPos, radius, enemiesMask);
        for (int i = 0; i < enemiesToAlert.Length - 1; i++)
        {
            enemiesToAlert[i].gameObject.GetComponent<GeneralEnemy>().PointToCheck = emitterPos;
        }
        soundNoise.PlayOneShot(soundNoise.clip);
    }
}
