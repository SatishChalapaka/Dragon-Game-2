
using UnityEngine;

[System.Serializable]
public class DragonStats
{
    [Header("Movement")]
    [Min(0f)]
    public float moveSpeed = 10f;

    [Header("Rotation")]
    [Min(0f)]
    public float rotationSpeed = 8f;

    [Range(0f, 60f)]
    public float bankingAngle = 25f;

    [Range(0f, 45f)]
    public float pitchAngle = 12f;

    [Header("Health")]
    [Min(1f)]
    public float maxHealth = 100f;
}
