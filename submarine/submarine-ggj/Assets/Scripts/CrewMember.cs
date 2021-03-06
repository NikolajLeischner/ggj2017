﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrewMember : MonoBehaviour {

	public AudioSource audioSource;
	public AudioClip step;
	public AudioClip climb;

    public float speed = 2.5f;

	public float climbSpeed = 1.5f;

	private Rigidbody rigidBody;

	public Transform cameraPosition;

	private int collidingLadders = 0;

    private Room actualRoom;

    private Coroutine lastRoutine = null;

    public bool hasTool;

	public WarningDisplay warningDisplay;

	public string toolsWarning = "You are carrying tools, and can now repair disabled rooms. Until you return the tools you cannot use anything else.";

	public void ShowToolsWarning() {
		warningDisplay.ShowWarning (toolsWarning);
	}

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
        return actualRoom != null ? actualRoom.GetRoomName().Equals("controls") && actualRoom.isUsed : false;
    }

	bool IsOnLadder() {
		return collidingLadders > 0;
	}

	private void UpdateLadders(int difference) {
		collidingLadders += difference;
		rigidBody.useGravity = collidingLadders == 0;
	}

	public void Act() {
        if (actualRoom != null && !IsUsingRoom())
        {
            actualRoom.isUsed = !actualRoom.isUsed;
            // Using the room
            if (actualRoom.isUsed)
            {
                // If the crew member has a tool
                if (this.hasTool && !actualRoom.isDisabled)
                {
                    // The room needs repair -> repair the room
                    // The room is the tools room, leave the tool (UseRoom)
                    // Otherwise do nothing (can't do something with a tool in hands !)
                    if (actualRoom.needsRepair)
                    {
                        lastRoutine = StartCoroutine(actualRoom.RepairRoom());
                    }
                    else if (actualRoom.GetRoomName().Equals(Constants.TOOLS))
                    {
                        lastRoutine = StartCoroutine(actualRoom.UseRoom());
                    }
                    else
                    {
                        actualRoom.isUsed = !actualRoom.isUsed;
                    }
                }
                else if (!actualRoom.needsRepair && !actualRoom.isDisabled && !this.hasTool)
                {
                    // If the room is OK and he has no tool, just use the room
                    lastRoutine = StartCoroutine(actualRoom.UseRoom());
                }
                else
                {
                    actualRoom.isUsed = !actualRoom.isUsed;
                }
            }
        }
        else if (IsUsingRoom())
        {
            actualRoom.isUsed = !actualRoom.isUsed;
			StartCoroutine(actualRoom.UseDoors(actualRoom.doors));
            StopCoroutine(lastRoutine);
			StartCoroutine (actualRoom.StopUsingRoom ());
        }
	}

	private float CurrentSpeed() {
		float currentSpeed = IsOnLadder () ? climbSpeed : speed;
		return currentSpeed * Time.fixedDeltaTime;
	}

	void PlayClip(AudioClip clip) {
		audioSource.pitch = 0.5f + 0.5f * Random.value;
		if (!audioSource.isPlaying)
			audioSource.PlayOneShot (clip);
	}

	public void Move(int horizontal, int vertical) {
		if (!IsOnLadder () && !IsControllingSub())
			vertical = 0;

        if (!IsControllingSub() && !IsUsingRoom())
        {
            var currentPosition = rigidBody.transform.position;

            var movement = new Vector3(horizontal * CurrentSpeed(), vertical * CurrentSpeed(), 0);

			if (IsOnLadder () && vertical != 0)
				PlayClip (climb);
			else if (horizontal != 0)
				PlayClip (step);

            rigidBody.MovePosition(currentPosition + movement);
		} else if (IsControllingSub() && actualRoom.submarine.HasEnergy(1))
        {
			actualRoom.submarine.AdjustDepth(direction: vertical);
        }
	}
}
