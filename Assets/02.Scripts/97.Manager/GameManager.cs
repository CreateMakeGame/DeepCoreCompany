using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        SetCursorState(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    private void SetCursorState(bool isLocked)
    {
        if (isLocked)
        {
            Cursor.lockState = CursorLockMode.Locked; // 커서를 화면 중앙에 고정
            Cursor.visible = false; // 커서 숨김
        }
        else
        {
            // 커서 고정을 해제하고 다시 보이게 합니다.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
