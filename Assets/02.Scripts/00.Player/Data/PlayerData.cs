using UnityEngine;
[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Player Settings")]
    public float maxHp = 100f;
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;    // 초당 스테미너 소모량
    public float staminaRegenRate = 10f;    // 초당 스테미너 회복량
    public float regenDelay = 2f;           // 스테미너 회복 시작 전 대기 시간

    [Header("Move Speeds")]
    public float baseSpeed = 5f;        // 기본 걷기 속도
    public float runSpeed = 8f;         // 달리기 속도

    [Header("Jump Physics")]
    public float jumpHeight = 1.5f;
    public float gravity = -9.81f;

    [Header("Ground Check")]
    public float groundCheckRadius = 0.5f;
    public Vector3 groundCheckOffset = new Vector3(0, 0.3f, 0);
    public LayerMask groundLayer;

    [Header("Digging")]
    public float baseDigDuration = 1.0f;    // 굴착 애니메이션의 기본 지속 시간
    public float digSpeedMultiplier = 1.0f; // 굴착 애니메이션 속도 조절을 위한 배수
    public float digCooldown = 1.0f;        // 굴착 사이의 대기 시간
    public float digRange = 4.0f;           // 굴착이 가능한 최대 거리
}
