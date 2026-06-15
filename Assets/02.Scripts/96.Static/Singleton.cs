using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T instance;
    private static readonly object lockObj = new object();
    public static T Instance
    {
        get
        {
            lock (lockObj)
            {
                if (instance == null)
                {
                    // 씬 내에 이미 존재하는 싱글톤 인스턴스를 찾습니다.
                    instance = (T)FindFirstObjectByType(typeof(T));

                    // 없을 경우 새로 생성합니다.
                    if (instance == null)
                    {
                        GameObject singletonObject = new GameObject();
                        instance = singletonObject.AddComponent<T>();
                        singletonObject.name = typeof(T).ToString() + " (Singleton)";
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
}
