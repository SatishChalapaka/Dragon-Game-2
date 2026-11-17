
using UnityEngine;

public class DragonRotation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform dragonVisual;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float bankingAngle = 25f;
    [SerializeField] private float pitchAngle = 12f;

    private Vector2 movementInput;
    private Quaternion initialRotation;

    private void Awake()
    {
        initialRotation = dragonVisual.localRotation;
    }

    public void SetMovementInput(Vector2 input)
    {
        movementInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void SetRotationSettings(
        float speed,
        float bankAngle,
        float tiltAngle)
    {
        rotationSpeed = speed;
        bankingAngle = bankAngle;
        pitchAngle = tiltAngle;
    }

    private void Update()
    {
        float pitch = -movementInput.y * pitchAngle;
        float roll = -movementInput.x * bankingAngle;

        Quaternion targetRotation =
            initialRotation * Quaternion.Euler(pitch, 0f, roll);

        dragonVisual.localRotation = Quaternion.Slerp(
            dragonVisual.localRotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}
