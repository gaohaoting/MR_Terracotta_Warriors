using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class activeObj3 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject objectToActivate;
    public GameObject objectToActivate2;

    // Update is called once per frame
    // void Start()
    // {
    //     objectToActivate.SetActive(false);
    //     objectToActivate2.SetActive(false);
    // }
    public void ActiveObj()
    {
        objectToActivate.SetActive(true);
        objectToActivate2.SetActive(true);
    }
}
