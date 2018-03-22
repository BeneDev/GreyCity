using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxEffect : MonoBehaviour {

    private GameObject player;

    [SerializeField] float speed = 1f;

    void Start()
    {
        // Subscribe to the delegate of the gamemaker, broadcasting when a new player is activated
        GameManager.Instance.OnPlayerChanged += GetNewPlayer;
    }

    // Update is called once per frame
    void Update () {
		if(player.GetComponent<PlayerController>().Velocity.x > 0f)
        {
            transform.position -= new Vector3(speed * Time.deltaTime, 0f);
        }
        else if(player.GetComponent<PlayerController>().Velocity.x < 0f)
        {
            transform.position += new Vector3(speed * Time.deltaTime, 0f);
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
