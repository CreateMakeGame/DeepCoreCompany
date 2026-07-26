using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObj = new object();
    private static bool isApplicationQuitting = false;
    public static T Instance
    {
        get
        {
            // 앱 종료 중일 때는 새로운 인스턴스를 생성하지 않도록 합니다.
            if (isApplicationQuitting)
            {
                return null;
            }

            lock (lockObj)
            {
                if (instance == null)
                {
                    // 1. 먼저 씬에서 찾기
                    instance = (T)FindFirstObjectByType(typeof(T));

                    // 2. 씬에 없을 때만 Resources에서 불러오거나 빈 오브젝트 생성
                    if (instance == null)
                    {
                        T prefab = Resources.Load<T>(typeof(T).Name);
                        if (prefab != null)
                        {
                            instance = Instantiate(prefab);
                            instance.name = typeof(T).Name;
                        }
                        else
                        {
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = typeof(T).ToString() + " (Singleton)";
                        }
                    }
                }
                return instance;
            }
        }
    }

    protected virtual void Awake()
    {
        if (instance == null)
        {
            instance = this as T;
            if (transform.parent == null)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        isApplicationQuitting = true;
    }

    protected virtual void OnDestroy()
    {
        if (instance == this)
        {
            isApplicationQuitting = true;
        }
    }
}
