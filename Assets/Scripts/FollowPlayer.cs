using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Attach this script to a game object to make it follow the currently activated player
/// </summary>
public class FollowPlayer : MonoBehaviour {
    
    private float normalCamZoom;
    public GameObject player;
    Camera cam;
    
    [SerializeField] float speed = 1f;
    [SerializeField] float zoomInSpeed = 1f;
    [SerializeField] float zoomedOutValue = 15f;

    private void Awake()
    {
        cam = Camera.main;
        normalCamZoom = cam.orthographicSize;
    }

    void Start() {
        // Subscribe to the delegate of the gamemaker, broadcasting when a new player is activated
        GameManager.Instance.OnPlayerChanged += GetNewPlayer;
    }
	
	void Update () {
        if(!player.GetComponent<PlayerController>().hasMoved)
        {
            cam.orthographicSize = zoomedOutValue;
        }
        else if(cam.orthographicSize != normalCamZoom)
        {
            float newZoom = Mathf.Lerp(cam.orthographicSize, normalCamZoom, zoomInSpeed * Time.deltaTime);
            cam.orthographicSize = newZoom;
        }
        // Follow the player if there is one referenced in the field
        if (player)
        {
            // Make smooth transition to the currently active player
            Vector3 newPos = transform.position;
            newPos.y = Mathf.Lerp(transform.position.y, player.transform.position.y, speed * Time.deltaTime);
            newPos.x = Mathf.Lerp(transform.position.x, player.transform.position.x, speed * Time.deltaTime);

            transform.position = newPos;
        }
	}

    /// <summary>
    /// The method subscribed to the Game Maker instance for changes to the currently activated player
    /// </summary>
    /// <param name="newPlayer"></param>
    void GetNewPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }
}
