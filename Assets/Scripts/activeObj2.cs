using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveObj2 : MonoBehaviour
{
    public GameObject objectToActivate;

    void Start()
    {
        objectToActivate.SetActive(false);
    }

    public void ActiveObj2nd()
    {
        objectToActivate.SetActive(true);
    }


}