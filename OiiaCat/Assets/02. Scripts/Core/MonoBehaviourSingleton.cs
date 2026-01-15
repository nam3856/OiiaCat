using UnityEngine;

/// <summary>
/// MonoBehaviour 기반 싱글톤 베이스 클래스
/// 저장소 스타일 가이드 1-e-(1) 규칙에 따른 구현
/// </summary>
/// <typeparam name="T">MonoBehaviour를 상속받은 클래스</typeparam>
public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
{
    private static T _instance;
    
    /// 싱글톤 인스턴스
    /// </summary>
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<T>();

                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(T).Name);
                    _instance = singletonObject.AddComponent<T>();
                    DontDestroyOnLoad(singletonObject);
                }
            }

            return _instance;
        }
    }

    /// <summary>
    /// 싱글톤 인스턴스가 이미 존재하는지 확인
    /// </summary>
    public static bool HasInstance => _instance != null;

    /// <summary>
    /// 싱글톤 초기화
    /// 중복 인스턴스가 있으면 제거
    /// </summary>
    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[MonoBehaviourSingleton] Duplicate instance of {typeof(T).Name} detected. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
    }

    /// <summary>
    /// 인스턴스 파기
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}
