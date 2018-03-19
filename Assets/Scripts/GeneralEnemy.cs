using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralEnemy : MonoBehaviour
{

    #region Properties

    // Stores if the enemy is looking to the left or not
    bool BLookLeft
    {
        get
        {
            return bLookLeft;
        }
        set
        {
            if (value == false)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else
            {
                transform.localScale = new Vector3(-1f, 1f, 1f);
            }
            bLookLeft = value;
        }
    }

    // Stores if the Enemy has detected the player
    public bool BDetected
    {
        get
        {
            return bDetected;
        }
        set
        {
            if(value == true)
            {
                durationUntilNotDetectedCounter = durationUntilNotDetected;
            }
            bDetected = value;
        }
    }

    // Stores the point the enemy is drawn to, because he heard something from there
    public Vector3 PointToCheck
    {
        get
        {
            return pointToCheck;
        }
        set
        {
            pointToCheck = value;
            if (alertedSound)
            {
                if (!alertedSound.isPlaying)
                {
                    alertedSound.PlayOneShot(alertedSound.clip);
                }
            }
        }
    }

    #endregion

    #region Fields

    [Header("Movement"), SerializeField] float moveSpeed = 1f; // The normal movement speed of the enemy
    [SerializeField] float stoppingDistance = 0.5f; // The distance, the enemy keeps to his targets

    [Header("Timer"), SerializeField] float durationUntilNotDetected = 3f; // The amount of time it takes after the enemy lost sight to the player, to return back to normal movement
    protected float durationUntilNotDetectedCounter; // The actual timer to tick down after the enemy lost sight to the player
    [SerializeField] float timeToGiveUpAfter = 6f; // The amount of seconds after which the enemy will give up to look for the point where the alerting sound came from

    [Header("The Eyes"), SerializeField] GameObject eyes; // Stores the Gameobject which has the collider, defining the sight of the enemy

    // Variables to find the player
    protected GameObject player;
    protected Vector3 toPlayer;

    // The layer mask to check collision and if sight to the player is blocked
    protected LayerMask layersToCollideWith; // The enemy will collide with those. This will be the Ground Layer
    protected LayerMask layersBlockingView; // These layers will block the view of the enemy. This will be the Ground and the default Layer

    // Components needed to store
    protected BoxCollider2D coll;
    protected SpriteRenderer rend;

    // Raycasts to move accordingly in the scene
    struct Raycasts
    {
        public RaycastHit2D left;
        public RaycastHit2D right;
        public RaycastHit2D upperLeft;
        public RaycastHit2D upperRight;
    }
    private Raycasts rays;

    // The state machine for the enemy
    enum PlayerState
    {
        patroling,
        detected,
        alerted,
        lookAround
    }
    PlayerState state = PlayerState.patroling;

    // Booleans storing informations about the enemy
    protected bool bLookLeft = false;   // Stores if the enemy looks to the left
    protected bool bDetected = false; // Stores if this enemy detected the player
    protected bool bPlayerInSight = false; // Stores wether the player is in sight or not

    Vector3 pointToCheck = Vector3.zero; // The point to walk to if it is set

    [Header("Sound"), SerializeField] AudioSource alarmSound; // The alarm sound to player when this enemy detected a player
    protected float alarmSoundVolume; // The alarm sound volume stored, to set it to this volume after the sound was fading out
    [SerializeField] AudioSource alertedSound;

    #endregion

    /// <summary>
    /// This method gets called by the derived scripts in the Start method
    /// </summary>
    protected virtual void GeneralInitialization()
    {
        // Get needed Components
        coll = GetComponent<BoxCollider2D>();
        rend = GetComponent<SpriteRenderer>();
        // Get needed LayerMasks
        int groundLayer = LayerMask.NameToLayer("Ground");
        int defaultLayer = LayerMask.NameToLayer("Default");
        layersToCollideWith = 1 << groundLayer;
        layersBlockingView = 1 << groundLayer;
        LayerMask defaultMask = 1 << defaultLayer;
        layersBlockingView = layersBlockingView | defaultMask;

        // Subscribe to the Game Managers Delegate, broadcasting every new player
        GameManager.Instance.OnPlayerChanged += GetNewPlayer;

        // Set the look left value according to the local scale
        if (transform.localScale.x == -1)
        {
            BLookLeft = true;
        }
        else
        {
            BLookLeft = false;
        }
        // Get the inital alarm sound volume
        alarmSoundVolume = alarmSound.volume;
    }

    protected virtual void GeneralStateBehavior()
    {
        #region Raycast Initialization

        // Update all of the rays, used to check for walls besides the enemy
        rays.left = Physics2D.Raycast(transform.position + new Vector3(coll.bounds.extents.x, 0.0f), Vector2.right, 0.2f, layersToCollideWith);
        rays.right = Physics2D.Raycast(transform.position + new Vector3(-coll.bounds.extents.x, 0.0f), Vector2.left, 0.2f, layersToCollideWith);

        rays.upperLeft = Physics2D.Raycast(transform.position + new Vector3(coll.bounds.extents.x, 0.4f), Vector2.right, 0.2f, layersToCollideWith);
        rays.upperRight = Physics2D.Raycast(transform.position + new Vector3(-coll.bounds.extents.x, 0.4f), Vector2.left, 0.2f, layersToCollideWith);

        #endregion
        // Update the Vector, pointing towards the player and the field which stores if the player is in sight or not
        if (player)
        {
            toPlayer = player.transform.position - transform.position;
            if (eyes.GetComponent<BoxCollider2D>().bounds.Contains(player.transform.position))
            {
                bPlayerInSight = true;
            }
            else
            {
                bPlayerInSight = false;
            }
        }
        switch (state)
        {
            case PlayerState.patroling:
                SimpleMove();
                break;
            case PlayerState.detected:
                DetectedBehavior();
                break;
            case PlayerState.alerted:
                AlertedBehavior();
                break;
            case PlayerState.lookAround:
                LookAround();
                break;
        }
    }

    /// <summary>
    /// This method gets called by the derived scripts in the update method
    /// </summary>
    protected virtual void GeneralBehavior()
    {
        #region Raycast Initialization

        // Update all of the rays, used to check for walls besides the enemy
        rays.left = Physics2D.Raycast(transform.position + new Vector3(coll.bounds.extents.x, 0.0f), Vector2.right, 0.2f, layersToCollideWith);
        rays.right = Physics2D.Raycast(transform.position + new Vector3(-coll.bounds.extents.x, 0.0f), Vector2.left, 0.2f, layersToCollideWith);

        rays.upperLeft = Physics2D.Raycast(transform.position + new Vector3(coll.bounds.extents.x, 0.4f), Vector2.right, 0.2f, layersToCollideWith);
        rays.upperRight = Physics2D.Raycast(transform.position + new Vector3(-coll.bounds.extents.x, 0.4f), Vector2.left, 0.2f, layersToCollideWith);

        #endregion
        // Update the Vector, pointing towards the player and the field which stores if the player is in sight or not
        if (player)
        {
            toPlayer = player.transform.position - transform.position;
            if(eyes.GetComponent<BoxCollider2D>().bounds.Contains(player.transform.position))
            {
                bPlayerInSight = true;
            }
            else
            {
                bPlayerInSight = false;
            }
        }
        // When the player is not detected and there is no point to be drawn towards, simply patrol
        if(!BDetected && pointToCheck == Vector3.zero)
        {
            SimpleMove();
        }
        // When the player is in sight and there is nothing blocking the view, the player is detected
        if (bPlayerInSight)
        {
            if (CheckForDetected())
            {
                BDetected = true;
            }
        }
        // When the player is detected, play the alarm sound and behave the way its supposed to when the player is detected 
        if(BDetected)
        {
            if (!alarmSound.isPlaying)
            {
                alarmSound.Play();
            }
            DetectedBehavior();
        }
        // When the player is not detected and the alarm sound is playing, let it fade out
        else
        {
            // TODO wait a given time before fading out the alarm sound(make it player after the enemy looked around)
            if (alarmSound.isPlaying)
            {
                // Stop the audio source from playing
                Coroutine fadingOut = StartCoroutine(FadeOut(alarmSound, 1f));
                if (fadingOut != null)
                {
                    StopCoroutine(fadingOut);
                }
            }
        }
        // When the player is not detected, but there is a point of interest(caused by noise from there) behave accordingly and travel towards that point
        if (!BDetected && pointToCheck != Vector3.zero)
        {
            StartCoroutine(AlertedBehavior());
        }
        // Count down the timer until the player is not detected anymore
        if (durationUntilNotDetectedCounter > 0f)
        {
            durationUntilNotDetectedCounter -= Time.deltaTime;
        }
        else if (durationUntilNotDetectedCounter <= 0f && BDetected)
        {
            StartCoroutine(LookAround());
            BDetected = false;
        }
    }

    #region Helper Methods

    #region Behaviors

    /// <summary>
    /// How the enemy behaves when he has seen the player
    /// </summary>
    protected virtual void DetectedBehavior()
    {
        // Tick down the detection counter of the player, killing him when he stays in sight for too long
        player.GetComponent<PlayerController>().DetectionCounter -= Time.deltaTime;
        if(player.GetComponent<PlayerController>().DetectionCounter <= 0f)
        {
            BDetected = false;
            StartCoroutine(LookAround());
        }
        // Walk towards the player when he is farther away than the stopping distance 
        if (player)
        {
            if (toPlayer.x > 0)
            {
                BLookLeft = false;
            }
            else if (toPlayer.x < 0)
            {
                BLookLeft = true;
            }
            if (toPlayer.magnitude > stoppingDistance)
            {
                transform.position += new Vector3(moveSpeed * 2f * transform.localScale.x * Time.deltaTime, 0f);
            }
            else if(toPlayer.magnitude < stoppingDistance)
            {
                transform.position += new Vector3(moveSpeed * 2f * -transform.localScale.x * Time.deltaTime, 0f);
            }
        }
    }

    /// <summary>
    /// Make the enemy turn around, whenever he faces the end of the platform he's walking on or a wall in front of him
    /// </summary>
    protected virtual void SimpleMove()
    {
        // When the enemy has walls either to the right or left side of him turn away from the wall
        if (!BLookLeft)
        {
            if (rays.left || rays.upperLeft)
            {
                BLookLeft = true;
            }
        }
        else if (BLookLeft)
        {
            if (rays.right || rays.upperRight)
            {
                BLookLeft = false;
            }
        }
        // Applies the movement after all the checks above
        transform.position += new Vector3(moveSpeed * transform.localScale.x * Time.deltaTime, 0f);
    }

    /// <summary>
    /// How the enemy behaves when he was alerted by a sound he heard
    /// </summary>
    /// <returns></returns>
    IEnumerator AlertedBehavior()
    {
        // Get a vector3 towards the point, which was the source to alert the enenmy
        Vector3 toPoint = pointToCheck - transform.position;
        // Turn towards the point 
        if (toPoint.x > 0)
        {
            BLookLeft = false;
        }
        else if (toPoint.x < 0)
        {
            BLookLeft = true;
        }
        // Wait a seconds before the enemy will carefully walk towards the point
        yield return new WaitForSeconds(1f);
        if (!eyes.GetComponent<BoxCollider2D>().bounds.Contains(pointToCheck))
        {
            transform.position += new Vector3(moveSpeed * 0.75f * transform.localScale.x * Time.deltaTime, 0f);
            // Give up after a given time, when you cant get to the point
            StartCoroutine(GiveUpAfterSeconds(timeToGiveUpAfter));
        }
    }

    IEnumerator LookAround()
    {
        for(int i = 0; i < (int)Random.Range(2f, 5f); i++)
        {
            BLookLeft = !BLookLeft;
            yield return new WaitForSeconds(Random.Range(1.5f, 3f));
        }
    }

    #endregion

    protected void WaitForSeconds(float seconds)
    {
        float startTime = Time.time;

    }

    /// <summary>
    /// Make the enemy give up the searching for the source of a sound he hears
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    IEnumerator GiveUpAfterSeconds(float seconds)
    {
        // Wait for given seconds until the point to walk to is reset to being zero
        yield return new WaitForSeconds(seconds);
        pointToCheck = Vector3.zero;
        //StartCoroutine(LookAround());
    }

    /// <summary>
    /// Checks if the player is in sight or if the sight is blocked by objects
    /// </summary>
    /// <returns></returns>
    private bool CheckForDetected()
    {
        // When there is anything of the layers which block the view in the way to the player, return false, otherwise true
        Vector3 direction = player.GetComponent<PlayerController>().PlayerEyes - eyes.transform.position;
        Debug.DrawRay(eyes.transform.position, direction);
        if (Physics2D.Raycast(eyes.transform.position, direction, direction.magnitude, layersBlockingView) && !BDetected)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// Makes the sound fade out over the given time
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="FadeTime"></param>
    /// <returns></returns>
    IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        // Let the sound fade out over the given time
        while (audioSource.volume > 0f)
        {
            audioSource.volume -= Time.deltaTime / 4;

            yield return new WaitForEndOfFrame();
        }
        // Stop the audio source from playing and resets the volume
        audioSource.Stop();
        audioSource.volume = alarmSoundVolume;
        yield break;
    }

    /// <summary>
    /// The method which is subscribed to the Game Manager On Player Change Delegate
    /// </summary>
    /// <param name="newPlayer"></param>
    protected void GetNewPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }

    #endregion

}

