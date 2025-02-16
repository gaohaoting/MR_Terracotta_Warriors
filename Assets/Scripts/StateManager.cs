using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public GameObject[] GameObjectsToActive;
    public GameObject[] GameObjectsToDeactive;
    void Start()
    {
        
    }

    // Update is called once per frame
    public void ActiveObjects()
    {
        foreach (GameObject obj in GameObjectsToActive)
        {
            obj.SetActive(true);
        }
    }
    public void HideObjects()
    {
        foreach (GameObject obj in GameObjectsToDeactive)
        {
            obj.SetActive(false);
        }
    }
}
