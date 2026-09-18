using UnityEngine;

public class GlobalManagers : Singleton<GlobalManagers>
{
    [field: Header("Child Managers")]
    [field: SerializeField] public GameManager Game { get; private set; }
    [field: SerializeField] public CameraManager Camera { get; private set; }
    [field: SerializeField] public UIManager UI { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InitManagers();
    }

    private void InitManagers()
    {
        // 인스펙터 연결이 비어있다면 자식 오브젝트에서 자동으로 찾아 매핑
        if (Game == null) Game = GetComponentInChildren<GameManager>();
        if (Camera == null) Camera = GetComponentInChildren<CameraManager>();
        if (UI == null) UI = GetComponentInChildren<UIManager>();
    }
}