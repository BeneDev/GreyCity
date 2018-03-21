using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RainController : MonoBehaviour {

    [SerializeField] GameObject rainObject;
    Camera cam;
    [SerializeField] float normalCamZoom = 5f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(rainObject)
        {
            if(cam.orthographicSize != normalCamZoom)
            {
                rainObject.SetActive(false);
            }
            else
            {
                //rainObject.SetActive(true);
            }
        }
	}
}
