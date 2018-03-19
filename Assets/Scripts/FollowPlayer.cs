using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Attach this script to a game object to make it follow the currently activated player
/// </summary>
public class FollowPlayer : MonoBehaviour {

    public GameObject player;
    
	void Start() {
        // Subscribe to the delegate of the gamemaker, broadcasting when a new player is activated
        GameManager.Instance.OnPlayerChanged += GetNewPlayer;
	}
	
	void Update () {
        // Follow the player if there is one referenced in the field
        if (player)
        {
            transform.position = player.transform.position;
            //transform.position += Vector3.Lerp(player.transform.position, transform.position, 100f);
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
