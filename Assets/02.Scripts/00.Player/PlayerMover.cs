using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CharacterController))]
public class PlayerMover : MonoBehaviour
{
    private CharacterController controller;
    private PlayerStateMachine stateMachine;

    private float moveSpeed;
    private Vector2 velocity;
    private Vector3 move;   // 이동 방향 벡터


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        stateMachine = GetComponent<PlayerStateMachine>();
    }
    private void Update()
    {
        // 단순 이동
        controller.Move(move * moveSpeed * Time.deltaTime);

        // 중력 적용
        if (IsGrounded() && velocity.y < 0)
            velocity.y = -2f; // 땅에 닿았을 때 약간의 힘을 주어 완전히 멈추도록 함

        Gravity(); // 중력 가속도 적용

        controller.Move(velocity * Time.deltaTime); // 중력에 따른 이동 적용

        move = Vector3.zero; // 매 프레임 이동 방향 초기화 (입력에 따라 새로 설정될 예정)
    }

    public void Move(Vector2 intput)
    {
        //move = new Vector3(intput.x, 0, intput.y);
        move = (transform.forward * intput.y) + (transform.right * intput.x); // 카메라 방향 기준 이동 벡터 계산
    }
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void Gravity()
    {
        velocity.y += stateMachine.Data.gravity * Time.deltaTime; // 중력 가속도 적용
    }

    public void Jump()
    {
        // 물리 공식 기반 점프 계산: v = sqrt(h * -2 * g)
        velocity.y = Mathf.Sqrt(stateMachine.Data.jumpHeight * -2f * stateMachine.Data.gravity);    // 점프 공식: v = sqrt(h * -2 * g)
    }

    public bool IsGrounded()
    {
        return Physics.CheckSphere(transform.position + stateMachine.Data.groundCheckOffset, 
            stateMachine.Data.groundCheckRadius, 
            stateMachine.Data.groundLayer);
    }
    public float GetVerticalVelocity() => velocity.y;   // 현재 수직 속도(y)를 알려줍니다. (내려가는 중인지 확인용)
}
