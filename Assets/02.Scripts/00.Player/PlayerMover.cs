using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMover : MonoBehaviour
{
    private CharacterController controller;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float gravity = -9.81f;
    private Vector2 velocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public void Move(Vector2 intput)
    {
        Vector3 move = new Vector3(intput.x, 0, intput.y);
        Debug.Log($"Move: {move}"); // 이동 벡터 출력

        // 단순 이동
        controller.Move(move *  moveSpeed * Time.deltaTime);

        // 중력 적용
        if (controller.isGrounded && velocity.y < 0)
            velocity.y = -2f; // 땅에 닿았을 때 약간의 힘을 주어 완전히 멈추도록 함
        velocity.y += gravity * Time.deltaTime; // 중력 가속도 적용
        controller.Move(velocity * Time.deltaTime); // 중력에 따른 이동 적용
    }
}
