using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    #region Fields

    [Header("Physics"), SerializeField] float gravity = 1f;
    [SerializeField] float veloYLimit = -1f;

    [SerializeField] float jumpForce = 1f;
    [SerializeField] float fallFactor = 0.2f;

    [SerializeField] float speed = 3f;
    private float actualSpeed;

    [SerializeField] float wallSlideSpeed = 0.4f;
    private float heightToPullUpTo;

    private Vector3 velocity;
    private PlayerInput input;
    private SpriteRenderer rend;
    private AudioSource audioSource;
    private Camera cam;

    private LayerMask layersToCollideWith;
    private LayerMask layersToInteractWith;

    private GameObject grabbedObject;

    [SerializeField] float detectionTime = 2f; // The amount of seconds it takes to get detected
    private float detectionCounter;

    [SerializeField] AudioClip[] audioClips;

    private bool bJumpable = true;
    private bool bOnWall = false;
    private bool bGrounded = false;
    private bool bCrouching = false;

    private Vector3 eyes;

    struct PlayerRaycasts // To store the informations of raycasts around the player to calculate physics
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomRight;
        public RaycastHit2D upperLeft;
        public RaycastHit2D lowerLeft;
        public RaycastHit2D upperRight;
        public RaycastHit2D lowerRight;
        public RaycastHit2D top;
    }
    PlayerRaycasts rays;

    RaycastHit2D[] anyRaycast = new RaycastHit2D[7];

    #endregion

    // Use this for initialization
    //void Start () {
    //    input = GetComponent<PlayerInput>();
    //    rend = GetComponent<SpriteRenderer>();
    //    cam = Camera.main;
    //    // Get the layerMask for collision
    //    int layer = LayerMask.NameToLayer("Ground");
    //    int layer2 = LayerMask.NameToLayer("EnemiesLight");
    //    layersToCollideWith = 1 << layer;
    //    layersToInteractWith = 1 << layer2;
    //    layersToCollideWith = layersToCollideWith | layersToInteractWith;

    //    //Make shadows happen
    //    rend.receiveShadows = true;

    //    detectionCounter = detectionTime;
    //}

    private void OnEnable()
    {
        input = GetComponent<PlayerInput>();
        rend = GetComponent<SpriteRenderer>();
        cam = Camera.main;
        // Get the layerMask for collision
        int layer = LayerMask.NameToLayer("Ground");
        int layer2 = LayerMask.NameToLayer("EnemiesLight");
        layersToCollideWith = 1 << layer;
        layersToInteractWith = 1 << layer2;
        layersToCollideWith = layersToCollideWith | layersToInteractWith;

        //Make shadows happen
        rend.receiveShadows = true;

        detectionCounter = detectionTime;
    }

    // Update is called once per frame
    void Update () {
        //if(input.Dodge)
        //{
        //    print("Dodge");
        //}
        //if(input.Interact)
        //{
        //    print("Grab or throw");
        //}
        if (input.Crouch)
        {
            Crouch();
        }
        else
        {
            bCrouching = false;
            rend.flipY = false;
        }
        if(!bOnWall)
        {
            heightToPullUpTo = 0f;
        }
        Flip();
        if (RaycastForTag("EnemyLight", anyRaycast))
        {
            if(CheckForDetected())
            {
                detectionCounter -= Time.deltaTime;
                if(detectionCounter <= 0f)
                {
                    Die();
                }
            }
        }
        // Count down the detection Counter when not in sight of a Guard
        else
        {
            if(detectionCounter < detectionTime)
            {
                detectionCounter += Time.deltaTime / 2;
            }
            else if(detectionCounter > detectionTime)
            {
                detectionCounter = detectionTime;
            }
        }
        // Detect Checkpoint in range and activate him
        if (RaycastForTag("Checkpoint", anyRaycast))
        {
            RaycastHit2D newCheckpoint = (RaycastHit2D)WhichRaycastForTag("Checkpoint", anyRaycast);
            GameManager.Instance.currentCheckpoint = newCheckpoint.collider.gameObject.transform.position;
        }
    }

    private void FixedUpdate()
    {
        #region Raycast Initialization

        // Update all the different raycast hit values to calculate physics
        rays.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * 0.1f + Vector3.down * 0.4f, Vector2.down, 0.2f, layersToCollideWith);
        rays.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * -0.2f + Vector3.down * 0.4f, Vector2.down, 0.2f, layersToCollideWith);

        rays.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.right * 0.4f, Vector2.left, 0.2f);
        rays.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.left * 0.4f, Vector2.right, 0.2f);
        
        rays.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.3f + Vector3.right * 0.4f, Vector2.left, 0.2f);
        rays.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.3f + Vector3.left * 0.4f, Vector2.right, 0.2f);

        rays.top = Physics2D.Raycast(transform.position + Vector3.up * 0.4f, Vector2.up, 0.2f);

        anyRaycast[0] = rays.bottomRight;
        anyRaycast[1] = rays.bottomLeft;
        anyRaycast[2] = rays.lowerLeft;
        anyRaycast[3] = rays.upperLeft;
        anyRaycast[4] = rays.lowerRight;
        anyRaycast[5] = rays.upperRight;
        anyRaycast[6] = rays.top;

        #endregion

        if(bCrouching)
        {
            actualSpeed = speed / 4;
            eyes = transform.position;

        }
        else
        {
            actualSpeed = speed;
            eyes = new Vector3(transform.position.x, transform.position.y + 0.8f);
        }

        velocity = new Vector3(input.Horizontal * actualSpeed * Time.fixedDeltaTime, velocity.y);

        CheckGrounded();

        if (!bCrouching)
        {
            HandleJump();
        }

        // Apply Gravity
        if (!bGrounded)
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
        }

        CheckVelocity();
        transform.position += velocity;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, Vector2.up * 0.2f);
    //    Gizmos.DrawRay(transform.position + Vector3.up * -0.4f + Vector3.right * 0.4f, Vector2.left * 0.2f);
    //}


    #region HelperMethods
    
    private void Die()
    {
        gameObject.GetComponent<PlayerController>().enabled = false;
        // Rotate the old player to show he ded
        Quaternion newRotation = new Quaternion();
        newRotation.eulerAngles = new Vector3(0f, 0f, 90f);
        gameObject.transform.rotation = newRotation;
        // Delete the reference to this Player in the cameraController
        cam.GetComponentInParent<CameraController>().player = null;
    }

    /// <summary>
    /// Check if the player is detected or if he is behind an object, blocking the view
    /// </summary>
    /// <returns></returns>
    private bool CheckForDetected()
    {
        GameObject enemyToAlarm = null;
        RaycastHit2D hit = (RaycastHit2D)WhichRaycastForTag("EnemyLight", anyRaycast);
        bool bDetected = false;
        Vector3 direction = hit.collider.gameObject.transform.position - eyes;
        RaycastHit2D[] hits = Physics2D.RaycastAll(eyes, direction, direction.magnitude);
        Debug.DrawRay(eyes, direction);
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length - 1; i++)
            {
                if (hits[i].collider.tag == "Ground" || hits[i].collider.tag == "HideBehind")
                {
                    return false;
                }
                else if(hits[i].collider.tag == "EnemyLight")
                {
                    bDetected = true;
                    enemyToAlarm = hits[i].collider.gameObject;
                }
            }
        }
        if(enemyToAlarm != null)
        {
            enemyToAlarm.GetComponentInParent<GeneralEnemy>().bAlarmed = true;
        }
        return bDetected;
    }

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

        // Check for possible Wallslide
        if (bOnWall && !bGrounded && HoldingInDirection() && velocity.y < 0)
        {
            velocity.y = - wallSlideSpeed * Time.fixedDeltaTime;
        }

        // Check if something is above the player and let him bounce down again relative to the force he went up with
        if (RaycastForTag("Ground", rays.top) && velocity.y > 0)
        {
            velocity.y = -velocity.y / 2;
        }
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

    /// <summary>
    /// Checks if there are walls in the direction the player is facing
    /// </summary>
    /// <returns> True if there is a wall. False when there is none</returns>
    private bool WallInWay()
    {
        if (rend.flipX == true)
        {
            if (RaycastForTag("Ground", rays.upperLeft, rays.lowerLeft))
            {
                bOnWall = true;
                return true;
            }
        }
        else if (rend.flipX == false)
        {
            if (RaycastForTag("Ground", rays.upperRight, rays.lowerRight))
            {
                bOnWall = true;
                return true;
            }
        }
        bOnWall = false;
        return false;
    }

    /// <summary>
    /// Checks if the player is holding the direction, hes facing in
    /// </summary>
    /// <returns> True if the player is holding in the direction, he is facing. False if he is not.</returns>
    private bool HoldingInDirection()
    {
        if (input.Horizontal < 0 && rend.flipX == true)
        {
            return true;
        }
        else if (input.Horizontal > 0 && rend.flipX == false)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if there is a raycast of the given in parameters hitting an object with the right tag
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="rayArray"></param>
    /// <returns> True if there was any raycast hitting an object with the right tag. False if there was none.</returns>
    private bool RaycastForTag(string tag, params RaycastHit2D[] rayArray)
    {
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
    /// Play the element of the audioclip array at the indice given in as a parameter
    /// </summary>
    /// <param name="indice"></param>
    private void PlayClip(int indice)
    {
        audioSource.clip = audioClips[indice];
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    #endregion

    #region Jump

    /// <summary>
    /// Start the Jump process
    /// </summary>
    private void HandleJump()
    {
        if (input.Jump == 2 && bGrounded)
        {
            if (!bOnWall)
            {
                Jump();
            }
            else if(bOnWall)
            {
                //Play pull up animation
                //RaycastHit2D hit = (RaycastHit2D)WhichRaycastForTag("Ground", rays.upperLeft, rays.lowerLeft, rays.lowerRight, rays.upperRight);
                //if(hit.collider != null)
                //{
                //    heightToPullUpTo = hit.collider.bounds.size.y;
                //    transform.position += Vector3.up * heightToPullUpTo;
                //}
            }
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
        velocity = new Vector3(0f, jumpForce * Time.deltaTime);
    }

    #endregion

    private void Crouch()
    {
        bCrouching = true;
        rend.flipY = true;
    }

    /// <summary>
    /// Checks if the player is on the ground or not
    /// </summary>
    private void CheckGrounded()
    {
        // When the bottom left collider hit something tagged as ground
        if (RaycastForTag("Ground", rays.bottomLeft) || RaycastForTag("Ground", rays.bottomRight))
        {
            bGrounded = true;
            //anim.SetBool("Grounded", true);
        }
        // Otherwise the player is not grounded
        else
        {
            bGrounded = false;
            //anim.SetBool("Grounded", false);
        }
    }
}
