using System.Collections;
using UnityEngine;

public class GifBurstPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string stateName = "Gif";
    [SerializeField] private string defaultStateName = "Idle";
    [SerializeField] private float playDuration = 0.5f;
    [SerializeField] private bool useUnscaledTime = false;
    [SerializeField] private GlobalInputActivityDetector_Windows inputCounter;

    [Header("Behavior")]
    [SerializeField] private bool restartFromFirstFrameOnTrigger = false; // true면 다시 누를 때마다 0프레임부터

    private bool _isPlaying;
    private float _endTime;
    private Coroutine _watchCo;

    private float Now => useUnscaledTime ? Time.unscaledTime : Time.time;

    private void Reset()
    {
        animator = GetComponent<Animator>();
    }

    private void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();

        // 시작은 첫 프레임 고정
        animator.speed = 0f;
        _isPlaying = false;
    }

    private void Start()
    {
        if (inputCounter != null)
        {
            inputCounter.OnActivity += Trigger;
        }
    }

    public void Trigger(uint _)
    {
        // “마지막 입력 기준 0.5초”로 종료 시각 갱신
        _endTime = Now + playDuration;

        if (!_isPlaying)
        {
            StartPlaying();
            _watchCo = StartCoroutine(CoWatchEnd());
            return;
        }

        // 재생 중인데 또 트리거 됨
        if (restartFromFirstFrameOnTrigger)
        {
            animator.Play(stateName, 0, 0f); // 원하면 다시 0프레임부터
        }
        // 아니면 그냥 계속 재생하면서 duration만 연장
    }

    private void StartPlaying()
    {
        _isPlaying = true;
        animator.speed = 1f;

        // 처음 시작 시에는 0프레임부터
        animator.Play(stateName, 0, 0f);
    }

    private IEnumerator CoWatchEnd()
    {
        while (Now < _endTime)
            yield return null;

        // 종료: 첫 프레임 복귀 + 정지
        animator.Play(defaultStateName, 0, 0f);
        animator.speed = 0f;

        _isPlaying = false;
        _watchCo = null;
    }
}
