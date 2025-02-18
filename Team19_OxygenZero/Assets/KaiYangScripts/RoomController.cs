using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public GameObject topDoor, bottomDoor, leftDoor, rightDoor;

    public void SetDoors(bool top, bool bottom, bool left, bool right)
    {
        if (topDoor) topDoor.SetActive(top);
        if (bottomDoor) bottomDoor.SetActive(bottom);
        if (leftDoor) leftDoor.SetActive(left);
        if (rightDoor) rightDoor.SetActive(right);
    }
}
