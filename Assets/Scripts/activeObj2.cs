using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activeObj2 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject objectToActivate;
    void Start()
    {
        objectToActivate.SetActive(false);
    }

    // Update is called once per frame
    public void ActiveObj2()
    {
        objectToActivate.SetActive(true);
        Debug.Log("detect");
    }
}
