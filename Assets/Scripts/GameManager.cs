using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    public Vector3 currentCheckpoint; // Stores the Checkpoint which is currently activated, to send the player there if he dies

    private Queue<GameObject> characters = new Queue<GameObject>();

    private LayerMask enemiesMask;

    [Header("Sounds"), SerializeField] AudioSource soundNoise;

    // Delegate for when the next player is on
    public event System.Action<GameObject> OnPlayerChanged;

    // Make the GameManger Instance a Singleton 
    private void Awake()
    {
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
        foreach(GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            characters.Enqueue(player);
        }
        int layerEnemies = LayerMask.NameToLayer("Enemies");
        enemiesMask = 1 << layerEnemies;
    }

    private void Update()
    {
        if(Camera.main.GetComponentInParent<FollowPlayer>().player == null)
        {
            GetNextPlayer();
        }
    }

    public void GetNextPlayer()
    {
        if (characters.Count > 0)
        {
            GameObject nextPlayer = (GameObject)characters.Dequeue();
            nextPlayer.GetComponent<PlayerController>().enabled = true;
            if (OnPlayerChanged != null)
            {
                OnPlayerChanged(nextPlayer);
            }
        }
        else
        {
            print("Gameover");
        }
    }

    public void MakeNoise(float radius, Vector3 emitterPos)
    {
        Collider2D[] enemiesToAlert = Physics2D.OverlapCircleAll(emitterPos, radius, enemiesMask);
        for (int i = 0; i < enemiesToAlert.Length - 1; i++)
        {
            enemiesToAlert[i].gameObject.GetComponent<NormalGuard>().GetAlerted(emitterPos);
        }
        soundNoise.PlayOneShot(soundNoise.clip);
    }
}
