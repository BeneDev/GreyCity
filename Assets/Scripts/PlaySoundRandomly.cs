using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays a given sound randomly in a certain range
/// </summary>
public class PlaySoundRandomly : MonoBehaviour {

    [SerializeField] AudioSource audioSource;
    [SerializeField] float maxTimeInBetween = 30f;
    [SerializeField] float minTimeInBetween = 1f;

    private float counter; // The timer to tick down and start the sound again 

    private void Awake()
    {
        // Set the counter to a random time in the given range
        counter = Random.Range(minTimeInBetween, maxTimeInBetween);
    }
    
    void Update () {
        // Play the sound with a random volume when the counter is zero or lower 
		if(counter <= 0f)
        {
            if(audioSource.clip.name == "ThunderLouder")
            {
                GameManager.Instance.LightningStrike();
            }
            audioSource.volume = Random.Range(0.4f, 1f);
            audioSource.Play();
            counter = Random.Range(minTimeInBetween, maxTimeInBetween);
        }
        // Otherwise tick down the counter
        else if(counter > 0f)
        {
            counter -= Time.deltaTime;
        }
	}
}
