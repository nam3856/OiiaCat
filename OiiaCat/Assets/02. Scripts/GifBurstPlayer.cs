using System.Collections;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// GIF 버스트 플레이어 - 멀티플레이어 지원
/// 내 입력만 감지하고, RPC로 모든 클라이언트에 애니메이션 재생 전송
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class GifBurstPlayer : MonoBehaviourPun
{
    /// 애니메이션 재생 모드
    public enum PlayMode
    {
        Random,      // 랜덤 재생
        Sequential   // 순차 재생
    }

    [SerializeField]
    private Animator _animator;                                  // 애니메이터 컴포넌트
                    
    [Header("Animation States")]
    [SerializeField]
    private string[] _stateNames = { "Gif" };                   // 재생할 애니메이션 상태 이름 배열

    [SerializeField]
    private PlayMode _playMode = PlayMode.Random;               // 애니메이션 재생 모드 (Random: 랜덤 재생, Sequential: 순차 재생)

    [SerializeField]
    private string _defaultStateName = "Idle";                  // 기본 상태 이름 (애니메이션 종료 후 복귀할 상태)

    [SerializeField]
    private float _playDuration = 0.5f;                         // 애니메이션 재생 지속 시간 (마지막 입력 기준)

    [SerializeField]
    private bool _useUnscaledTime = false;                      // Unscaled Time 사용 여부 (Time.timeScale 영향을 받지 않음)

    [Header("Behavior")]
    [SerializeField]
    private bool _restartFromFirstFrameOnTrigger = false;       // 재생 중 트리거 시 첫 프레임부터 재시작 여부


    private GlobalInputActivityDetector_Windows _inputCounter;  // 전역 입력 감지기 (자동으로 찾음)
    private bool _isPlaying;                                    // 현재 재생 중인지 여부
    private float _endTime;                                     // 애니메이션 종료 예정 시각
    private Coroutine _watchCo;                                 // 종료 감시 코루틴 참조
    private int _currentSequentialIndex = 0;                    // Sequential 모드에서 현재 인덱스
    private string _currentPlayingState = "";                   // 현재 재생 중인 애니메이션 상태 이름
    private float Now => _useUnscaledTime ? Time.unscaledTime : Time.time;     // 현재 시간 (useUnscaledTime 설정에 따라 분기)

    /// <summary>
    /// 컴포넌트 리셋 시 Animator를 자동으로 할당
    /// </summary>
    private void Reset()
    {
        _animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Awake 시 Animator 초기화 및 첫 프레임 고정
    /// </summary>
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

    /// <summary>
    /// Start 시 로컬 캐릭터인 경우 입력 감지기 구독
    /// </summary>
    private void Start()
    {
        // 로컬 모드 체크
        bool isLocalMode = !PhotonNetwork.InRoom;

        // 로컬 모드이거나 내 캐릭터인 경우에만 입력 감지
        if (isLocalMode || photonView.IsMine)
        {
            // 전역 입력 감지기 자동으로 찾기
            _inputCounter = GameObject.FindAnyObjectByType<GlobalInputActivityDetector_Windows>();
            if (_inputCounter != null)
            {
                _inputCounter.OnActivity += Trigger;
                Debug.Log($"[GifBurstPlayer] Subscribed to input events (LocalMode: {isLocalMode}, IsMine: {photonView.IsMine})");
            }
            else
            {
                Debug.LogWarning("GlobalInputActivityDetector_Windows not found in scene!");
            }
        }
    }

    /// <summary>
    /// 오브젝트 파괴 시 이벤트 구독 해제
    /// </summary>
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (_inputCounter != null)
        {
            bool isLocalMode = !PhotonNetwork.InRoom;
            if (isLocalMode || photonView.IsMine)
            {
                _inputCounter.OnActivity -= Trigger;
            }
        }
    }

    /// <summary>
    /// 재생할 애니메이션 상태 선택 (PlayMode에 따라 Random 또는 Sequential)
    /// </summary>
    /// <returns>선택된 애니메이션 상태 이름</returns>
    private string GetNextStateName()
    {
        if (_stateNames == null || _stateNames.Length == 0)
        {
            Debug.LogWarning("[GifBurstPlayer] No state names defined!");
            return "Gif";
        }

        if (_stateNames.Length == 1)
        {
            return _stateNames[0];
        }

        switch (_playMode)
        {
            case PlayMode.Random:
                return _stateNames[Random.Range(0, _stateNames.Length)];

            case PlayMode.Sequential:
                string state = _stateNames[_currentSequentialIndex];
                _currentSequentialIndex = (_currentSequentialIndex + 1) % _stateNames.Length;
                return state;

            default:
                return _stateNames[0];
        }
    }

    /// <summary>
    /// 입력 트리거 (내 캐릭터에서만 호출됨)
    /// </summary>
    /// <param name="count">입력 카운트</param>
    public void Trigger(uint count)
    {
        // Destroy된 객체 체크
        if (this == null) return;
        if (_animator == null) return;

        // 로컬 모드 체크
        bool isLocalMode = !PhotonNetwork.InRoom;

        // 내 캐릭터인지 확인 (로컬 모드이거나 IsMine)
        if (!isLocalMode && !photonView.IsMine) return;

        // 재생할 애니메이션 선택
        string selectedState = GetNextStateName();

        // 로컬에서 즉시 재생
        PlayAnimation(selectedState);

        // 멀티 모드에서만 RPC 전송
        if (!isLocalMode && PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(RPCPlayAnimation), RpcTarget.Others, selectedState);
        }
    }

    /// <summary>
    /// RPC 애니메이션 재생 (다른 클라이언트에서 실행)
    /// </summary>
    /// <param name="stateName">재생할 애니메이션 상태 이름</param>
    [PunRPC]
    private void RPCPlayAnimation(string stateName)
    {
        if (this == null) return;
        if (_animator == null) return;

        PlayAnimation(stateName);
        Debug.Log($"[GifBurstPlayer] RPC animation played - State: {stateName}, Sender: {photonView.Owner.NickName}");
    }

    /// <summary>
    /// 애니메이션 재생 (공통 로직)
    /// 재생 중이 아니면 새로 시작, 재생 중이면 duration 연장 또는 재시작
    /// </summary>
    /// <param name="stateName">재생할 애니메이션 상태 이름</param>
    private void PlayAnimation(string stateName)
    {
        // "마지막 입력 기준 0.5초"로 종료 시각 갱신
        _endTime = Now + _playDuration;

        if (!_isPlaying)
        {
            StartPlaying(stateName);
            _watchCo = StartCoroutine(CoWatchEnd());
            return;
        }

        // 재생 중인데 또 트리거 됨
        // 다른 애니메이션이 선택되었거나, 재시작 옵션이 켜져 있으면 재생
        if (stateName != _currentPlayingState || _restartFromFirstFrameOnTrigger)
        {
            _animator.Play(stateName, 0, 0f);
            _currentPlayingState = stateName;
        }
        // 같은 애니메이션이고 재시작 옵션 OFF면 duration만 연장
    }

    /// <summary>
    /// 애니메이션 재생 시작 (0프레임부터 시작, speed 1로 설정)
    /// </summary>
    /// <param name="stateName">재생할 애니메이션 상태 이름</param>
    private void StartPlaying(string stateName)
    {
        if (_animator == null) return;

        _isPlaying = true;
        _animator.speed = 1f;
        _currentPlayingState = stateName;

        // 처음 시작 시에는 0프레임부터
        _animator.Play(stateName, 0, 0f);
    }

    /// <summary>
    /// 애니메이션 종료 시각을 감시하는 코루틴
    /// 종료 시 기본 상태로 복귀하고 정지
    /// </summary>
    /// <returns>코루틴 IEnumerator</returns>
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
