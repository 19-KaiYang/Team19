using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour
{
    [SerializeField] public bool DoorCanOpen = false; // Exposed to RoomController

    public void SetDoorStatus(bool status)
    {
        DoorCanOpen = status;
    }

    public bool GetDoorStatus()
    {
        return DoorCanOpen;
    }

    public void TriggerDoorAnimation()
    {
        Animator doorAnimator = this.GetComponentInChildren<Animator>();
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("IsDoorOpen", true);
        }
        else
        {
            Debug.LogWarning($"No Animator found in child of {this.name}");
        }
    }
}

