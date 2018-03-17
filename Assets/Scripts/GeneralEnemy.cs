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
                durationUntilNotAlarmedCounter = durationUntilNotAlarmed;
            }
            bDetected = value;
        }
    }

    #endregion

    #region Fields

    protected GameObject player;
    protected Vector3 toPlayer;

    // The layer mask used to collide with only walls
    protected LayerMask layersToCollideWith;
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

    Vector3 pointToCheck = Vector3.zero;

    [SerializeField] float durationUntilNotAlarmed = 3f;
    protected float durationUntilNotAlarmedCounter;

    [SerializeField] float moveSpeed = 1f;
    [SerializeField] float stoppingDistance = 0.5f;

    #endregion

    protected virtual void GeneralInitialization()
    {
        coll = GetComponent<BoxCollider2D>();
        // Get the layerMask for collision
        int layer = LayerMask.NameToLayer("Ground");
        layersToCollideWith = 1 << layer;
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

    // Update is called once per frame
    void Update()
    {

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
        }
        if(!BDetected && pointToCheck == Vector3.zero)
        {
            SimpleMove();
        }
        else if(BDetected)
        {
            DetectedBehavior();
        }
        else if(toPlayer != Vector3.zero)
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
            if (toPoint.x > stoppingDistance || toPoint.x < -stoppingDistance)
            {
                transform.position += new Vector3(moveSpeed * 2f * transform.localScale.x * Time.deltaTime, 0f);
                StartCoroutine(LeaveItAfterSeconds(6f));
            }
            if (eyes.GetComponent<BoxCollider2D>().bounds.Contains(pointToCheck) && !eyes.GetComponent<BoxCollider2D>().bounds.Contains(player.transform.position))
            {
                pointToCheck = Vector3.zero;
            }
        }
        if(durationUntilNotAlarmedCounter > 0f)
        {
            durationUntilNotAlarmedCounter -= Time.deltaTime;
        }
        else if(durationUntilNotAlarmedCounter <= 0f && BDetected)
        {
            BDetected = false;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawRay(transform.position + new Vector3(coll.bounds.extents.x, 0.0f), Vector2.right * 0.2f);
    //    Gizmos.DrawRay(transform.position + new Vector3(-coll.bounds.extents.x, 0.0f), Vector2.left * 0.2f);
    //}

    public void GetAlerted(Vector3 newPointToCheck)
    {
        pointToCheck = newPointToCheck;
    }

    protected void GetNewPlayer(GameObject newPlayer)
    {
        print("Got it");
        player = newPlayer;
    }

    IEnumerator LeaveItAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pointToCheck = Vector3.zero;
    }

    protected virtual void DetectedBehavior()
    {
        //Quaternion newRotation = Quaternion.LookRotation(toPlayer);
        //newRotation.eulerAngles = new Vector3(newRotation.eulerAngles.x, 0f, newRotation.eulerAngles.z);
        //eyes.transform.rotation = newRotation;
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
            if (toPlayer.x > stoppingDistance || toPlayer.x < -stoppingDistance)
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

