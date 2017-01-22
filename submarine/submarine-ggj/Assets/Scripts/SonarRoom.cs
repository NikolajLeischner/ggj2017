﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonarRoom : Room {

    public float duration = 5;

    void Start () {
        this.collider = GetComponent<Collider>();
        this.roomName = Constants.SONAR;
    }

	void FixedUpdate () {
        Lights();
    }

    public override IEnumerator UseRoom()
    {
		if (submarine.HasEnergy(submarine.sonarEnergyCost)) {
			StartCoroutine(UseDoors(doors));
			submarine.energy -= submarine.sonarEnergyCost;
			sonar.SetActive(!sonar.active);
			yield return new WaitForSeconds(duration);
			this.isUsed = !this.isUsed;
			sonar.SetActive(!sonar.active);
			StartCoroutine(UseDoors(doors));
		}
    }
}
