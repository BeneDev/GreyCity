using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Script to control the player
/// </summary>
public class PlayerController : MonoBehaviour {

    #region Properties

    // Just returns the position of the players eyes to make it available for other scripts to read
    public Vector3 PlayerEyes
    {
        get
        {
            return eyes;
        }
    }

    // Makes the detectionCounter available to read and write for other scripts
    public float DetectionCounter
    {
        get
        {
            return detectionCounter;
        }
        set
        {
            detectionCounter = value;
        }
    }

    #endregion

    #region Fields

    [Header("Physics"), SerializeField] float gravity = 1f;
    [SerializeField] float veloYLimit = -1f; // The player cant fall faster than that
    [SerializeField] float jumpForce = 1f; // The force which is applied to the player when he is jumping
    [SerializeField] float fallFactor = 0.2f; // The force which, either gets subtracted when the player does not hold the jump button anymore when being in the air or which gets added when he is actually still holding the jump button

    [Header("Speed Values"), SerializeField] float speed = 3f; // The normal speed which is used to calculate the actual speed
    private float actualSpeed; // The actual speed with which the player travels
    [SerializeField] float crouchSpeedPenalty = 0.15f; // The factor which is applied to the normal speed when crouching
    [SerializeField] float wallSlideSpeed = 0.4f; // The speed of which the player falls down when pressing against a wall

    private float heightToPullUpTo; // The height of the object standing in front of when pressing jump. Most likely used for the pull up animation later
    private float fellThroughCounter = 0f; // The timer to tick down when the player fell through a ground gate. This prevents the player from getting stuck in the ground gate if he instantly let go of holding the left stick down
    private Vector3 velocity; // The field storing the velocity over the fixed update circle. This is applied to the transform position in the end after it got checked for validity

    // Components the player needs to store
    private PlayerInput input;
    private SpriteRenderer rend;
    private AudioSource audioSource;

    private Animator anim;
    private Camera cam;

    // Several LayerMasks
    private LayerMask layersToCollideWith; // This will be the ground layer
    private LayerMask layersDetectingThePlayer; // This will be the enemies light layer

    private GameObject grabbedObject; // The currently hold object is stored in here

    [Header("Ranges"), SerializeField] float shoutRange = 50f; // The radius of the circle, to reach enemies and alert them when shouting
    [SerializeField] float walkNoiseRadius = 1f; // The radius of the circle, to reach enemies and alert them when not crouching

    [Header("Timer"), SerializeField] float detectionTime = 2f; // The amount of seconds it takes to get detected
    private float detectionCounter; // The acutal counter ticking down the seconds it takes to get detected

    [Header("Sound"), SerializeField] AudioSource heartBeatAudioSource; // The audio Source which stores the heart beat sound
    [SerializeField] AudioSource shots; // The sound which is played when the player is being killed

    // All the booleans to store informations about the player 
    private bool bJumpable = true;
    private bool bOnWall = false;
    private bool bGrounded = false;
    private bool bCrouching = false;
    private bool bDead = false;
    public bool hasMoved = false; // Lets the cam zoom out if the player hasnt moved yet

    private Vector3 eyes; // The position of the eyes of the player

    struct PlayerRaycasts // To store the informations of raycasts around the player to calculate physics
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomRight;
        public RaycastHit2D upperLeft;
        public RaycastHit2D lowerLeft;
        public RaycastHit2D upperRight;
        public RaycastHit2D lowerRight;
        public RaycastHit2D top;
        public RaycastHit2D detectRight;
        public RaycastHit2D detectLeft;
    }
    PlayerRaycasts rays;

    RaycastHit2D[] anyPhysicsRaycast = new RaycastHit2D[7]; // Any Raycast used for the Physics collision

    #endregion

    // Initializes the fields when the script is being enabled
    private void OnEnable()
    {
        // Get the components of the player to use
        input = GetComponent<PlayerInput>();
        rend = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        // Get the camera to delete this object from the player field there, when this character died
        cam = Camera.main;
        // Get the layerMask for collision
        int layerGround = LayerMask.NameToLayer("Ground");
        int layerEnemyLight = LayerMask.NameToLayer("EnemiesLight");
        layersToCollideWith = 1 << layerGround;
        layersDetectingThePlayer = 1 << layerEnemyLight;

        // Set the detection counter to the time it takes to die after being detected
        detectionCounter = detectionTime;
    }

    private void OnDisable()
    {
        float startTime = Time.realtimeSinceStartup;
        // Rotate the old player to show he ded
        Quaternion newRotation = new Quaternion();
        newRotation.eulerAngles = new Vector3(0f, 0f, 90f);
        gameObject.transform.rotation = newRotation;
        StartCoroutine(ShootAfterSeconds(1f));
    }

    void Update () {
        if (!bDead)
        {
            //if(input.Interact)
            //{
            //    print("Grab or throw");
            //}
            // Go in Crouching mode for as long as the player holds the button
            if (input.Crouch)
            {
                Crouch();
            }
            else
            {
                bCrouching = false;
                anim.SetBool("Crouching", false);
            }
            // When the player is not on a wall, there is nothing to pull up to
            if (!bOnWall)
            {
                heightToPullUpTo = 0f;
            }
            // Flip the sprite in the direction of travel
            Flip();
            // When the player is in the sight of an enemy
            if (RaycastForTag("EnemyLight", rays.detectRight, rays.detectLeft))
            {
                // Make the hearbeat sound go
                heartBeatAudioSource.volume = 1f;
                if (heartBeatAudioSource && !heartBeatAudioSource.isPlaying)
                {
                    heartBeatAudioSource.Play();
                }
                // Die when the detection counter ticked down
                if (detectionCounter <= 0f)
                {
                    bDead = true;
                }
            }
            // When not in sight of an enemy
            else if (!RaycastForTag("EnemyLight", rays.detectLeft, rays.detectRight))
            {
                // Fade out the heartbeat sound
                if (heartBeatAudioSource.isPlaying)
                {
                    Coroutine fadingOut = StartCoroutine(FadeOut(heartBeatAudioSource, 1f));
                    if (fadingOut != null)
                    {
                        StopCoroutine(fadingOut);
                    }
                }
                // Slowly increase the detection Counter again
                if (detectionCounter < detectionTime)
                {
                    detectionCounter += Time.deltaTime / 2;
                }
                else if (detectionCounter > detectionTime)
                {
                    detectionCounter = detectionTime;
                }
            }
            // Detect Checkpoint in range and activate him
            if (RaycastForTag("Checkpoint", rays.detectRight, rays.detectLeft))
            {
                RaycastHit2D newCheckpoint = (RaycastHit2D)WhichRaycastForTag("Checkpoint", rays.detectRight, rays.detectLeft);
                GameManager.Instance.currentCheckpoint = newCheckpoint.collider.gameObject.transform.position;
            }
            // Shout when the button is pressed
            if (input.Shout)
            {
                GameManager.Instance.MakeNoise(shoutRange, transform.position);
            }
        }
        else
        {
            Die();
        }
    }

    private void FixedUpdate()
    {
        if (!bDead)
        {
            #region Raycast Initialization

            // Update all the different raycast hit values to calculate physics
            rays.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * 0.1f + Vector3.down * 0.4f, Vector2.down, 0.35f, layersToCollideWith);
            rays.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * -0.2f + Vector3.down * 0.4f, Vector2.down, 0.35f, layersToCollideWith);

            rays.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.right * 0.4f, Vector2.left, 0.3f, layersToCollideWith);
            rays.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.left * 0.4f, Vector2.right, 0.3f, layersToCollideWith);

            rays.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.3f + Vector3.right * 0.4f, Vector2.left, 0.3f, layersToCollideWith);
            rays.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.3f + Vector3.left * 0.4f, Vector2.right, 0.3f, layersToCollideWith);

            rays.detectRight = Physics2D.Raycast(transform.position + Vector3.left * 0.4f, Vector2.right, 0.3f);
            rays.detectLeft = Physics2D.Raycast(transform.position + Vector3.left * 0.4f, Vector2.right, 0.3f);

            rays.top = Physics2D.Raycast(transform.position + Vector3.up * 0.4f, Vector2.up, 0.2f, layersToCollideWith);

            anyPhysicsRaycast[0] = rays.bottomRight;
            anyPhysicsRaycast[1] = rays.bottomLeft;
            anyPhysicsRaycast[2] = rays.lowerLeft;
            anyPhysicsRaycast[3] = rays.upperLeft;
            anyPhysicsRaycast[4] = rays.lowerRight;
            anyPhysicsRaycast[5] = rays.upperRight;
            anyPhysicsRaycast[6] = rays.top;

            #endregion

            // Make player move slower when crouching and set the eye position down
            if (bCrouching)
            {
                actualSpeed = speed * crouchSpeedPenalty;
                eyes = transform.position + Vector3.down * 0.2f;

            }
            // Otherwise make him have the normal speed and normal eyeposition
            else
            {
                actualSpeed = speed;
                eyes = new Vector3(transform.position.x, transform.position.y + 0.8f);
            }

            if(input.Horizontal != 0f && !hasMoved)
            {
                hasMoved = true;
            }

            // Set the horizontale velocity to the given input
            velocity = new Vector3(input.Horizontal * actualSpeed * Time.fixedDeltaTime, velocity.y);

            CheckGrounded();

            // When the player is not crouching, allow him to jump
            if (!bCrouching)
            {
                HandleJump();
            }

            // Apply Gravity if not grounded
            if (!bGrounded)
            {
                velocity.y -= gravity * Time.fixedDeltaTime;
            }

            // Apply the velocity to the transform position after its validity was checked
            CheckVelocity();
            if (!bCrouching && velocity.x != 0f && bGrounded)
            {
                GameManager.Instance.MakeNoise(walkNoiseRadius, transform.position);
            }
            anim.SetFloat("Velocity", GetValue(velocity.x * 100f));
            //if (velocity.x > 0.0001f || velocity.x < -0.0001f)
            //{
            //    anim.SetFloat("Velocity", 1f);
            //}
            //else
            //{
            //    anim.SetFloat("Velocity", 0f);
            //}
            transform.position += velocity;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector2.up * 0.2f);
    //    Gizmos.DrawRay(transform.position + Vector3.up * -0.4f + Vector3.right * 0.4f, Vector2.left * 0.2f);
    //}


    #region HelperMethods

    IEnumerator ShootAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (!shots.isPlaying)
        {
            shots.PlayOneShot(shots.clip);
        }
    }

    /// <summary>
    /// Return only the value and not the algebraic sign
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private float GetValue(float number)
    {
        if(number > 0f)
        {
            return number;
        }
        if(number < 0f)
        {
            return -number;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Necessary changes to make the player die and a new one to be activated
    /// </summary>
    public void Die()
    {
        heartBeatAudioSource.Stop();
        // Delete the reference to this gameObject on the camera to cause the next player to get activated
        cam.GetComponentInParent<FollowPlayer>().player = null;

        // Disable the script
        gameObject.GetComponent<PlayerController>().enabled = false;
    }

    /// <summary>
    /// Flips the Sprite when the player walks to the left and flips back when he walks to the right
    /// </summary>
    private void Flip()
    {
        if (input.Horizontal < 0f)
        {
            rend.flipX = true;
        }
        else if (input.Horizontal > 0f)
        {
            rend.flipX = false;
        }
    }

    #region Physics Helper

    /// <summary>
    /// Make sure the velocity does not violate the laws of physics in this game
    /// </summary>
    private void CheckVelocity()
    {
        // Check for ground under the player
        if (bGrounded && velocity.y < 0)
        {
            velocity.y = 0;
        }

        // Checking for colliders to the sides
        if (WallInWay())
        {
            velocity.x = 0f;
        }

        // Make sure, velocity in y axis does not get over limit
        if (velocity.y < veloYLimit)
        {
            velocity.y = veloYLimit;
        }
        // Check if something is above the player and let him bounce down again relative to the force he went up with
        if (RaycastForTag("Ground", rays.top) && velocity.y > 0)
        {
            velocity.y = -velocity.y / 2;
        }
    }

    /// <summary>
    /// Checks if there are walls in the direction the player is facing
    /// </summary>
    /// <returns> True if there is a wall. False when there is none</returns>
    private bool WallInWay()
    {
        // When the player looks left and there is a wall in the wall, set bOnWall to true and return true
        if (rend.flipX == true)
        {
            if (RaycastForTag("Ground", rays.upperLeft, rays.lowerLeft))
            {
                bOnWall = true;
                return true;
            }
        }
        // Otherwise when the player looks right and there is a wall in the way, set bOnWall to true and return true
        else if (rend.flipX == false)
        {
            if (RaycastForTag("Ground", rays.upperRight, rays.lowerRight))
            {
                bOnWall = true;
                return true;
            }
        }
        // Otherwise set bOnwall to false and return false
        bOnWall = false;
        return false;
    }

    #endregion

    #region Raycast Helper

    /// <summary>
    /// Checks if there is a raycast of the given in parameters hitting an object with the right tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="rayArray"></param>
    /// <returns> True if there was any raycast hitting an object with the right tag. False if there was none.</returns>
    private bool RaycastForTag(string tag, params RaycastHit2D[] rayArray)
    {
        // Iterates through the given raycasts and checks if one of them hits a collider, tagged as the given tag
        for (int i = 0; i < rayArray.Length; i++)
        {
            if (rayArray[i].collider != null)
            {
                if (rayArray[i].collider.tag == tag)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Check every raycast from the raycasts struct and return the first one, which found an object which matched the tag 
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="rayArray"></param>
    /// <returns> The first raycast who hit an object with the right tag</returns>
    private RaycastHit2D? WhichRaycastForTag(string tag, params RaycastHit2D[] rayArray)
    {
        // Iterates through the given raycasts and returns the raycast hit which did hit a collider, tagged with the given tag
        for (int i = 0; i < rayArray.Length; i++)
        {
            if (rayArray[i].collider != null)
            {
                if (rayArray[i].collider.tag == tag)
                {
                    return rayArray[i];
                }
            }
        }
        return null;
    }

    #endregion

    #region Coroutines

    /// <summary>
    /// Stop the time and make it run again after n frames
    /// </summary>
    /// <param name="frameAmount"></param>
    /// <returns></returns>
    IEnumerator StopTimeForFrames(int frameAmount)
    {
        Time.timeScale = 0f;
        for (int i = 0; i < frameAmount; i++)
        {
            yield return new WaitForEndOfFrame();
        }
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Makes the given audiosource fade out over the given time
    /// </summary>
    /// <param name="audioSource"></param>
    /// <param name="FadeTime"></param>
    /// <returns></returns>
    IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        // Slowly fade out the volume 
        while (audioSource.volume > 0f)
        {
            audioSource.volume -= Time.deltaTime / 4;

            yield return new WaitForEndOfFrame();
        }
        // Make the sound stop and set the volume back to 1
        audioSource.Stop();
        audioSource.volume = 1f;
        yield break;
    }

    #endregion

    #endregion

    #region Jump

    /// <summary>
    /// Start the Jump process
    /// </summary>
    private void HandleJump()
    {
        if (input.Jump == 2 && bGrounded)
        {
            Jump();
        }
        // Make the player fall less fast when still holding the jump button
        if (input.Jump == 1 && !bGrounded)
        {
            velocity += new Vector3(0f, fallFactor * Time.deltaTime);
        }
        // Make the player fall faster when not holding the jump button anymore
        else if (!bGrounded)
        {
            velocity -= new Vector3(0f, fallFactor * Time.deltaTime);
        }
    }

    private void Jump()
    {
        anim.SetBool("Jumping", true);
        velocity = new Vector3(0f, jumpForce * Time.deltaTime);
    }

    #endregion

    /// <summary>
    /// Make the player crouch
    /// </summary>
    private void Crouch()
    {
        bCrouching = true;
        anim.SetBool("Crouching", true);
    }

    /// <summary>
    /// Checks if the player is on the ground or not
    /// </summary>
    private void CheckGrounded()
    {
        // Count down the counter after the player fell through a ground gate
        if(fellThroughCounter > 0f)
        {
            fellThroughCounter -= Time.fixedDeltaTime;
        }
        // Check if the player is grounded using the bottom raycasts of the player
        if (RaycastForTag("Ground", rays.bottomLeft, rays.bottomRight))
        {
            bGrounded = true;
            if(anim.GetBool("Jumping") == true)
            {
                anim.SetBool("Jumping", false);
            }
        }
        // Make the player fall through ground gates when the player holds down the left analog stick or walk over the ground gates if he chooses not to hold down the stick
        else if (RaycastForTag("GroundGate", rays.bottomRight, rays.bottomLeft))
        {
            if(input.Vertical >= 0 && fellThroughCounter <= 0f)
            {
                bGrounded = true;
                if (anim.GetBool("Jumping") == true)
                {
                    anim.SetBool("Jumping", false);
                }
            }
            else
            {
                bGrounded = false;
                fellThroughCounter = 0.3f;
            }
        }
        // Otherwise the player is not grounded
        else
        {
            bGrounded = false;
        }
    }
}
