using System.Collections;
using UnityEngine;

public class GifBurstPlayer : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _stateName = "Gif";
    [SerializeField] private string _defaultStateName = "Idle";
    [SerializeField] private float _playDuration = 0.5f;
    [SerializeField] private bool _useUnscaledTime = false;

    // 전역 입력 감지기 (자동으로 찾음)
    private GlobalInputActivityDetector_Windows _inputCounter;

    [Header("Behavior")]
    [SerializeField] private bool _restartFromFirstFrameOnTrigger = false;

    private bool _isPlaying;
    private float _endTime;
    private Coroutine _watchCo;

    private float Now => _useUnscaledTime ? Time.unscaledTime : Time.time;

    private void Reset()
    {
        _animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (_animator == null) _animator = GetComponent<Animator>();

        // 시작은 첫 프레임 고정
        if (_animator != null)
        {
            _animator.speed = 0f;
        }
        _isPlaying = false;
    }

    private void Start()
    {
        // 전역 입력 감지기 자동으로 찾기
        _inputCounter = GameObject.FindAnyObjectByType<GlobalInputActivityDetector_Windows>();
        if (_inputCounter != null)
        {
            _inputCounter.OnActivity += Trigger;
        }
        else
        {
            Debug.LogWarning("GlobalInputActivityDetector_Windows not found in scene!");
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_inputCounter != null)
        {
            _inputCounter.OnActivity -= Trigger;
        }
    }

    public void Trigger(uint _)
    {
        // Destroy된 객체 체크
        if (this == null) return;
        if (_animator == null) return;

        // "마지막 입력 기준 0.5초"로 종료 시각 갱신
        _endTime = Now + _playDuration;

        if (!_isPlaying)
        {
            StartPlaying();
            _watchCo = StartCoroutine(CoWatchEnd());
            return;
        }

        // 재생 중인데 또 트리거 됨
        if (_restartFromFirstFrameOnTrigger)
        {
            _animator.Play(_stateName, 0, 0f);
        }
        // 아니면 그냥 계속 재생하면서 duration만 연장
    }

    private void StartPlaying()
    {
        if (_animator == null) return;

        _isPlaying = true;
        _animator.speed = 1f;

        // 처음 시작 시에는 0프레임부터
        _animator.Play(_stateName, 0, 0f);
    }

    private IEnumerator CoWatchEnd()
    {
        while (Now < _endTime)
            yield return null;

        // Destroy된 객체 체크
        if (_animator == null) yield break;

        // 종료: 첫 프레임 복귀 + 정지
        _animator.Play(_defaultStateName, 0, 0f);
        _animator.speed = 0f;

        _isPlaying = false;
        _watchCo = null;
    }
}
