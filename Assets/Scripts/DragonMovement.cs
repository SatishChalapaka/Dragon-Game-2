
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
    private Vector3 targetPosition;
    private Vector3 startingPosition;
    private bool isFollowing;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        startingPosition = rb.position;

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotation;
    }

    public void SetTargetPosition(Vector3 position)
    {
        targetPosition = position;
        isFollowing = true;
    }

    public void StopFollowing()
    {
        isFollowing = false;
    }

    public void SetMoveSpeed(float speed)
    {
        moveSpeed = Mathf.Max(0f, speed);
    }

    public void ResetMovement()
    {
        isFollowing = false;
        targetPosition = startingPosition;
        rb.position = startingPosition;
        rb.velocity = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (!isFollowing)
            return;

        targetPosition.x = Mathf.Clamp(
            targetPosition.x, minBounds.x, maxBounds.x
        );

        targetPosition.z = Mathf.Clamp(
            targetPosition.z, minBounds.y, maxBounds.y
        );

        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            targetPosition,
            moveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }
}
