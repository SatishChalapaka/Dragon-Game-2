
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    public enum MovementType
    {
        Straight,
        SineWave,
        Formation
    }

    [Header("Enemy Settings")]
    [SerializeField] private MovementType movementType;
    [SerializeField] private float moveSpeed = 5f;

    [Header("Sine Wave Settings")]
    [SerializeField] private float waveAmplitude = 2f;
    [SerializeField] private float waveFrequency = 2f;

    [Header("Formation Settings")]
    [SerializeField] private Transform formationRoot;
    [SerializeField] private Vector3 formationOffset;
    [SerializeField] private float formationSpeed = 5f;

    private Rigidbody rb;

    private Vector3 startPosition;
    private float waveTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;

        startPosition = transform.position;
    }

    private void FixedUpdate()
    {
        switch (movementType)
        {
            case MovementType.Straight:
                MoveStraight();
                break;

            case MovementType.SineWave:
                MoveSineWave();
                break;

            case MovementType.Formation:
                MoveFormation();
                break;
        }
    }

    private void MoveStraight()
{
    Vector3 direction = Vector3.back;

    rb.MovePosition(
        rb.position + direction * moveSpeed * Time.fixedDeltaTime
    );

    RotateTowards(direction);
}

    private void MoveSineWave()
{
    waveTimer += Time.fixedDeltaTime;

    float xOffset =
        Mathf.Sin(waveTimer * waveFrequency) * waveAmplitude;

    Vector3 newPosition = startPosition +
        new Vector3(xOffset, 0f, -moveSpeed * waveTimer);

    Vector3 direction = newPosition - rb.position;

    rb.MovePosition(newPosition);

    //RotateTowards(direction);
}
    private void MoveFormation()
    {
        if (formationRoot == null)
            return;

        Vector3 targetPosition =
            formationRoot.TransformPoint(formationOffset);

        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            targetPosition,
            formationSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
        Vector3 direction = targetPosition - rb.position;

RotateTowards(direction);
    }

    public void SetFormation(
        Transform root,
        Vector3 offset)
    {
        formationRoot = root;
        formationOffset = offset;
    }

private void RotateTowards(Vector3 direction)
{
    direction.y = 0f;

    if (direction.sqrMagnitude < 0.001f)
        return;

    Quaternion targetRotation = Quaternion.LookRotation(direction);

    rb.MoveRotation(
        Quaternion.Slerp(
            rb.rotation,
            targetRotation,
            8f * Time.fixedDeltaTime
        )
    );
}

}
