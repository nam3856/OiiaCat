using UnityEngine;
using System;

/// <summary>
/// 게임 모드 상태 관리 (로컬/멀티플레이어 전환)
/// </summary>
public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    public enum GameMode { Local, Multi }
    public GameMode CurrentMode { get; private set; } = GameMode.Local;

    public event Action OnTransitionToMulti;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 멀티플레이어 모드로 전환 요청
    /// </summary>
    public void RequestTransitionToMulti()
    {
        if (CurrentMode == GameMode.Local)
        {
            CurrentMode = GameMode.Multi;
            OnTransitionToMulti?.Invoke();
            Debug.Log("[GameModeManager] Local → Multi transition");
        }
    }

    /// <summary>
    /// 로컬 모드로 재설정 (연결 끊김 시)
    /// </summary>
    public void ResetToLocal()
    {
        CurrentMode = GameMode.Local;
        Debug.Log("[GameModeManager] Reset to Local mode");
    }
}
