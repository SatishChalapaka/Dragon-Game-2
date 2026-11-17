
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DragonMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Movement Boundaries")]
    [SerializeField] private Vector2 minBounds = new Vector2(-5f, -12f);
    [SerializeField] private Vector2 maxBounds = new Vector2(5f, 12f);

    private Rigidbody rb;
    private Vector2 movementInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotation;
    }

    public void SetMovementInput(Vector2 input)
    {
        movementInput = Vector2.ClampMagnitude(input, 1f);
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    private void FixedUpdate()
    {
        Vector3 direction = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        );

        Vector3 targetPosition =
            rb.position + direction * moveSpeed * Time.fixedDeltaTime;

        targetPosition.x = Mathf.Clamp(
            targetPosition.x,
            minBounds.x,
            maxBounds.x
        );

        targetPosition.z = Mathf.Clamp(
            targetPosition.z,
            minBounds.y,
            maxBounds.y
        );

        rb.MovePosition(targetPosition);
    }
}
