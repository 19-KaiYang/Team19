using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EngineArea : MonoBehaviour
{
    public ShuttleStatus terminal;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Item")
        {
            ObjectData EngineData = other.gameObject.GetComponent<ObjectData>();

            if (EngineData != null)
            {
                if (EngineData.item.itemName == "Engine")
                {
                    if (terminal != null) terminal.EngineFound();
                }
            }
        }
    }
}