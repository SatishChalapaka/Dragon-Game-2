
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    public enum MovementType
    {
        Straight,
        SineWave,
        Formation,
        Stationary,
        StationaryHover,
        Circular,
        Dive
    }

    [Header("Enemy Settings")]
    [SerializeField] private MovementType movementType;
    [SerializeField] private float moveSpeed = 5f;

    private float baseMoveSpeed;
    private MovementType defaultMovementType;

    [Header("Screen Entry Settings")]
    [SerializeField] private float entryMoveSpeed = 5f;
    [SerializeField, Range(0.5f, 1f)]
    private float entryViewportY = 0.85f;

    private bool hasEnteredScreen;

    [Header("Individual Destination Settings")]
    [SerializeField]
    private Vector2 destinationXRange =
        new Vector2(-4f, 4f);

    [SerializeField]
    private Vector2 destinationZRange =
        new Vector2(0f, 12f);

    [SerializeField] private float destinationMoveSpeed = 5f;

    [SerializeField] private float destinationReachDistance = 0.15f;

    private Vector3 assignedDestination;
    private bool hasAssignedDestination;

    [Header("Sine Wave Settings")]
    [SerializeField] private float waveAmplitude = 2f;
    [SerializeField] private float waveFrequency = 2f;

    private float randomizedWaveAmplitude;
    private float randomizedWaveFrequency;
    private float movementPhase;
    private float waveTimer;

    [Header("Stationary Hover Settings")]
    [SerializeField] private float hoverAmplitude = 0.5f;
    [SerializeField] private float hoverFrequency = 1.5f;

    private float randomizedHoverAmplitude;
    private float randomizedHoverFrequency;
    private Vector3 hoverCenter;

    [Header("Circular Movement Settings")]
    [SerializeField] private float circularRadius = 2f;
    [SerializeField] private float circularSpeed = 2f;

    private float randomizedCircularRadius;
    private float randomizedCircularSpeed;
    private float circularTimer;

    [Header("Dive Settings")]
    [SerializeField] private float diveSpeed = 10f;
    [SerializeField]
    private Vector3 predeterminedDiveDirection =
        new Vector3(0f, 0f, -1f);

    private Vector3 diveDirection;
    private Vector3 diveDestination;
    private bool diveStarted;

    [Header("Formation Settings")]
    [SerializeField] private Transform formationRoot;
    [SerializeField] private Vector3 formationOffset;
    [SerializeField] private float formationSpeed = 5f;

    private Rigidbody rb;
    private Camera mainCamera;
    private Vector3 startPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        defaultMovementType = movementType;
        baseMoveSpeed = moveSpeed;

        mainCamera = Camera.main;

        rb.useGravity = false;
        rb.isKinematic = true;

        startPosition = transform.position;

        RandomizeMovement();
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

            case MovementType.Stationary:
                MoveStationary();
                break;

            case MovementType.StationaryHover:
                MoveStationaryHover();
                break;

            case MovementType.Circular:
                MoveCircular();
                break;

            case MovementType.Dive:
                MoveDive();
                break;
        }
    }

    private void RandomizeMovement()
    {
        movementPhase = Random.Range(0f, 6f);

        randomizedWaveAmplitude = Random.Range(
            waveAmplitude * 0.7f,
            waveAmplitude * 1.3f
        );

        randomizedWaveFrequency = Random.Range(
            waveFrequency * 0.7f,
            waveFrequency * 1.3f
        );

        randomizedHoverAmplitude = Random.Range(
            hoverAmplitude * 0.7f,
            hoverAmplitude * 1.3f
        );

        randomizedHoverFrequency = Random.Range(
            hoverFrequency * 0.7f,
            hoverFrequency * 1.3f
        );

        randomizedCircularRadius = Random.Range(
            circularRadius * 0.7f,
            circularRadius * 1.3f
        );

        randomizedCircularSpeed = Random.Range(
            circularSpeed * 0.7f,
            circularSpeed * 1.3f
        );
    }

    private void AssignDestination()
    {
        float randomX = Random.Range(
            destinationXRange.x,
            destinationXRange.y
        );

        float randomZ = Random.Range(
            destinationZRange.x,
            destinationZRange.y
        );

        assignedDestination = new Vector3(
            randomX,
            rb.position.y,
            randomZ
        );

        hasAssignedDestination = true;
    }

    private void MoveStraight()
    {
        Vector3 direction = Vector3.back;

        Vector3 target = rb.position +
            direction * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(target);

        RotateTowards(direction);
    }

    private void MoveSineWave()
    {
        waveTimer += Time.fixedDeltaTime;

        float xOffset = Mathf.Sin(
            (waveTimer * randomizedWaveFrequency) + movementPhase
        ) * randomizedWaveAmplitude;

        Vector3 target = startPosition +
            new Vector3(
                xOffset,
                0f,
                -moveSpeed * waveTimer
            );

        Vector3 direction = target - rb.position;

        rb.MovePosition(target);

        RotateTowards(direction);
    }

    private void MoveFormation()
    {
        if (formationRoot == null)
            return;

        Vector3 target = formationRoot.TransformPoint(formationOffset);

        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            target,
            formationSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        RotateTowards(Vector3.back);
    }

    private void MoveStationary()
    {
        if (!hasEnteredScreen)
        {
            if (!MoveIntoScreen())
                return;

            hasEnteredScreen = true;

            AssignDestination();
        }

        MoveTowardsAssignedDestination();
    }

    private void MoveStationaryHover()
    {
        if (!hasEnteredScreen)
        {
            if (!MoveIntoScreen())
                return;

            hasEnteredScreen = true;

            AssignDestination();

            hoverCenter = assignedDestination;
        }

        MoveTowardsAssignedDestination();

        if (Vector3.Distance(rb.position, assignedDestination) >
            destinationReachDistance)
        {
            return;
        }

        waveTimer += Time.fixedDeltaTime;

        float xOffset = Mathf.Sin(
            waveTimer * randomizedHoverFrequency + movementPhase
        ) * randomizedHoverAmplitude;

        float zOffset = Mathf.Cos(
            waveTimer * randomizedHoverFrequency * 0.7f + movementPhase
        ) * randomizedHoverAmplitude * 0.25f;

        Vector3 target = hoverCenter +
            new Vector3(xOffset, 0f, zOffset);

        rb.MovePosition(target);
    }

    private void MoveCircular()
    {
        if (!hasEnteredScreen)
        {
            if (!MoveIntoScreen())
                return;

            hasEnteredScreen = true;

            AssignDestination();

            startPosition = assignedDestination;
        }

        circularTimer += Time.fixedDeltaTime;

        float angle =
            circularTimer * randomizedCircularSpeed + movementPhase;

        float xOffset =
            Mathf.Sin(angle) * randomizedCircularRadius;

        float zOffset =
            Mathf.Cos(angle) * randomizedCircularRadius;

        Vector3 target = startPosition +
            new Vector3(xOffset, 0f, zOffset);

        rb.MovePosition(target);
    }

    private void MoveDive()
    {
        if (!hasEnteredScreen)
        {
            if (!MoveIntoScreen())
                return;

            hasEnteredScreen = true;

            if (!hasAssignedDestination)
                AssignDestination();

            diveStarted = true;

            diveDirection = predeterminedDiveDirection;
            diveDirection.y = 0f;

            if (diveDirection.sqrMagnitude < 0.001f)
                diveDirection = Vector3.back;

            diveDirection.Normalize();

            diveDestination = assignedDestination;
        }

        if (!diveStarted)
            return;

        Vector3 target = rb.position +
            diveDirection * diveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(target);

        RotateTowards(diveDirection);
    }

    private bool MoveIntoScreen()
    {
        Vector3 direction = Vector3.back;

        Vector3 target = rb.position +
            direction * entryMoveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(target);

        RotateTowards(direction);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return false;

        Vector3 viewportPosition =
            mainCamera.WorldToViewportPoint(rb.position);

        return viewportPosition.z > 0f &&
               viewportPosition.y <= entryViewportY;
    }

    private void MoveTowardsAssignedDestination()
    {
        if (!hasAssignedDestination)
            return;

        Vector3 target = Vector3.MoveTowards(
            rb.position,
            assignedDestination,
            destinationMoveSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(target);

        RotateTowards(assignedDestination - rb.position);
    }

    public void SetFormation(Transform root, Vector3 offset)
    {
        movementType = MovementType.Formation;

        formationRoot = root;
        formationOffset = offset;
    }

    public void SetDiveDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            direction = Vector3.back;

        diveDirection = direction.normalized;
        predeterminedDiveDirection = diveDirection;
    }

    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(direction);

        rb.MoveRotation(
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                8f * Time.fixedDeltaTime
            )
        );
    }

    public void ResetForSpawn()
    {
        waveTimer = 0f;
        circularTimer = 0f;

        startPosition = rb.position;

        formationRoot = null;
        formationOffset = Vector3.zero;

        hasEnteredScreen = false;
        hasAssignedDestination = false;

        diveStarted = false;

        diveDirection = predeterminedDiveDirection;

        movementType = defaultMovementType;
        moveSpeed = baseMoveSpeed;

        RandomizeMovement();
    }

    public void ApplyWaveDifficulty(float speedMultiplier)
    {
        moveSpeed = baseMoveSpeed *
            Mathf.Max(0.1f, speedMultiplier);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            0.5f
        );

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            assignedDestination,
            0.5f
        );

        Gizmos.DrawLine(
            transform.position,
            assignedDestination
        );
    }
}
