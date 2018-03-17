using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralEnemy : MonoBehaviour
{

    #region Properties

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

    public Vector3 PointToCheck
    {
        get
        {
            return pointToCheck;
        }
        set
        {
            pointToCheck = value;
        }
    }

    #endregion

    #region Fields

    protected GameObject player;
    protected Vector3 toPlayer;

    // The layer mask used to collide with only walls
    protected LayerMask layersToCollideWith;
    protected LayerMask layersBlockingView;
    protected BoxCollider2D coll;
    protected SpriteRenderer rend;

    struct Raycasts
    {
        public RaycastHit2D bottomLeft;
        public RaycastHit2D bottomMid;
        public RaycastHit2D bottomRight;
        public RaycastHit2D left;
        public RaycastHit2D right;
        public RaycastHit2D upperLeft;
        public RaycastHit2D upperRight;
    }
    private Raycasts rays;

    protected bool bLookLeft = false;

    [SerializeField] GameObject eyes;

    protected bool bDetected = false;

    protected bool bStandStill = false;

    protected bool bPlayerInSight = false;

    Vector3 pointToCheck = Vector3.zero;

    [SerializeField] float durationUntilNotDetected = 3f;
    protected float durationUntilNotDetectedCounter;

    [SerializeField] float timeToGiveUpAfter = 6f; // The amount of seconds after which the enemy will give up to look for the point where the alerting sound came from

    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float stoppingDistance = 0.5f;

    #endregion

    protected virtual void GeneralInitialization()
    {
        coll = GetComponent<BoxCollider2D>();
        // Get all layermasks
        int groundLayer = LayerMask.NameToLayer("Ground");
        int defaultLayer = LayerMask.NameToLayer("Default");
        layersToCollideWith = 1 << groundLayer;
        layersBlockingView = 1 << groundLayer;
        LayerMask defaultMask = 1 << defaultLayer;
        layersBlockingView = layersBlockingView | defaultMask;
        rend = GetComponent<SpriteRenderer>();
        GameManager.Instance.OnPlayerChanged += GetNewPlayer;
        if (transform.localScale.x == -1)
        {
            BLookLeft = true;
        }
        else
        {
            BLookLeft = false;
        }
        rend.receiveShadows = true;
        rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    protected virtual void GeneralBehavior()
    {
        #region Raycast Initialization

        // Update all of the rays, used to check for ground under the enemy
        rays.left = Physics2D.Raycast(transform.position + new Vector3(coll.bounds.extents.x, 0.0f), Vector2.right, 0.2f, layersToCollideWith);
        rays.right = Physics2D.Raycast(transform.position + new Vector3(-coll.bounds.extents.x, 0.0f), Vector2.left, 0.2f, layersToCollideWith);

        rays.upperLeft = Physics2D.Raycast(transform.position + new Vector3(coll.bounds.extents.x, 0.4f), Vector2.right, 0.2f, layersToCollideWith);
        rays.upperRight = Physics2D.Raycast(transform.position + new Vector3(-coll.bounds.extents.x, 0.4f), Vector2.left, 0.2f, layersToCollideWith);

        #endregion
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
        if(!BDetected && pointToCheck == Vector3.zero)
        {
            SimpleMove();
        }
        if (bPlayerInSight)
        {
            if (CheckForDetected())
            {
                BDetected = true;
            }
        }
        if(BDetected)
        {
            DetectedBehavior();
        }
        if (!BDetected && pointToCheck != Vector3.zero)
        {
            StartCoroutine(AlertedBehavior());
        }
        if (durationUntilNotDetectedCounter > 0f)
        {
            durationUntilNotDetectedCounter -= Time.deltaTime;
        }
        else if (durationUntilNotDetectedCounter <= 0f && BDetected)
        {
            BDetected = false;
        }
    }

    IEnumerator AlertedBehavior()
    {
        Vector3 toPoint = pointToCheck - transform.position;
        if (toPoint.x > 0)
        {
            BLookLeft = false;
        }
        else if (toPoint.x < 0)
        {
            BLookLeft = true;
        }
        yield return new WaitForSeconds(1f);
        if (!eyes.GetComponent<BoxCollider2D>().bounds.Contains(pointToCheck))
        {
            transform.position += new Vector3(moveSpeed * 0.75f * transform.localScale.x * Time.deltaTime, 0f);
            StartCoroutine(GiveUpAfterSeconds(timeToGiveUpAfter));
        }
        else
        {
            //Look Around
        }
    }
    
    IEnumerator GiveUpAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pointToCheck = Vector3.zero;
    }

    public void DontMoveForSeconds(float seconds)
    {
        bStandStill = true;
        StartCoroutine(WaitForMoveAgain(seconds));
    }

    IEnumerator WaitForMoveAgain(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        bStandStill = false;
    }

    private bool CheckForDetected()
    {
        Vector3 direction = player.GetComponent<PlayerController>().PlayerEyes - eyes.transform.position;
        Debug.DrawRay(eyes.transform.position, direction);
        //RaycastHit2D[] hits = Physics2D.RaycastAll(eyes.transform.position, direction, direction.magnitude, layersToCollideWith);
        //if (hits.Length > 0)
        //{
        //    for (int i = 0; i < hits.Length - 1; i++)
        //    {
        //        print(hits[i].collider.tag);
        //        if (hits[i].collider.tag == "Ground" || hits[i].collider.tag == "HideBehind")
        //        {
        //            return false;
        //        }
        //    }
        //}
        //print("So detected");
        //return true;
        if (Physics2D.Raycast(eyes.transform.position, direction, direction.magnitude, layersBlockingView))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    protected void GetNewPlayer(GameObject newPlayer)
    {
        player = newPlayer;
    }

    protected virtual void DetectedBehavior()
    {
        player.GetComponent<PlayerController>().DetectionCounter -= Time.deltaTime;
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
        }
    }

    /// <summary>
    /// Make the enemy turn around, whenever he faces the end of the platform he's walking on or a wall in front of him
    /// </summary>
    protected virtual void SimpleMove()
    {
        // When the enemy has walls either to the right or left side of him
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
}

