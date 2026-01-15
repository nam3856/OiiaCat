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
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private string _stateName = "Gif";

    [SerializeField]
    private string _defaultStateName = "Idle";

    [SerializeField]
    private float _playDuration = 0.5f;

    [SerializeField]
    private bool _useUnscaledTime = false;

    // 전역 입력 감지기 (자동으로 찾음)
    private GlobalInputActivityDetector_Windows _inputCounter;

    [Header("Behavior")]
    [SerializeField]
    private bool _restartFromFirstFrameOnTrigger = false;

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
    /// 입력 트리거 (내 캐릭터에서만 호출됨)
    /// </summary>
    public void Trigger(uint count)
    {
        // Destroy된 객체 체크
        if (this == null) return;
        if (_animator == null) return;

        // 로컬 모드 체크
        bool isLocalMode = !PhotonNetwork.InRoom;

        // 내 캐릭터인지 확인 (로컬 모드이거나 IsMine)
        if (!isLocalMode && !photonView.IsMine) return;

        // 로컬에서 즉시 재생
        PlayAnimation();

        // 멀티 모드에서만 RPC 전송
        if (!isLocalMode && PhotonNetwork.InRoom)
        {
            photonView.RPC(nameof(RPCPlayAnimation), RpcTarget.Others);
        }
    }

    /// <summary>
    /// RPC 애니메이션 재생 (다른 클라이언트에서 실행)
    /// </summary>
    [PunRPC]
    private void RPCPlayAnimation()
    {
        if (this == null) return;
        if (_animator == null) return;

        PlayAnimation();
        Debug.Log($"[GifBurstPlayer] RPC animation played - Sender: {photonView.Owner.NickName}");
    }

    /// <summary>
    /// 애니메이션 재생 (공통 로직)
    /// </summary>
    private void PlayAnimation()
    {
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
