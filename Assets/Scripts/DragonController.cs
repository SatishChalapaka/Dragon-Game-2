
using UnityEngine;

public class DragonController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Joystick joystick;
    [SerializeField] private DragonMovement movement;
    [SerializeField] private DragonRotation rotation;

    [Header("Dragon Stats")]
    [SerializeField] private DragonStats stats = new DragonStats();

    private void Awake()
    {
        movement.SetMoveSpeed(stats.moveSpeed);

        rotation.SetRotationSettings(
            stats.rotationSpeed,
            stats.bankingAngle,
            stats.pitchAngle
        );
    }

    private void Update()
    {
        Vector2 input = joystick != null
            ? joystick.Direction
            : Vector2.zero;

        input = Vector2.ClampMagnitude(input, 1f);

        movement.SetMovementInput(input);
        rotation.SetMovementInput(input);
    }
}
