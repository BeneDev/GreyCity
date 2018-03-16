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

    public bool BAlarmed
    {
        get
        {
            return bAlarmed;
        }
        set
        {
            if(value == true)
            {
                durationUntilNotAlarmedCounter = durationUntilNotAlarmed;
            }
            bAlarmed = value;
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

    public bool bAlarmed = false;
    [SerializeField] float durationUntilNotAlarmed = 3f;
    protected float durationUntilNotAlarmedCounter;

    [SerializeField] float moveSpeed = 1f;

    #endregion

    // Use this for initialization
    void Start()
    {

    }

    protected virtual void GeneralInitialization()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        coll = GetComponent<BoxCollider2D>();
        // Get the layerMask for collision
        int layer = LayerMask.NameToLayer("Ground");
        layersToCollideWith = 1 << layer;
        rend = GetComponent<SpriteRenderer>();
        if(transform.localScale.x == -1)
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
        toPlayer = player.transform.position - transform.position;
        if(!bAlarmed)
        {
            SimpleMove();
        }
        else
        {
            AlarmedBehavior();
        }
        if(durationUntilNotAlarmedCounter > 0f)
        {
            durationUntilNotAlarmedCounter -= Time.deltaTime;
        }
        else if(durationUntilNotAlarmedCounter <= 0f && BAlarmed)
        {
            BAlarmed = false;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawRay(transform.position + new Vector3(coll.bounds.extents.x, 0.0f), Vector2.right * 0.2f);
    //    Gizmos.DrawRay(transform.position + new Vector3(-coll.bounds.extents.x, 0.0f), Vector2.left * 0.2f);
    //}

    protected virtual void AlarmedBehavior()
    {
        Quaternion newRotation = new Quaternion();
        newRotation.eulerAngles = new Vector3(eyes.transform.rotation.x, eyes.transform.rotation.y, toPlayer.z);
        eyes.transform.rotation = newRotation;
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

