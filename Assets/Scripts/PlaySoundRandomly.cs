using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundRandomly : MonoBehaviour {

    [SerializeField] AudioSource audioSource;
    [SerializeField] float maxTimeInBetween = 30f;
    [SerializeField] float minTimeInBetween = 1f;


    private float counter;
	
	// Update is called once per frame
	void Update () {
		if(counter <= 0f)
        {
            audioSource.volume = Random.Range(0.2f, 1f);
            audioSource.Play();
            counter = Random.Range(minTimeInBetween, maxTimeInBetween);
        }
        else if(counter > 0f)
        {
            counter -= Time.deltaTime;
        }
	}
}
