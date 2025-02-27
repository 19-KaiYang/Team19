using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private AudioManager audioManager;

    public void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
    }

    public void PlayFootstep()
    {
        if (audioManager != null)
        {

            audioManager.PlaySFX("Metal_Footstep_Large1");

        }
    }

    public void PlayLightFootstep()
    {
        audioManager.PlaySFX("Modern_Metal_Walk_Small_5");
    }
}
