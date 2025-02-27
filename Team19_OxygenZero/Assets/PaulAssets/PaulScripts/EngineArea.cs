using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EngineArea : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Item")
        {
            ObjectData EngineData = other.gameObject.GetComponent<ObjectData>();

            if (EngineData != null)
            {
                if (EngineData.item.itemName == "Engine")
                {
                    SceneManager.LoadScene("WinScene");
                }
            }
        }
    }
}