using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShuttleStatus : MonoBehaviour
{
    public GameObject engineMissing, engineFound;

    // Start is called before the first frame update
    void Start()
    {
        engineMissing.SetActive(true);
        engineFound.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EngineFound()
    {
        engineFound.SetActive(true);
        engineMissing.SetActive(false);
    }

    public void LaunchBtn()
    {
        SceneManager.LoadScene("WinScene");
    }
}
