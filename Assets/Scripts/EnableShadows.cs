using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableShadows : MonoBehaviour {

    private SpriteRenderer rend;

	// Use this for initialization
	void Start () {
        rend.receiveShadows = true;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
