
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
        Dive,
        ZigZag,
        HorizontalSweep,
        FigureEight,
        ArcMovement,
        BobbingHover,
        DiagonalSweep,
        Patrol,
        Swoop
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
    [SerializeField] private Vector2 destinationXRange = new Vector2(-4f, 4f);
    [SerializeField] private Vector2 destinationZRange = new Vector2(0f, 12f);
    [SerializeField] private float destinationMoveSpeed = 5f;
    [SerializeField] private float destinationReachDistance = 0.15f;

    private Vector3 assignedDestination;
    private bool hasAssignedDestination;

    [Header("Sine Wave Settings")]
    [SerializeField] private float waveAmplitude = 2f;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float verticalWaveAmplitude = 3f;
    [SerializeField] private float verticalWaveFrequency = 0.8f;

    private float randomizedWaveAmplitude;
    private float randomizedWaveFrequency;
    private float randomizedVerticalAmplitude;
    private float randomizedVerticalFrequency;

    private float movementPhase;
    private float waveTimer;

    [Header("Stationary Hover Settings")]
    [SerializeField] private float hoverAmplitude = 0.5f;
    [SerializeField] private float hoverFrequency = 1.5f;

    private float randomizedHoverAmplitude;
    private float randomizedHoverFrequency;
    private Vector3 hoverCenter;
    private bool isHovering;

    [Header("Circular Movement Settings")]
    [SerializeField] private float circularRadius = 2f;
    [SerializeField] private float circularSpeed = 2f;

    private float randomizedCircularRadius;
    private float randomizedCircularSpeed;
    private float circularTimer;

    [Header("Dive Settings")]
    [SerializeField] private float diveSpeed = 10f;
    [SerializeField] private Vector3 predeterminedDiveDirection =
        new Vector3(0f, 0f, -1f);

    private Vector3 diveDirection;
    private bool diveStarted;

    [Header("ZigZag Settings")]
    [SerializeField] private float zigZagWidth = 3f;
    [SerializeField] private float zigZagFrequency = 2f;
    [SerializeField] private float zigZagForwardSpeed = 2f;

    [Header("Horizontal Sweep Settings")]
    [SerializeField] private float sweepWidth = 4f;
    [SerializeField] private float sweepFrequency = 1.5f;
    [SerializeField] private float sweepForwardSpeed = 1f;

    [Header("Figure Eight Settings")]
    [SerializeField] private float figureEightWidth = 2f;
    [SerializeField] private float figureEightDepth = 4f;
    [SerializeField] private float figureEightSpeed = 1.5f;

    [Header("Arc Movement Settings")]
    [SerializeField] private float arcWidth = 3f;
    [SerializeField] private float arcDepth = 4f;
    [SerializeField] private float arcSpeed = 1f;

    [Header("Bobbing Hover Settings")]
    [SerializeField] private float bobbingAmplitude = 2f;
    [SerializeField] private float bobbingFrequency = 1.5f;

    [Header("Diagonal Sweep Settings")]
    [SerializeField] private float diagonalWidth = 4f;
    [SerializeField] private float diagonalSpeed = 3f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolWidth = 3f;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Swoop Settings")]
    [SerializeField] private float swoopWidth = 3f;
    [SerializeField] private float swoopDepth = 5f;
    [SerializeField] private float swoopSpeed = 1.5f;

    [Header("Formation Settings")]
    [SerializeField] private Transform formationRoot;
    [SerializeField] private Vector3 formationOffset;
    [SerializeField] private float formationSpeed = 5f;

    private Rigidbody rb;
    private Camera mainCamera;

    private Vector3 startPosition;
    private Vector3 movementCenter;

    private float movementTimer;
    private float patrolDirection = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        defaultMovementType = movementType;
        baseMoveSpeed = moveSpeed;

        mainCamera = Camera.main;

        rb.useGravity = false;
        rb.isKinematic = true;

        startPosition = rb.position;

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

            case MovementType.ZigZag:
                MoveZigZag();
                break;

            case MovementType.HorizontalSweep:
                MoveHorizontalSweep();
                break;

            case MovementType.FigureEight:
                MoveFigureEight();
                break;

            case MovementType.ArcMovement:
                MoveArc();
                break;

            case MovementType.BobbingHover:
                MoveBobbingHover();
                break;

            case MovementType.DiagonalSweep:
                MoveDiagonalSweep();
                break;

            case MovementType.Patrol:
                MovePatrol();
                break;

            case MovementType.Swoop:
                MoveSwoop();
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

        randomizedVerticalAmplitude = Random.Range(
            verticalWaveAmplitude * 0.7f,
            verticalWaveAmplitude * 1.3f
        );

        randomizedVerticalFrequency = Random.Range(
            verticalWaveFrequency * 0.7f,
            verticalWaveFrequency * 1.3f
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
        assignedDestination = new Vector3(
            Random.Range(destinationXRange.x, destinationXRange.y),
            rb.position.y,
            Random.Range(destinationZRange.x, destinationZRange.y)
        );

        hasAssignedDestination = true;
    }

    private void MoveSmoothly(Vector3 target, float speed)
    {
        Vector3 direction = target - rb.position;

        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            target,
            speed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        RotateTowards(direction);
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
        movementTimer += Time.fixedDeltaTime;

        float x = Mathf.Sin(
            movementTimer * randomizedWaveFrequency + movementPhase
        ) * randomizedWaveAmplitude;

        float z = Mathf.Sin(
            movementTimer * randomizedVerticalFrequency + movementPhase
        ) * randomizedVerticalAmplitude;

        Vector3 target = startPosition + new Vector3(x, 0f, z);

        MoveSmoothly(target, moveSpeed);
    }

    private void MoveFormation()
    {
        if (formationRoot == null)
            return;

        Vector3 target = formationRoot.TransformPoint(formationOffset);

        MoveSmoothly(target, formationSpeed);
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
            isHovering = false;
        }

        if (!isHovering)
        {
            MoveTowardsAssignedDestination();

            if (Vector3.Distance(rb.position, assignedDestination)
                <= destinationReachDistance)
            {
                isHovering = true;
                hoverCenter = rb.position;
                movementTimer = 0f;
            }

            return;
        }

        movementTimer += Time.fixedDeltaTime;

        float x = Mathf.Sin(
            movementTimer * randomizedHoverFrequency + movementPhase
        ) * randomizedHoverAmplitude;

        float z = Mathf.Cos(
            movementTimer * randomizedHoverFrequency * 0.7f + movementPhase
        ) * randomizedHoverAmplitude * 0.25f;

        MoveSmoothly(
            hoverCenter + new Vector3(x, 0f, z),
            destinationMoveSpeed
        );
    }

    private void MoveCircular()
    {
        if (!hasEnteredScreen)
        {
            if (!MoveIntoScreen())
                return;

            hasEnteredScreen = true;
            AssignDestination();

            movementCenter = assignedDestination;
            movementTimer = 0f;
        }

        movementTimer += Time.fixedDeltaTime;

        float angle =
            movementTimer * randomizedCircularSpeed + movementPhase;

        Vector3 target = movementCenter + new Vector3(
            Mathf.Sin(angle) * randomizedCircularRadius,
            0f,
            Mathf.Cos(angle) * randomizedCircularRadius
        );

        MoveSmoothly(target, moveSpeed);
    }

    private void MoveDive()
    {
        if (!hasEnteredScreen)
        {
            if (!MoveIntoScreen())
                return;

            hasEnteredScreen = true;

            diveDirection = predeterminedDiveDirection;
            diveDirection.y = 0f;

            if (diveDirection.sqrMagnitude < 0.001f)
                diveDirection = Vector3.back;

            diveDirection.Normalize();
            diveStarted = true;
        }

        if (!diveStarted)
            return;

        Vector3 target = rb.position +
            diveDirection * diveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(target);
        RotateTowards(diveDirection);
    }

    // Smooth alternating left and right.
    private void MoveZigZag()
    {
        movementTimer += Time.fixedDeltaTime;

        float x = Mathf.PingPong(
            movementTimer * zigZagFrequency,
            zigZagWidth * 2f
        ) - zigZagWidth;

        Vector3 target = startPosition + new Vector3(
            x,
            0f,
            -movementTimer * zigZagForwardSpeed
        );

        MoveSmoothly(target, moveSpeed);
    }

    // Smooth horizontal sweeping with forward progression.
    private void MoveHorizontalSweep()
    {
        movementTimer += Time.fixedDeltaTime;

        float x = Mathf.Sin(
            movementTimer * sweepFrequency + movementPhase
        ) * sweepWidth;

        Vector3 target = startPosition + new Vector3(
            x,
            0f,
            -movementTimer * sweepForwardSpeed
        );

        MoveSmoothly(target, moveSpeed);
    }

    // Figure-eight flight path.
    private void MoveFigureEight()
    {
        movementTimer += Time.fixedDeltaTime;

        float angle = movementTimer * figureEightSpeed + movementPhase;

        float x = Mathf.Sin(angle) * figureEightWidth;

        float z = Mathf.Sin(angle * 2f) * figureEightDepth;

        Vector3 target = movementCenter + new Vector3(x, 0f, z);

        MoveSmoothly(target, moveSpeed);
    }

    // Smooth arc movement.
    private void MoveArc()
    {
        movementTimer += Time.fixedDeltaTime;

        float angle = Mathf.Lerp(
            -Mathf.PI * 0.5f,
            Mathf.PI * 0.5f,
            Mathf.PingPong(movementTimer * arcSpeed, 1f)
        );

        float x = Mathf.Sin(angle) * arcWidth;
        float z = Mathf.Cos(angle) * arcDepth;

        Vector3 target = movementCenter + new Vector3(x, 0f, z);

        MoveSmoothly(target, moveSpeed);
    }

    // Gentle up and down movement along the Z axis.
    private void MoveBobbingHover()
    {
        movementTimer += Time.fixedDeltaTime;

        float z = Mathf.Sin(
            movementTimer * bobbingFrequency + movementPhase
        ) * bobbingAmplitude;

        Vector3 target = movementCenter + new Vector3(0f, 0f, z);

        MoveSmoothly(target, destinationMoveSpeed);
    }

    // Repeated diagonal movement.
    private void MoveDiagonalSweep()
    {
        movementTimer += Time.fixedDeltaTime;

        float progress = Mathf.PingPong(
            movementTimer * diagonalSpeed,
            2f
        ) - 1f;

        float x = progress * diagonalWidth;

        Vector3 target = startPosition + new Vector3(
            x,
            0f,
            -movementTimer * moveSpeed * 0.25f
        );

        MoveSmoothly(target, moveSpeed);
    }

    // Moves between two horizontal positions.
    private void MovePatrol()
    {
        if (!hasEnteredScreen)
        {
            if (!MoveIntoScreen())
                return;

            hasEnteredScreen = true;
            movementCenter = rb.position;
        }

        float nextX = rb.position.x +
            patrolDirection * patrolSpeed * Time.fixedDeltaTime;

        float leftLimit = movementCenter.x - patrolWidth;
        float rightLimit = movementCenter.x + patrolWidth;

        if (nextX >= rightLimit)
        {
            nextX = rightLimit;
            patrolDirection = -1f;
        }
        else if (nextX <= leftLimit)
        {
            nextX = leftLimit;
            patrolDirection = 1f;
        }

        Vector3 target = new Vector3(
            nextX,
            rb.position.y,
            movementCenter.z
        );

        MoveSmoothly(target, patrolSpeed);
    }

    // Curved downward and upward swoop.
    private void MoveSwoop()
    {
        movementTimer += Time.fixedDeltaTime;

        float angle = movementTimer * swoopSpeed + movementPhase;

        float x = Mathf.Sin(angle) * swoopWidth;

        float z = -Mathf.Sin(angle * 0.5f) * swoopDepth;

        Vector3 target = movementCenter + new Vector3(x, 0f, z);

        MoveSmoothly(target, moveSpeed);
    }

    private bool MoveIntoScreen()
    {
        Vector3 direction = Vector3.back;

        rb.MovePosition(
            rb.position + direction * entryMoveSpeed * Time.fixedDeltaTime
        );

        RotateTowards(direction);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return false;

        Vector3 viewport = mainCamera.WorldToViewportPoint(rb.position);

        return viewport.z > 0f && viewport.y <= entryViewportY;
    }

    private void MoveTowardsAssignedDestination()
    {
        if (!hasAssignedDestination)
            return;

        MoveSmoothly(assignedDestination, destinationMoveSpeed);
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

        predeterminedDiveDirection = direction.normalized;
        diveDirection = predeterminedDiveDirection;
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
        movementTimer = 0f;
        waveTimer = 0f;
        circularTimer = 0f;

        startPosition = rb.position;
        movementCenter = rb.position;

        formationRoot = null;
        formationOffset = Vector3.zero;

        hasEnteredScreen = false;
        hasAssignedDestination = false;

        isHovering = false;
        diveStarted = false;

        diveDirection = predeterminedDiveDirection;

        patrolDirection = 1f;

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
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        if (hasAssignedDestination)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(assignedDestination, 0.5f);
            Gizmos.DrawLine(transform.position, assignedDestination);
        }
    }
}

    // private void MoveSineWave()
    // {
    //     waveTimer += Time.fixedDeltaTime;

    //     float xOffset = Mathf.Sin(
    //         waveTimer * randomizedWaveFrequency + movementPhase
    //     ) * randomizedWaveAmplitude;

    //     Vector3 target = startPosition +
    //         new Vector3(xOffset, 0f, -moveSpeed * waveTimer);

    //     Vector3 direction = target - rb.position;

    //     rb.MovePosition(target);
    //     RotateTowards(direction);
    // }
