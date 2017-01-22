﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlsRoom : Room {

	void Start () {
        this.collider = GetComponent<Collider>();
        this.roomName = Constants.CONTROLS;
    }

    void FixedUpdate () {
        Lights();
    }

    public override IEnumerator UseRoom()
    {
		if (submarine.HasEnergy(1))
			StartCoroutine(UseDoors(doors));
        yield return null;
    }
}
