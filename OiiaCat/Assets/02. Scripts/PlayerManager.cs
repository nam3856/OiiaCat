using Kirurobo;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

/// <summary>
/// 포톤 멀티플레이어 매니저 - 플레이어 스폰 담당
/// 로컬 모드로 시작하여 멀티플레이로 전환 가능
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Header("Spawn Settings")]
    [SerializeField]
    private GameObject _playerPrefab;
    [SerializeField]
    private Transform _canvasTransform;
    [SerializeField]
    private Transform[] _spawnPoints;

    private GameObject _localPlayer;
    private bool _hasTransitionedToMulti;

    private void Start()
    {
        // 창 설정 (투명 + 클릭 스루 + 항상 위)
        UniWindowController uniwinc = UniWindowController.current;
        if (uniwinc != null)
        {
            uniwinc.isTopmost = true;              // 항상 위
            uniwinc.isTransparent = true;           // 투명 모드
            uniwinc.forceWindowed = true;           // 창 모드
            uniwinc.isHitTestEnabled = true;        // 히트 테스트 활성화 (중요!)
            uniwinc.hitTestType = UniWindowController.HitTestType.Opacity;  // 투명도 기반 클릭 통과
            uniwinc.opacityThreshold = 0.1f;        // 투명도 임계값
            uniwinc.isClickThrough = false;         // 자동 모드이므로 false로 두고 자동으로 제어
            Debug.Log("[PlayerManager] Window configured: Transparent + Auto ClickThrough");
        }

        // Photon 콜백 타겟 등록
        PhotonNetwork.AddCallbackTarget(this);

        // 앱 시작 시 로컬 플레이어 즉시 스폰
        if (!PhotonNetwork.InRoom && !_hasTransitionedToMulti)
        {
            SpawnLocalPlayer();
        }

        // GameModeManager 이벤트 구독
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnTransitionToMulti += HandleTransitionToMulti;
        }
    }

    /// <summary>
    /// 로컬 플레이어 스폰 (Photon 연결 없이)
    /// </summary>
    private void SpawnLocalPlayer()
    {
        if (_playerPrefab == null)
        {
            Debug.LogError("[PlayerManager] PlayerPrefab not assigned!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        _localPlayer = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);

        // Canvas 찾기 및 설정
        if (_canvasTransform == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null) _canvasTransform = canvas.transform;
        }

        if (_localPlayer != null && _canvasTransform != null)
        {
            _localPlayer.transform.SetParent(_canvasTransform, false);
            Debug.Log("[PlayerManager] Local player set to Canvas");
        }

        // 로컬 모드로 설정
        var playerChar = _localPlayer.GetComponent<PlayerCharacter>();
        if (playerChar != null)
        {
            playerChar.SetLocalMode(true);
        }

        Debug.Log("[PlayerManager] Local player spawned");
    }

    /// <summary>
    /// 멀티플레이어 플레이어 스폰 (Photon 연결 후)
    /// </summary>
    private void SpawnMultiPlayer()
    {
        Debug.Log("[PlayerManager] SpawnMultiPlayer called");

        if (_playerPrefab == null)
        {
            Debug.LogError("[PlayerManager] PlayerPrefab not assigned!");
            return;
        }

        Vector3 spawnPos = GetSpawnPosition();
        string prefabName = _playerPrefab.name;

        // 1. 포톤 네트워크를 통해 캐릭터 생성
        GameObject player = PhotonNetwork.Instantiate(prefabName, spawnPos, Quaternion.identity);

        if (player == null)
        {
            Debug.LogError($"[PlayerManager] Failed to instantiate '{prefabName}'!");
            return;
        }

        Debug.Log($"[PlayerManager] Successfully instantiated '{prefabName}'");

        // 2. Canvas를 부모로 설정 (로컬 클라이언트용)
        if (_canvasTransform == null)
        {
            Canvas canvas = FindAnyObjectByType<Canvas>();
            if (canvas != null)
            {
                _canvasTransform = canvas.transform;
                Debug.Log("[PlayerManager] Canvas found in scene");
            }
        }

        if (_canvasTransform != null)
        {
            // UI 요소인 경우 worldPositionStays를 false로 해야 UI 좌표계가 정상 작동
            player.transform.SetParent(_canvasTransform, false);
            Debug.Log($"[PlayerManager] {player.name} set to Canvas child (local)");
        }

        // 3. 캐릭터 설정 (로컬/멀티 구분)
        var playerChar = player.GetComponent<PlayerCharacter>();
        if (playerChar != null)
        {
            playerChar.SetLocalMode(false);
            Debug.Log("[PlayerManager] Set local mode to false");

            // OnPhotonInstantiate가 자동으로 Canvas 설정을 처리하므로 RPC 불필요
        }
        else
        {
            Debug.LogWarning("[PlayerManager] PlayerCharacter component not found!");
        }

        Debug.Log("[PlayerManager] Multi player spawned and parented to Canvas");
    }

    /// <summary>
    /// 스폰 위치 계산
    /// </summary>
    private Vector3 GetSpawnPosition()
    {
        if (_spawnPoints != null && _spawnPoints.Length > 0)
        {
            if (PhotonNetwork.InRoom)
            {
                int spawnIndex = PhotonNetwork.LocalPlayer.ActorNumber - 1;
                return _spawnPoints[spawnIndex % _spawnPoints.Length].position;
            }
            else
            {
                return _spawnPoints[0].position;
            }
        }
        return Vector3.zero;
    }

    /// <summary>
    /// 로컬→멀티 전환 처리
    /// </summary>
    private void HandleTransitionToMulti()
    {
        // 이미 전환되었으면 무시
        if (_hasTransitionedToMulti)
        {
            Debug.Log("[PlayerManager] Already transitioned to multiplayer, skipping");
            return;
        }

        if (_localPlayer != null)
        {
            Debug.Log("[PlayerManager] HandleTransitionToMulti - destroying local player");
            Destroy(_localPlayer);
            _localPlayer = null;
        }

        _hasTransitionedToMulti = true;
        Debug.Log("[PlayerManager] Transitioned to multiplayer mode");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[PlayerManager] OnJoinedRoom called! HasTransitioned: {_hasTransitionedToMulti}, IsConnected: {PhotonNetwork.IsConnected}");

        // 로컬 플레이어가 있으면 먼저 파괴 (멀티플레이어 전환)
        if (_localPlayer != null && !_hasTransitionedToMulti)
        {
            Debug.Log("[PlayerManager] Destroying local player before spawning multi player");
            Destroy(_localPlayer);
            _localPlayer = null;
            _hasTransitionedToMulti = true;
        }

        // 멀티플레이어 스폰
        if (_hasTransitionedToMulti || PhotonNetwork.IsConnected)
        {
            SpawnMultiPlayer();
        }
        else
        {
            Debug.LogWarning("[PlayerManager] Skipped spawning - not ready");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"[PlayerManager] Disconnected: {cause}");
    }

    private void OnDestroy()
    {
        if (GameModeManager.Instance != null)
        {
            GameModeManager.Instance.OnTransitionToMulti -= HandleTransitionToMulti;
        }

        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
