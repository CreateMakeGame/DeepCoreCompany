using UnityEngine;

public class Managers : Singleton<Managers>
{
    public GameManager Game { get; private set; }
    public CameraManager Camera { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        InitManagers();
    }

    private void InitManagers()
    {
        // 1. 자식 오브젝트들 중에서 매니저를 찾습니다.
        Game = GetComponentInChildren<GameManager>();
        Camera = GetComponentInChildren<CameraManager>();

        // 2. 만약 씬에 없다면, 자식 오브젝트로 새로 생성해서 붙여줍니다.
        if (Game == null) Game = AddChildManager<GameManager>();
        if (Camera == null) Camera = AddChildManager<CameraManager>();
    }

    private T AddChildManager<T>() where T : MonoBehaviour
    {
        GameObject child = new GameObject(typeof(T).Name);
        child.transform.parent = transform;
        return child.AddComponent<T>();
    }
}
