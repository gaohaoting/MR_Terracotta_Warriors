using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class switchObj : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject objectA;
    public GameObject objectB;
    public void DestoryObjectA()
    {
        if (objectA.activeSelf)
        {
            StartCoroutine(DeactivateAfterTime(3.0f));
        }
    }
    private IEnumerator DeactivateAfterTime(float delay)
    {
        // Wait for the specified delay (3 seconds)
        yield return new WaitForSeconds(delay);

        // Deactivate object A
        if (objectA != null && objectA.activeSelf)
        {
            // Destroy object A if it is still active
            Destroy(objectA);
        }

        // Activate object B if it exists
        if (objectB != null)
        {
            objectB.SetActive(true);
        }
    }
}
