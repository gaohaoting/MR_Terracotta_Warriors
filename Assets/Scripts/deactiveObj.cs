using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class deactiveObj : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject objectToDeactive1;
    public GameObject objectToDeactive2;

    // Update is called once per frame
    public void DactiveObj()
    {
        objectToDeactive1.SetActive(false);
        objectToDeactive2.SetActive(false);
    }
}
