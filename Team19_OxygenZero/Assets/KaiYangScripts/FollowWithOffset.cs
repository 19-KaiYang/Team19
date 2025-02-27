using UnityEngine;

public class FollowWithOffset : MonoBehaviour
{
    public Transform target; 
    public float heightOffset = 10f; 

    void Update()
    {
        if (target != null)
        {
           
            transform.position = new Vector3(target.position.x, target.position.y + heightOffset, target.position.z);
        }
    }
}