using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine;

public class GlobalInputActivityDetector_Windows : MonoBehaviour
{
    public event Action<uint> OnActivity; // "뭔가 입력이 있었다" 신호

    [SerializeField] private bool runInBackground = true;
    [SerializeField] private bool detectKeyboard = true;
    [SerializeField] private bool detectMouseClick = true;

    private uint _activityCount = 0;

    // Hook handles
    private IntPtr _keyboardHookId = IntPtr.Zero;
    private IntPtr _mouseHookId = IntPtr.Zero;

    // Prevent GC collection of delegates
    private LowLevelKeyboardProc _keyboardProc;
    private LowLevelMouseProc _mouseProc;

    // 메인 스레드에서 이벤트 호출을 위한 큐
    private readonly object _lock = new object();
    private int _pendingActivityCount = 0;

    // 키 반복 필터링을 위한 눌린 키 추적
    private readonly HashSet<uint> _pressedKeys = new HashSet<uint>();

    private void Awake()
    {
        if (runInBackground)
            Application.runInBackground = true;
    }

    private void OnEnable()
    {
        if (detectKeyboard)
        {
            _keyboardProc = KeyboardHookCallback;
            _keyboardHookId = SetKeyboardHook(_keyboardProc);
        }

        if (detectMouseClick)
        {
            _mouseProc = MouseHookCallback;
            _mouseHookId = SetMouseHook(_mouseProc);
        }
    }

    private void OnDisable()
    {
        if (_keyboardHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_keyboardHookId);
            _keyboardHookId = IntPtr.Zero;
        }

        if (_mouseHookId != IntPtr.Zero)
        {
            UnhookWindowsHookEx(_mouseHookId);
            _mouseHookId = IntPtr.Zero;
        }

        // Hook 해제 시 눌린 키 상태 초기화
        lock (_lock)
        {
            _pressedKeys.Clear();
        }
    }

    private void Update()
    {
        // 메인 스레드에서 이벤트 발생
        int pending;
        lock (_lock)
        {
            pending = _pendingActivityCount;
            _pendingActivityCount = 0;
        }

        for (int i = 0; i < pending; i++)
        {
            _activityCount++;
            OnActivity?.Invoke(_activityCount);
        }
    }

    private void RegisterActivity()
    {
        lock (_lock)
        {
            _pendingActivityCount++;
        }
    }

    #region Keyboard Hook

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYUP = 0x0105;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = wParam.ToInt32();
            var kbStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            uint vkCode = kbStruct.vkCode;

            if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
            {
                // 이미 눌려있는 키면 무시 (키 반복 필터링)
                lock (_lock)
                {
                    if (_pressedKeys.Contains(vkCode))
                    {
                        // 키 반복 - 무시
                        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
                    }
                    _pressedKeys.Add(vkCode);
                }
                RegisterActivity();
            }
            else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
            {
                // 키 뗌 - 추적에서 제거
                lock (_lock)
                {
                    _pressedKeys.Remove(vkCode);
                }
            }
        }
        return CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private static IntPtr SetKeyboardHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    #endregion

    #region Mouse Hook

    private const int WH_MOUSE_LL = 14;
    private const int WM_LBUTTONDOWN = 0x0201;
    private const int WM_RBUTTONDOWN = 0x0204;
    private const int WM_MBUTTONDOWN = 0x0207;
    private const int WM_XBUTTONDOWN = 0x020B;

    private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

    private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            int msg = wParam.ToInt32();
            // 마우스 클릭만 감지 (이동 제외)
            if (msg == WM_LBUTTONDOWN || msg == WM_RBUTTONDOWN || 
                msg == WM_MBUTTONDOWN || msg == WM_XBUTTONDOWN)
            {
                RegisterActivity();
            }
        }
        return CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
    }

    private static IntPtr SetMouseHook(LowLevelMouseProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    #endregion

    #region Win32 Imports

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    #endregion
}
