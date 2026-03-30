using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMover : MonoBehaviour
{
    private CharacterController controller;
    [Tooltip("점프 높이")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("중력")]
    [SerializeField] private float gravity = -9.81f;
    [Tooltip("점프 높이")]
    [SerializeField] private float JumpHeight = 1.5f;
    private Vector2 velocity;

    public bool IsGrounded() => controller.isGrounded;    // 현재 캐릭터가 땅에 닿아 있는지 여부를 알려줍니다.
    public float GetVerticalVelocity() => velocity.y;   // 현재 수직 속도(y)를 알려줍니다. (내려가는 중인지 확인용)

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Move(Vector2 intput)
    {
        Vector3 move = new Vector3(intput.x, 0, intput.y);

        // 단순 이동
        controller.Move(move *  moveSpeed * Time.deltaTime);

        // 중력 적용
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f; // 땅에 닿았을 때 약간의 힘을 주어 완전히 멈추도록 함
        Gravity(); // 중력 가속도 적용
        controller.Move(velocity * Time.deltaTime); // 중력에 따른 이동 적용
    }

    public void Gravity()
    {
        velocity.y += gravity * Time.deltaTime; // 중력 가속도 적용
    }

    public void Jump()
    {
        // 물리 공식 기반 점프 계산: v = sqrt(h * -2 * g)
        velocity.y = Mathf.Sqrt(JumpHeight * -2f * gravity);    // 점프 공식: v = sqrt(h * -2 * g)
    }
}
