using UnityEngine;
using Oculus.Interaction;

public class ActivateOnSelect : MonoBehaviour
{
    public GameObject objectToActivate; // Assign this in the inspector

    private InteractableUnityEventWrapper eventWrapper;

    private void Awake()
    {
        eventWrapper = GetComponent<InteractableUnityEventWrapper>();
        if (eventWrapper != null)
        {
            eventWrapper.WhenSelect.AddListener(ActivateObject);
        }
    }

    private void OnDestroy()
    {
        if (eventWrapper != null)
        {
            eventWrapper.WhenSelect.RemoveListener(ActivateObject);
        }
    }

    public void ActivateObject()
    {
        objectToActivate.SetActive(true);
    }
}
