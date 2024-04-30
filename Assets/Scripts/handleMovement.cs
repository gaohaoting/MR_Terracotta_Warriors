using UnityEngine;
public class SimpleVRObjectMover : MonoBehaviour
{
    [SerializeField] private Transform movementFrameOfReference;

    [SerializeField] private float maximumLinearSpeed = 0.5f;
    [SerializeField] private float verticalSpeed = 0.5f;

    private CharacterController _characterController;
    private Vector2 _motionInput;
    private float _verticalInput;

    void Awake()
    {
        _characterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        GetLocomotionInput();
        ApplyMotion();
    }

    private void GetLocomotionInput()
    {
        Vector2 thumbstickAxis = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        _motionInput = new Vector2(thumbstickAxis.x, thumbstickAxis.y);
        _verticalInput = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick).y;
    }

    private void ApplyMotion()
    {
        Vector3 motionForwardDirection = Vector3.ProjectOnPlane(movementFrameOfReference.forward, Vector3.up).normalized;
        Vector3 motionRightDirection = Vector3.ProjectOnPlane(movementFrameOfReference.right, Vector3.up).normalized;
        Vector3 horizontalDirection = (motionForwardDirection * _motionInput.y + motionRightDirection * _motionInput.x).normalized;

        Vector3 movement = horizontalDirection * maximumLinearSpeed * Time.deltaTime;
        movement += Vector3.up * _verticalInput * verticalSpeed * Time.deltaTime;

        _characterController.Move(movement);
    }
}
