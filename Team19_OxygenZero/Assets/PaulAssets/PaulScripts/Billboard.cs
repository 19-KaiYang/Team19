using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform childObject;  // Assign the child GameObject in Inspector
    private GameObject player;        // Assign the player's transform in Inspector
    private float activationRange = 5f; // Distance threshold


    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        childObject = transform.GetChild(0);
    }
    // Update is called once per frame
    void Update()
    {
        Quaternion rotation = Camera.main.transform.rotation;
        transform.LookAt(transform.position + rotation * Vector3.forward, rotation * Vector3.up);
        EnablePrompt();
    }

    public void EnablePrompt()
    {
        if (player == null || childObject == null)
            return;

        // Calculate the squared distance for better performance
        float distanceSqr = (player.transform.position - childObject.transform.position).sqrMagnitude;
        float rangeSqr = activationRange * activationRange;

        // Enable child if within range, disable otherwise
        childObject.gameObject.SetActive(distanceSqr <= rangeSqr);
    }


}
