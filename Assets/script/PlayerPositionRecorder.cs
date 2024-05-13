using UnityEngine;

public class PlayerPositionRecorder : MonoBehaviour
{
    public static Vector3 playerPosition;
    public static Quaternion rotation;

    void Start()
    {
        playerPosition = transform.position;
        rotation = transform.rotation;
        Debug.Log("Position of " +  ": " + playerPosition);
    }
}

