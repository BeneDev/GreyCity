﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    [SerializeField] AudioSource radio;
    [SerializeField] AudioSource call;
    [SerializeField] GameObject frame1;
    [SerializeField] GameObject frame2;

    private float absolutStartTime;
    private float radioStartTime = 0f;
    private float callStartTime = 0f;

	// Use this for initialization
	void Start () {
        absolutStartTime = Time.realtimeSinceStartup;
	}
	
	// Update is called once per frame
	void Update () {
        if (Time.realtimeSinceStartup >= absolutStartTime + 1.5f && radioStartTime == 0f)
        {
            if (!radio.isPlaying)
            {
                radio.Play();
                radioStartTime = Time.realtimeSinceStartup + 3f;
            }
        }
		if(Time.realtimeSinceStartup >= radioStartTime + radio.clip.length)
        {
            radio.Stop();
            if (!call.isPlaying)
            {
                call.Play();
                callStartTime = Time.realtimeSinceStartup;
                StartCoroutine(PickUpPhone());
            }
        }
        if(Time.realtimeSinceStartup >= callStartTime + call.clip.length)
        {
            print("Load the fucking scene you asshole");
            SceneManager.LoadScene(1);
        }
	}

    IEnumerator PickUpPhone()
    {
        yield return new WaitForSeconds(3f);
        frame1.SetActive(false);
        frame2.SetActive(true);
    }

}