﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("Physics"), SerializeField] float gravity = 1f;
    [SerializeField] float veloYLimit = -1f;
    [SerializeField] float wallSlideSpeed = 0.4f;

    [SerializeField] float jumpForce = 1f;
    [SerializeField] float jumpCooldown = 0.1f;
    [SerializeField] float fallFactor = 0.2f;

    private Vector3 velocity;
    private PlayerInput input;
    private SpriteRenderer rend;
    private AudioSource audioSource;

    [SerializeField] AudioClip[] audioClips;

    private bool bJumpable = true;
    private bool bOnWall = false;
    private bool bGrounded = false;

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

    [SerializeField] float speed = 3f;

	// Use this for initialization
	void Start () {
        input = GetComponent<PlayerInput>();
        rend = GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        //if(input.Dodge)
        //{
        //    print("Dodge");
        //}
        //if(input.Crouch)
        //{
        //    print("Crouch");
        //}
        //if(input.Interact)
        //{
        //    print("Grab or throw");
        //}
        HandleJump();
        Flip();
	}

    private void FixedUpdate()
    {
        #region Raycast Initialization

        // Update all the different raycast hit values to calculate physics
        rays.bottomRight = Physics2D.Raycast(transform.position + Vector3.right * 0.1f + Vector3.down * 0.4f, Vector2.down, 0.2f);
        rays.bottomLeft = Physics2D.Raycast(transform.position + Vector3.right * -0.2f + Vector3.down * 0.4f, Vector2.down, 0.2f);

        rays.upperRight = Physics2D.Raycast(transform.position + Vector3.up * 0.3f + Vector3.right * 0.4f, Vector2.left, 0.2f);
        rays.lowerRight = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.right * 0.4f, Vector2.left, 0.2f);

        rays.upperLeft = Physics2D.Raycast(transform.position + Vector3.up * 0.3f + Vector3.left * 0.4f, Vector2.right, 0.2f);
        rays.lowerLeft = Physics2D.Raycast(transform.position + Vector3.up * -0.4f + Vector3.left * 0.4f, Vector2.right, 0.2f);

        rays.top = Physics2D.Raycast(transform.position + Vector3.up * 0.4f, Vector2.up, 0.3f);

        anyRaycast[0] = rays.bottomRight;
        anyRaycast[1] = rays.bottomLeft;
        anyRaycast[2] = rays.lowerLeft;
        anyRaycast[3] = rays.upperLeft;
        anyRaycast[4] = rays.lowerRight;
        anyRaycast[5] = rays.upperRight;
        anyRaycast[6] = rays.top;

        #endregion

        velocity = new Vector3(input.Horizontal * speed * Time.fixedDeltaTime, velocity.y);

        CheckGrounded();
        if (!bGrounded)
        {
            velocity.y -= gravity * Time.fixedDeltaTime;
        }

        CheckVelocity();
        transform.position += velocity;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawRay(transform.position + Vector3.right * 0.1f + Vector3.down * 0.4f, Vector2.down * 0.2f);
    //    Gizmos.DrawRay(transform.position + Vector3.right * -0.2f + Vector3.down * 0.4f, Vector2.down * 0.2f);
    //}


    #region HelperMethods

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
        if (transform.localScale.x < 0)
        {
            if (RaycastForTag("Ground", rays.upperLeft, rays.lowerLeft))
            {
                bOnWall = true;
                return true;
            }
        }
        else if (transform.localScale.x > 0)
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
        if (input.Jump == 2 && bGrounded && bJumpable)
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
        velocity += new Vector3(0f, jumpForce * Time.deltaTime);
        bJumpable = false;
        StartCoroutine(JumpCooldown(jumpCooldown));
    }

    IEnumerator JumpCooldown(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        bJumpable = true;
    }

    #endregion

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
