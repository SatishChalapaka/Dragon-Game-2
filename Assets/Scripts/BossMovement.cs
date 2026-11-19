
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossMovement : MonoBehaviour
{
    [Header("Entrance Settings")]
    [SerializeField] private float entranceSpeed = 5f;
    [SerializeField] private float distanceFromDragon = 12f;

    [Header("Battle Movement")]
    [SerializeField] private float horizontalRange = 5f;
    [SerializeField] private float horizontalSpeed = 1.5f;

    private Rigidbody rb;
    private Transform dragon;

    private Vector3 battlePosition;
    private float movementTimer;
    private float speedMultiplier = 1f;

    private bool hasEntered;
    private bool isInitialized;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;
    }

    public void Initialize(
        Transform dragonTarget,
        float waveSpeedMultiplier = 1f)
    {
        dragon = dragonTarget;

        speedMultiplier =
            Mathf.Max(0.1f, waveSpeedMultiplier);

        movementTimer = 0f;
        hasEntered = false;
        isInitialized = dragon != null;

        if (!isInitialized)
            return;

        battlePosition = new Vector3(
            dragon.position.x,
            transform.position.y,
            dragon.position.z + distanceFromDragon
        );
    }

    private void FixedUpdate()
    {
        if (!isInitialized || dragon == null)
            return;

        if (!hasEntered)
        {
            MoveToBattlePosition();
        }
        else
        {
            MoveDuringBattle();
        }
    }

    private void MoveToBattlePosition()
    {
        Vector3 newPosition = Vector3.MoveTowards(
            rb.position,
            battlePosition,
            entranceSpeed * speedMultiplier *
            Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);

        if (Vector3.Distance(
            newPosition,
            battlePosition) <= 0.05f)
        {
            hasEntered = true;
            movementTimer = 0f;
        }
    }

    private void MoveDuringBattle()
    {
        movementTimer += Time.fixedDeltaTime;

        float horizontalOffset =
            Mathf.Sin(
                movementTimer *
                horizontalSpeed *
                speedMultiplier
            ) * horizontalRange;

        Vector3 targetPosition = new Vector3(
            battlePosition.x + horizontalOffset,
            battlePosition.y,
            battlePosition.z
        );

        rb.MovePosition(targetPosition);
    }

    private void OnDisable()
    {
        dragon = null;
        movementTimer = 0f;
        hasEntered = false;
        isInitialized = false;
        speedMultiplier = 1f;
    }
}
