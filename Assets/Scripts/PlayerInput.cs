﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The Script, returning the input, used to the CharController Script, which converts the given information into actions the player can do
/// </summary>
public class PlayerInput : MonoBehaviour, IInput
{

    // Prevents the controller from reading tiny input, caused by old sticks
    [Range(0, 1)] [SerializeField] float controllerThreshhold;

    #region Axis

    // The input for horizontal movement
    public float Horizontal
    {
        get
        {
            if (Input.GetAxis("Horizontal") >= controllerThreshhold || Input.GetAxis("Horizontal") <= -controllerThreshhold)
            {
                return Input.GetAxis("Horizontal");
            }
            return 0f;
        }
    }

    // The input for vertical movement
    public float Vertical
    {
        get
        {
            if (Input.GetAxis("Vertical") >= controllerThreshhold || Input.GetAxis("Vertical") <= -controllerThreshhold)
            {
                return Input.GetAxis("Vertical");
            }
            return 0f;
        }
    }

    #endregion

    #region Actions

    // Check if jump button is pressed or holded
    public int Jump
    {
        get
        {
            if (Input.GetButtonDown("Jump"))
            {
                return 2;
            }
            else if (Input.GetButton("Jump"))
            {
                return 1;
            }
            return 0;
        }
    }

    // Look for Input for Dodge
    public bool Shout
    {
        get
        {
            if (Input.GetButtonDown("Shout"))
            {
                return true;
            }
            return false;
        }
    }

    // Look for Input for Crouching
    public bool Crouch
    {
        get
        {
            if(Input.GetButton("Crouch"))
            {
                return true;
            }
            return false;
        }
    }

    // Look for Input for Interacting with objects
    public bool Interact
    {
        get
        {
            if(Input.GetButtonDown("Interact"))
            {
                return true;
            }
            return false;
        }
    }

    #endregion

}