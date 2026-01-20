using Kirurobo;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

/// <summary>
/// 창 설정 및 포톤 멀티플레이어 연결 관리
/// </summary>
[RequireComponent(typeof(PhotonView))]
public class Settings : MonoBehaviourPunCallbacks
{
    private PhotonView _photonView;
    [Header("Menu Buttons")]
    [SerializeField]
    private Button _menuButton;
    [SerializeField]
    private Button _catButton;

    [Header("UI Panels")]
    [SerializeField]
    private GameObject _menuPanel;
    [SerializeField]
    private GameObject _catPanel;

    [Header("Window Settings")]
    [SerializeField]
    private UniWindowController _uniwinc;

    [SerializeField]
    private Toggle _alwaysUpToggle;

    [SerializeField]
    private Toggle _transparentToggle;    // 투명 토글

    [SerializeField]
    private GameObject _backgroundGameObject;  // 배경 GameObject

    [Header("Photon Connection UI")]
    [SerializeField]
    private TMP_InputField _nicknameInputField;

    [SerializeField]
    private Button _nicknameConfirmButton;

    [SerializeField]
    private TMP_InputField _roomNameInputField;

    [SerializeField]
    private Button _joinRoomButton;

    [SerializeField]
    private TextMeshProUGUI _statusText;

    [Header("Room Settings")]
    [SerializeField]
    private byte _maxPlayers = 4;

    [Header("Cat Selection")]
    [SerializeField]
    private List<Button> _catButtonList;

    [SerializeField]
    private List<GameObject> _catObjectList;

    // 상수 (스타일 가이드 1-i-(1))
    private const string DEFAULT_ROOM_NAME = "OiiaRoom";
    private const string DEFAULT_NICKNAME_PREFIX = "Player";
    private const int MIN_NICKNAME_NUMBER = 1000;
    private const int MAX_NICKNAME_NUMBER = 9999;
    private const string GAME_VERSION = "1";

    private bool _isConnecting;

    private void Start()
    {
        // PhotonView 초기화
        _photonView = GetComponent<PhotonView>();

        // UniWindowController 초기화
        if (_uniwinc == null)
        {
            _uniwinc = UniWindowController.current;
        }

        // 메뉴 버튼 초기화
        if (_menuButton != null)
        {
            _menuButton.onClick.AddListener(OnMenuButtonClicked);
        }

        // 고양이 버튼 초기화
        if (_catButton != null)
        {
            _catButton.onClick.AddListener(OnCatButtonClicked);
        }

        // 패널 초기 상태 설정 (메뉴 패널 활성화)
        if (_menuPanel != null)
        {
            _menuPanel.SetActive(true);
        }
        if (_catPanel != null)
        {
            _catPanel.SetActive(false);
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
            _roomNameInputField.text = DEFAULT_ROOM_NAME;
        }

        // 고양이 선택 버튼 초기화
        InitializeCatSelectionButtons();
    }

    /// <summary>
    /// 고양이 선택 버튼 초기화
    /// </summary>
    private void InitializeCatSelectionButtons()
    {
        if (_catButtonList == null || _catObjectList == null)
        {
            return;
        }

        // 버튼과 오브젝트 개수가 일치하지 않으면 경고
        if (_catButtonList.Count != _catObjectList.Count)
        {
            Debug.LogWarning("[Settings] CatButtonList와 CatObjectList의 개수가 일치하지 않습니다.");
            return;
        }

        // 각 버튼에 리스너 추가
        for (int i = 0; i < _catButtonList.Count; i++)
        {
            int index = i; // 클로저 캡처를 위한 복사
            if (_catButtonList[i] != null)
            {
                _catButtonList[i].onClick.AddListener(() => OnCatSelectionButtonClicked(index));
            }
        }

        // 첫 번째 고양이 오브젝트 활성화 (나머지 비활성화)
        if (_catObjectList.Count > 0)
        {
            ActivateCatObject(0);
        }
    }

    /// <summary>
    /// 고양이 선택 버튼 클릭 시 호출
    /// </summary>
    /// <param name="index">선택된 고양이 인덱스</param>
    private void OnCatSelectionButtonClicked(int index)
    {
        // 로컬에서 활성화
        ActivateCatObject(index);

        // 멀티플레이 모드에서는 다른 플레이어에게도 전송
        if (PhotonNetwork.InRoom && _photonView != null)
        {
            _photonView.RPC(nameof(RPCChangeCat), RpcTarget.Others, index);
            Debug.Log($"[Settings] RPC로 고양이 {index}번 변경 전송");
        }
    }

    /// <summary>
    /// 지정된 인덱스의 고양이 오브젝트만 활성화하고 나머지는 비활성화
    /// </summary>
    /// <param name="index">활성화할 고양이 오브젝트 인덱스</param>
    public void ActivateCatObject(int index)
    {
        if (_catObjectList == null || index < 0 || index >= _catObjectList.Count)
        {
            Debug.LogWarning($"[Settings] 잘못된 고양이 인덱스: {index}");
            return;
        }

        for (int i = 0; i < _catObjectList.Count; i++)
        {
            if (_catObjectList[i] != null)
            {
                _catObjectList[i].SetActive(i == index);
            }
        }

        // 활성화된 고양이의 애니메이션 실행
        GameObject activeCat = _catObjectList[index];
        if (activeCat != null)
        {
            var gifPlayer = activeCat.GetComponent<GifBurstPlayer>();
            if (gifPlayer != null)
            {
                gifPlayer.Trigger(1);
            }
        }

        Debug.Log($"[Settings] 고양이 {index}번 활성화");
    }

    /// <summary>
    /// 다른 플레이어에게 고양이 변경 전송 (RPC)
    /// </summary>
    /// <param name="index">선택한 고양이 인덱스</param>
    [PunRPC]
    private void RPCChangeCat(int index)
    {
        ActivateCatObject(index);
        Debug.Log($"[Settings] RPC로 고양이 {index}번 변경 받음");
    }

    /// <summary>
    /// 메뉴 버튼 클릭 시 호출
    /// </summary>
    private void OnMenuButtonClicked()
    {
        ActivatePanel(_menuPanel);
    }

    /// <summary>
    /// 고양이 버튼 클릭 시 호출
    /// </summary>
    private void OnCatButtonClicked()
    {
        ActivatePanel(_catPanel);
    }

    /// <summary>
    /// 지정된 패널만 활성화하고 나머지는 비활성화
    /// </summary>
    /// <param name="panelToActivate">활성화할 패널</param>
    private void ActivatePanel(GameObject panelToActivate)
    {
        if (_menuPanel != null)
        {
            _menuPanel.SetActive(_menuPanel == panelToActivate);
        }
        if (_catPanel != null)
        {
            _catPanel.SetActive(_catPanel == panelToActivate);
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
        string roomName = DEFAULT_ROOM_NAME;
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
            PhotonNetwork.NickName = $"{DEFAULT_NICKNAME_PREFIX}{Random.Range(MIN_NICKNAME_NUMBER, MAX_NICKNAME_NUMBER)}";
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
            PhotonNetwork.GameVersion = GAME_VERSION;
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
        string roomName = DEFAULT_ROOM_NAME;
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
        if (_menuButton != null)
        {
            _menuButton.onClick.RemoveListener(OnMenuButtonClicked);
        }

        if (_catButton != null)
        {
            _catButton.onClick.RemoveListener(OnCatButtonClicked);
        }

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

        // 고양이 선택 버튼 리스너 제거
        if (_catButtonList != null)
        {
            foreach (var button in _catButtonList)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }
    }
}
