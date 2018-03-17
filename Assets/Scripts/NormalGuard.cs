using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The actual script on normal guards
/// </summary>
public class NormalGuard : GeneralEnemy
{

	void Start () {
        GeneralInitialization();
	}
	
	void Update () {
        GeneralBehavior();
	}
}
