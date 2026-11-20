
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BossMovement : MonoBehaviour
{
    [Header("Camera Positioning")]
    [SerializeField, Range(0.5f, 0.98f)]
    private float bossViewportHeight = 0.85f;

    [SerializeField, Range(0.05f, 0.4f)]
    private float horizontalScreenPadding = 0.12f;

    [Header("Battle Movement")]
    [SerializeField] private float horizontalRange = 5f;
    [SerializeField] private float horizontalSpeed = 1.5f;

    [Header("Movement Settings")]
    [SerializeField] private float speedMultiplier = 1f;

    private Rigidbody rb;
    private Camera mainCamera;

    private Vector3 battlePosition;

    private float movementTimer;
    private float currentSpeedMultiplier = 1f;
    private float actualHorizontalRange;

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
        // Kept for compatibility with the existing EnemySpawner.
        // Boss positioning no longer depends on dragon position.

        mainCamera = Camera.main;

        currentSpeedMultiplier =
            Mathf.Max(0.1f, waveSpeedMultiplier * speedMultiplier);

        movementTimer = 0f;
        isInitialized = false;

        if (mainCamera == null)
        {
            Debug.LogError(
                "BossMovement: Main Camera not found!",
                this
            );
            return;
        }

        if (!TryGetBattlePosition(out Vector3 centerPosition))
        {
            Debug.LogError(
                "BossMovement: Could not calculate the top-screen position.",
                this
            );
            return;
        }

        battlePosition = centerPosition;

        actualHorizontalRange = CalculateHorizontalRange();

        // Place the boss directly at the top battle position.
        rb.position = battlePosition;

        isInitialized = true;

        Debug.Log(
            name + " positioned at the top boss battle area."
        );
    }

    private bool TryGetBattlePosition(out Vector3 position)
    {
        position = transform.position;

        Plane gameplayPlane = new Plane(
            Vector3.up,
            new Vector3(0f, transform.position.y, 0f)
        );

        Ray ray = mainCamera.ViewportPointToRay(
            new Vector3(0.5f, bossViewportHeight, 0f)
        );

        if (!gameplayPlane.Raycast(ray, out float distance))
            return false;

        position = ray.GetPoint(distance);

        return true;
    }

    private float CalculateHorizontalRange()
    {
        Plane gameplayPlane = new Plane(
            Vector3.up,
            new Vector3(0f, transform.position.y, 0f)
        );

        float leftX = GetViewportWorldX(
            horizontalScreenPadding,
            gameplayPlane
        );

        float rightX = GetViewportWorldX(
            1f - horizontalScreenPadding,
            gameplayPlane
        );

        if (float.IsNaN(leftX) || float.IsNaN(rightX))
            return horizontalRange;

        float availableHalfWidth =
            Mathf.Max(0f, (rightX - leftX) * 0.5f);

        return Mathf.Min(horizontalRange, availableHalfWidth);
    }

    private float GetViewportWorldX(
        float viewportX,
        Plane gameplayPlane)
    {
        Ray ray = mainCamera.ViewportPointToRay(
            new Vector3(viewportX, bossViewportHeight, 0f)
        );

        if (!gameplayPlane.Raycast(ray, out float distance))
            return float.NaN;

        return ray.GetPoint(distance).x;
    }

    private void FixedUpdate()
    {
        if (!isInitialized)
            return;

        MoveDuringBattle();
    }

    private void MoveDuringBattle()
    {
        movementTimer += Time.fixedDeltaTime;

        float horizontalOffset =
            Mathf.Sin(
                movementTimer *
                horizontalSpeed *
                currentSpeedMultiplier
            ) * actualHorizontalRange;

        Vector3 targetPosition = new Vector3(
            battlePosition.x + horizontalOffset,
            battlePosition.y,
            battlePosition.z
        );

        rb.MovePosition(targetPosition);
    }

    public void ResetMovement()
    {
        isInitialized = false;
        movementTimer = 0f;
        actualHorizontalRange = 0f;
        currentSpeedMultiplier = 1f;
    }

    private void OnDisable()
    {
        ResetMovement();
        mainCamera = null;
    }
}
