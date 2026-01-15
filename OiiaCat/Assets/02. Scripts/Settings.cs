using Kirurobo;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

/// <summary>
/// 창 설정 및 포톤 멀티플레이어 연결 관리
/// </summary>
public class Settings : MonoBehaviourPunCallbacks
{
    [Header("Window Settings")]
    [SerializeField] private UniWindowController _uniwinc;
    [SerializeField] private Toggle _alwaysUpToggle;
    [SerializeField] private Toggle _transparentToggle;    // 투명 토글
    [SerializeField] private GameObject _backgroundGameObject;  // 배경 GameObject

    [Header("Photon Connection UI")]
    [SerializeField] private TMP_InputField _nicknameInputField;
    [SerializeField] private Button _nicknameConfirmButton;
    [SerializeField] private TMP_InputField _roomNameInputField;
    [SerializeField] private Button _joinRoomButton;
    [SerializeField] private TextMeshProUGUI _statusText;

    [Header("Room Settings")]
    [SerializeField] private byte _maxPlayers = 4;

    private bool _isConnecting;

    private void Start()
    {
        // UniWindowController 초기화
        if (_uniwinc == null)
        {
            _uniwinc = UniWindowController.current;
        }

        // 항상 위 토글 초기화
        if (_alwaysUpToggle != null)
        {
            _alwaysUpToggle.isOn = _uniwinc.isTopmost;
            _alwaysUpToggle.onValueChanged.AddListener(OnAlwaysUpToggleChanged);
        }

        // 투명 토글 초기화
        if (_transparentToggle != null)
        {
            _transparentToggle.isOn = _uniwinc.isTransparent;
            _transparentToggle.onValueChanged.AddListener(OnTransparentToggleChanged);
        }

        // 닉네임 확인 버튼 초기화
        if (_nicknameConfirmButton != null)
        {
            _nicknameConfirmButton.onClick.AddListener(OnNicknameConfirmClicked);
        }

        // 방 입장 버튼 초기화
        if (_joinRoomButton != null)
        {
            _joinRoomButton.onClick.AddListener(OnJoinRoomClicked);
        }

        // 닉네임 입력 필드 초기화 (이전 닉네임이 있으면 로드)
        if (_nicknameInputField != null)
        {
            _nicknameInputField.text = PhotonNetwork.NickName;
        }

        // 방 이름 입력 필드 초기화
        if (_roomNameInputField != null)
        {
            _roomNameInputField.text = "OiiaRoom";
        }
    }

    private void OnAlwaysUpToggleChanged(bool isOn)
    {
        _uniwinc.isTopmost = isOn;
    }

    /// <summary>
    /// 투명 토글 변경 시 호출
    /// </summary>
    private void OnTransparentToggleChanged(bool isOn)
    {
        // 배경 GameObject 켜고 끄기
        if (_backgroundGameObject != null)
        {
            _backgroundGameObject.SetActive(!isOn);  // 투명이면 BG 끄기
            Debug.Log($"[Settings] Transparent mode: {isOn}, Background active: {!isOn}");
        }
    }

    private void OnNicknameConfirmClicked()
    {
        // 닉네임 설정
        if (_nicknameInputField != null && !string.IsNullOrWhiteSpace(_nicknameInputField.text))
        {
            PhotonNetwork.NickName = _nicknameInputField.text;
            UpdateStatus($"닉네임 변경: {PhotonNetwork.NickName}");
        }
        else
        {
            UpdateStatus("닉네임을 입력해주세요.");
        }
    }

    private void OnJoinRoomClicked()
    {
        if (_isConnecting) return;

        // 이미 방에 있는 경우
        if (PhotonNetwork.InRoom)
        {
            UpdateStatus("이미 방에 입장한 상태입니다.");
            return;
        }

        // 방 이름 가져오기
        string roomName = "OiiaRoom";
        if (_roomNameInputField != null && !string.IsNullOrWhiteSpace(_roomNameInputField.text))
        {
            roomName = _roomNameInputField.text;
        }

        // 닉네임이 설정되어 있는지 확인
        if (_nicknameInputField != null && !string.IsNullOrWhiteSpace(_nicknameInputField.text))
        {
            PhotonNetwork.NickName = _nicknameInputField.text;
        }
        else if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            // 랜덤 닉네임 생성
            PhotonNetwork.NickName = $"Player{Random.Range(1000, 9999)}";
        }

        _isConnecting = true;
        UpdateStatus($"'{roomName}' 입장 중...");

        if (_joinRoomButton != null)
        {
            _joinRoomButton.interactable = false;
        }

        // 포톤 연결 상태 확인
        if (PhotonNetwork.IsConnectedAndReady)
        {
            // 로비에 입장하지 않은 경우 먼저 입장
            if (!PhotonNetwork.InLobby)
            {
                PhotonNetwork.JoinLobby();
            }
            else
            {
                // 로비에 있으면 바로 방 입장 시도
                PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = _maxPlayers }, TypedLobby.Default);
            }
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = "1";
        }
    }

    public override void OnConnectedToMaster()
    {
        UpdateStatus("서버 연결 성공! 로비 입장 중...");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // 방 이름 가져오기
        string roomName = "OiiaRoom";
        if (_roomNameInputField != null && !string.IsNullOrWhiteSpace(_roomNameInputField.text))
        {
            roomName = _roomNameInputField.text;
        }

        UpdateStatus("로비 입장 성공! 룸 참여 중...");
        PhotonNetwork.JoinOrCreateRoom(roomName, new RoomOptions { MaxPlayers = _maxPlayers }, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[Settings] OnJoinedRoom called!");

        UpdateStatus($"룸 입장 성공! ({PhotonNetwork.CurrentRoom.PlayerCount}/{PhotonNetwork.CurrentRoom.MaxPlayers})");
        _isConnecting = false;

        if (_joinRoomButton != null)
        {
            _joinRoomButton.interactable = true;
        }

        // 멀티플레이 전환 요청
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.RequestTransitionToMulti();
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        UpdateStatus($"룸 입장 실패: {message}");
        _isConnecting = false;

        if (_joinRoomButton != null)
        {
            _joinRoomButton.interactable = true;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        UpdateStatus($"연결 끊김: {cause}");
        _isConnecting = false;

        if (_joinRoomButton != null)
        {
            _joinRoomButton.interactable = true;
        }

        // 로컬 모드로 복귀
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.ResetToLocal();
        }
    }

    private void UpdateStatus(string message)
    {
        if (_statusText != null)
        {
            _statusText.text = message;
        }
        Debug.Log($"[Settings] {message}");
    }

    private void OnDestroy()
    {
        if (_alwaysUpToggle != null)
        {
            _alwaysUpToggle.onValueChanged.RemoveListener(OnAlwaysUpToggleChanged);
        }

        if (_transparentToggle != null)
        {
            _transparentToggle.onValueChanged.RemoveListener(OnTransparentToggleChanged);
        }

        if (_nicknameConfirmButton != null)
        {
            _nicknameConfirmButton.onClick.RemoveListener(OnNicknameConfirmClicked);
        }

        if (_joinRoomButton != null)
        {
            _joinRoomButton.onClick.RemoveListener(OnJoinRoomClicked);
        }
    }
}
