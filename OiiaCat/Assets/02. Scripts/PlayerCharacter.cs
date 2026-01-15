using System.Threading;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Cysharp.Threading.Tasks;

/// <summary>
/// 멀티플레이어 플레이어 캐릭터 - 자신만 메뉴 버튼 활성화, 전역 입력 감지로 클릭수 저장
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class PlayerCharacter : MonoBehaviourPun
{
    [Header("UI References")]
    [SerializeField] private GameObject _menuButton;                  // 메뉴 버튼 (자신에게만 보임)
    [SerializeField] private TextMeshProUGUI _infoText;             // 정보 텍스트 (내: 클릭수, 남: 이름)
    [SerializeField] private Transform _canvasTransform;            // Canvas Transform (프리팹에서 할당)

    // 전역 입력 감지기 (자동으로 찾음)
    private GlobalInputActivityDetector_Windows _inputDetector;

    // 클릭수 (PhotonView로 동기화됨)
    [SerializeField] private int _clickCount;

    // 로컬 모드 플래그 (Photon 연결 없이 실행 시)
    private bool _isLocalMode = false;

    /// <summary>
    /// 시작 시 비동기 초기화 (UniTask 사용)
    /// </summary>
    private async UniTaskVoid Start()
    {
        // 취소 토큰 연결 (GameObject 파괴 시 자동 취소)
        var cts = this.GetCancellationTokenOnDestroy();

        await InitializeAsync(cts);
    }

    /// <summary>
    /// 비동기 초기화 (UniTask 사용)
    /// </summary>
    private async UniTask InitializeAsync(CancellationToken cancellationToken)
    {
        // 멀티 모드에서는 Canvas 설정 먼저 실행 (모든 클라이언트에서)
        if (!_isLocalMode)
        {
            SetupCanvas();
        }

        // 로컬 모드이면 즉시 초기화
        if (_isLocalMode)
        {
            Debug.Log("[PlayerCharacter] Local mode - immediate initialization");
            SetupLocalPlayer();

            // 전역 입력 감지기 자동으로 찾기
            _inputDetector = GameObject.FindAnyObjectByType<GlobalInputActivityDetector_Windows>();
            if (_inputDetector != null)
            {
                _inputDetector.OnActivity += OnInputDetected;
            }
            else
            {
                Debug.LogWarning("GlobalInputActivityDetector_Windows not found in scene!");
            }

            UpdateInfoText();
            return;
        }

        // 멀티 모드에서는 PhotonView 준비 대기
        await UniTask.WaitUntil(() => photonView != null && photonView.ViewID != 0, cancellationToken: cancellationToken);

        Debug.Log($"[PlayerCharacter] PhotonView ready. IsMine: {photonView.IsMine}, ViewID: {photonView.ViewID}");

        // 자신의 캐릭터인지 확인
        if (photonView.IsMine)
        {
            SetupLocalPlayer();

            // 전역 입력 감지기 자동으로 찾기
            _inputDetector = GameObject.FindAnyObjectByType<GlobalInputActivityDetector_Windows>();
            if (_inputDetector != null)
            {
                _inputDetector.OnActivity += OnInputDetected;
            }
            else
            {
                Debug.LogWarning("GlobalInputActivityDetector_Windows not found in scene!");
            }
        }
        else
        {
            SetupRemotePlayer();
        }

        // 텍스트 초기화
        UpdateInfoText();
    }

    /// <summary>
    /// Canvas 설정
    /// </summary>
    private void SetupCanvas()
    {
        if (_canvasTransform != null)
        {
            transform.SetParent(_canvasTransform, false);
            Debug.Log($"[PlayerCharacter] SetParent to Canvas - IsMine: {photonView.IsMine}");
        }
        else
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                transform.SetParent(canvas.transform, false);
                Debug.Log($"[PlayerCharacter] SetParent to auto-found Canvas - IsMine: {photonView.IsMine}");
            }
            else
            {
                Debug.LogWarning("[PlayerCharacter] Canvas not found!");
            }
        }
    }

    /// <summary>
    /// 로컬 모드 설정 (Photon 연결 없이)
    /// </summary>
    public void SetLocalMode(bool isLocalMode)
    {
        _isLocalMode = isLocalMode;
        Debug.Log($"[PlayerCharacter] Local mode: {_isLocalMode}");
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if ((_isLocalMode || photonView.IsMine) && _inputDetector != null)
        {
            _inputDetector.OnActivity -= OnInputDetected;
        }
    }

    /// <summary>
    /// 전역 입력 감지시 호출
    /// </summary>
    private void OnInputDetected(uint activityCount)
    {
        // 로컬 모드이거나 룸에 입장했으면 클릭수 증가
        if (_isLocalMode)
        {
            AddClickCount();
        }
        else if (PhotonNetwork.InRoom)
        {
            // 클릭수 증가 (모든 클라이언트에 동기화)
            photonView.RPC(nameof(AddClickCount), RpcTarget.All);
        }
    }

    /// <summary>
    /// 로컬 플레이어 설정 (내 캐릭터)
    /// </summary>
    private void SetupLocalPlayer()
    {
        // 메뉴 버튼 활성화
        if (_menuButton != null)
        {
            _menuButton.SetActive(true);
        }

        // 드래그 가능하게 추가
        if (GetComponent<DraggableCharacter>() == null)
        {
            gameObject.AddComponent<DraggableCharacter>();
        }

        Debug.Log("Local Player Spawned");
    }

    /// <summary>
    /// 원격 플레이어 설정 (남의 캐릭터)
    /// </summary>
    private void SetupRemotePlayer()
    {
        // 메뉴 버튼 비활성화
        if (_menuButton != null)
        {
            _menuButton.SetActive(false);
        }

        // 드래그 가능하게 추가 (모든 캐릭터 드래그 가능)
        if (GetComponent<DraggableCharacter>() == null)
        {
            gameObject.AddComponent<DraggableCharacter>();
        }

        Debug.Log($"Remote Player Spawned: {photonView.Owner.NickName}");
    }

    /// <summary>
    /// 클릭수 증가 RPC
    /// </summary>
    [PunRPC]
    private void AddClickCount()
    {
        _clickCount++;
        UpdateInfoText();
    }

    /// <summary>
    /// 정보 텍스트 업데이트
    /// 내 캐릭터: 클릭수 표시
    /// 남의 캐릭터: 이름 표시
    /// </summary>
    private void UpdateInfoText()
    {
        if (_infoText == null) return;

        if (_isLocalMode || photonView.IsMine)
        {
            // 내 캐릭터: 클릭수 표시
            _infoText.text = $" {_clickCount}";
        }
        else
        {
            // 남의 캐릭터: 이름 표시
            if (photonView.Owner != null)
            {
                _infoText.text = photonView.Owner.NickName;
            }
        }
    }

    /// <summary>
    /// 외부에서 클릭수 증가시킬 때 사용 (예: 버튼 연결)
    /// </summary>
    public void OnClickButton()
    {
        if (_isLocalMode)
        {
            AddClickCount();
        }
        else if (photonView.IsMine)
        {
            photonView.RPC(nameof(AddClickCount), RpcTarget.All);
        }
    }
}