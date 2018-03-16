using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour {

    public GameObject player;

	// Use this for initialization
	void Start() {
        GameManager.Instance.OnPlayerChanged += GetNewPlayer;
	}
	
	// Update is called once per frame
	void Update () {
        if (player)
        {
            transform.position = player.transform.position;
        }
	}

    void GetNewPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }
}
