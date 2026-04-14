using UnityEngine;
[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Move Speeds")]
    public float baseSpeed = 5f;        // 기본 걷기 속도
    public float runSpeed = 8f;         // 달리기 속도

    [Header("Jump Physics")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.2f;
    public Vector3 groundCheckOffset = new Vector3(0, -0.1f, 0);
    public LayerMask groundLayer;
}
