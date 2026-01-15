using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class OIIAPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] 
    private AudioSource _audioSource;
    [SerializeField] 
    private Dictionary<ECatList, AudioClip> _audioClip;

    [Header("Settings")]
    [SerializeField]
    private float _playDuration = 1f;
    [SerializeField]
    private bool _useUnscaledTime = false;


    private float _endTime = 0f;
    private bool _isPlaying = false;
    private Coroutine _watchCo = null;

    private GlobalInputActivityDetector_Windows _globalInputActivityDetector;

    private float Now => _useUnscaledTime ? Time.unscaledTime : Time.time;
    private void Start()
    {
        _globalInputActivityDetector = FindAnyObjectByType<GlobalInputActivityDetector_Windows>();

        if (_globalInputActivityDetector != null)
        {
            _globalInputActivityDetector.OnActivity += HandleInputActivityDetected;
        }
        else
        {
            Debug.LogWarning("GlobalInputActivityDetector_Windows not found in the scene.");
        }
    }

    public void ChangeClip(ECatList catlist)
    {
        if (_audioClip.ContainsKey(catlist))
        {
            _audioSource.clip = _audioClip[catlist];
        }
        else
        {
            Debug.LogWarning("Audio clip for " + catlist + " not found.");
        }
    }

    private void HandleInputActivityDetected(uint _)
    {
        if (!_audioSource.isPlaying)
        {
            _audioSource.Play();
            _watchCo = StartCoroutine(CoWatchEnd());
        }
        _endTime = Now + _playDuration;

    }

    private IEnumerator CoWatchEnd()
    {
        while (Now < _endTime)
            yield return null;

        // Destroy된 객체 체크
        if (_audioSource == null) yield break;

        _audioSource.Stop();

        _isPlaying = false;
        _watchCo = null;
    }


}
