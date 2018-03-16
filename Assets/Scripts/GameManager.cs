using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance { get; private set; }

    public Vector3 currentCheckpoint; // Stores the Checkpoint which is currently activated, to send the player there if he dies

    private Queue<GameObject> characters = new Queue<GameObject>();

    private Camera cam;

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
        cam = Camera.main;
    }

    public void GetNextPlayer()
    {
        if (characters.Count > 0)
        {
            GameObject nextPlayer = (GameObject)characters.Dequeue();
            nextPlayer.GetComponent<PlayerController>().enabled = true;
            cam.GetComponentInParent<CameraController>().player = nextPlayer;
        }
        else
        {
            print("Gameover");
        }
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(cam.GetComponentInParent<CameraController>().player == null)
        {
            GetNextPlayer();
        }
	}
}
