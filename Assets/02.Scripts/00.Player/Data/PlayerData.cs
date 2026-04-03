using UnityEngine;
[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Move Speeds")]
    public float baseSpeed = 5f;
    public float runSpeed = 8f;

    [Header("Jump Physics")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.4f;
    public Vector3 groundCheckOffset = new Vector3(0, -0.5f, 0);
    public LayerMask groundLayer;
}
