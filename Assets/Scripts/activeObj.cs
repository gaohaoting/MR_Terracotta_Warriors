using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activeObj : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject objectToActivate;
    void Start()
    {
        objectToActivate.SetActive(false); // Object to be activated
    }

    // Update is called once per frame
    public void ActiveObj()
    {
        objectToActivate.SetActive(true);
    }
}
