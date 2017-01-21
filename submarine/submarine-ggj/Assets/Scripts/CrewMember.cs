﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewMember : MonoBehaviour {

    public float speed = 2.5f;

	public float climbSpeed = 1.5f;

	private Rigidbody rigidBody;

	private int collidingLadders = 0;

    private Room actualRoom;

    private Coroutine lastRoutine = null;

    void Start () {
		rigidBody = GetComponentInChildren<Rigidbody> ();
	}

	void OnTriggerEnter(Collider other) {
		if (other.tag == Constants.TAG_LADDER) {
			UpdateLadders (1);
		}
        if (other.tag == Constants.TAG_ROOM)
        {
            actualRoom = other.gameObject.GetComponent<Room>();
            actualRoom.crewMember = this;
        }
	}

	void OnTriggerExit(Collider other) {
		if (other.tag == Constants.TAG_LADDER) {
			UpdateLadders (-1);
		}
        if (other.tag == Constants.TAG_ROOM && actualRoom != null)
        {
            actualRoom.crewMember = null;
            actualRoom = null;
        }
    }

    public bool IsUsingRoom()
    {
        return actualRoom != null && actualRoom.isUsed && actualRoom.crewMember == this;
    }

    bool IsControllingSub()
    {
        return actualRoom != null ? actualRoom.getRoomName().Equals("controls") && actualRoom.isUsed : false;
    }

	bool IsOnLadder() {
		return collidingLadders > 0;
	}

	private void UpdateLadders(int difference) {
		collidingLadders += difference;
		rigidBody.useGravity = collidingLadders == 0;
		Debug.Log ("Update ladders (" + difference + ") -> " + collidingLadders);
	}

	public void Act() {
		if (actualRoom != null && !IsUsingRoom())
        {
            actualRoom.isUsed = !actualRoom.isUsed;
            if (actualRoom.isUsed)
            {
                lastRoutine = StartCoroutine(actualRoom.useRoom());
            }
        } else if (IsUsingRoom())
        {
            actualRoom.isUsed = !actualRoom.isUsed;
            StopCoroutine(lastRoutine);
        }
	}

	private float CurrentSpeed() {
		float currentSpeed = IsOnLadder () ? climbSpeed : speed;
		return currentSpeed * Time.fixedDeltaTime;
	}

	public void Move(int horizontal, int vertical) {
		if (!IsOnLadder () && !IsControllingSub())
			vertical = 0;

        if (!IsControllingSub() && !IsUsingRoom())
        {
            var currentPosition = rigidBody.transform.position;

            var movement = new Vector3(horizontal * CurrentSpeed(), vertical * CurrentSpeed(), 0);

            rigidBody.MovePosition(currentPosition + movement);
        } else if (IsControllingSub())
        {
			actualRoom.submarine.AdjustDepth(direction: vertical);
        }
	}
}