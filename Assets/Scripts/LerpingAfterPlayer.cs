using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LerpingAfterPlayer : MonoBehaviour {

    GameObject player;

    // Use this for initialization
    void Start() {
        GameManager.Instance.OnPlayerChanged += GetNewPlayer;
    }

    // Update is called once per frame
    void Update () {
		if(player)
        {
            Vector3 newPos = Vector3.Lerp(transform.position, player.transform.position, 100f);
            newPos.y = 10f;
            transform.position = newPos;
        }
	}

    void GetNewPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }
}
