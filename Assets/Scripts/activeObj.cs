using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activeObj : MonoBehaviour
{
    public GameObject objectToActivate;
    void Start()
    {
        objectToActivate.SetActive(false);
    }
    public void ActiveObj()
    {
        objectToActivate.SetActive(true);
    }
}
