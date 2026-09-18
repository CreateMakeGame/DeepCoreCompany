using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int currentQuota = 200;       // 현재 목표 할당량
    public int currentMoney = 0;     // 현재 모은 돈

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        SetCursorState(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IncreaseQuota()
    {
        currentQuota += 150;
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
