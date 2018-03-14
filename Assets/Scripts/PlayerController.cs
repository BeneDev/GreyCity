using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    private Vector3 velocity;
    private PlayerInput input;

    [SerializeField] float speed = 3f;

	// Use this for initialization
	void Start () {
        input = GetComponent<PlayerInput>();
	}
	
	// Update is called once per frame
	void Update () {
        velocity = new Vector3(input.Horizontal, 0f);
        transform.position += velocity;
        if(input.Dodge)
        {
            print("Dodge");
        }
        if(input.Crouch)
        {
            print("Crouch");
        }
        if(input.Jump == 2)
        {
            print("Jumped");
        }
        if(input.Jump == 1)
        {
            print("Still jumping");
        }
        if(input.Interact)
        {
            print("Grab or throw");
        }
	}
}
