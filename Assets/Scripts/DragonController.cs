
using UnityEngine;

public class DragonController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private DragonMovement movement;
    [SerializeField] private DragonRotation rotation;

    [Header("Dragon Stats")]
    [SerializeField] private DragonStats stats = new DragonStats();

    private Camera mainCamera;
    private Plane movementPlane;

    private void Awake()
    {
        mainCamera = Camera.main;

        movementPlane = new Plane(
            Vector3.up,
            new Vector3(0f, transform.position.y, 0f)
        );

        movement.SetMoveSpeed(stats.moveSpeed);

        rotation.SetRotationSettings(
            stats.rotationSpeed,
            stats.bankingAngle,
            stats.pitchAngle
        );
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (movementPlane.Raycast(ray, out float distance))
            {
                Vector3 targetPosition = ray.GetPoint(distance);

                movement.SetTargetPosition(targetPosition);

                Vector3 direction = targetPosition - transform.position;

                rotation.SetMovementInput(
                    new Vector2(direction.x, direction.z).normalized
                );
            }
        }
        else
        {
            movement.StopFollowing();
            rotation.SetMovementInput(Vector2.zero);
        }
    }
}
