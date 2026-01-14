using Kirurobo;
using UnityEngine;
using UnityEngine.UI;
public class Settings : MonoBehaviour
{
    [SerializeField]
    private UniWindowController uniwinc;

    [SerializeField]
    private Toggle _alwaysUpToggle;

    [SerializeField]
    private Toggle _transparentToggle;

    [SerializeField]
    private GameObject _background;

    private void Start()
    {
        if(uniwinc == null)
        {
            uniwinc = UniWindowController.current;
        }
    }

    private void OnEnable()
    {
        if (_alwaysUpToggle != null)
        {
            _alwaysUpToggle.isOn = uniwinc.isTopmost;
            _alwaysUpToggle.onValueChanged.AddListener(val => uniwinc.isTopmost = val);
        }
        if (_transparentToggle != null)
        {
            _transparentToggle.isOn = !_background.activeSelf; 
            _transparentToggle.onValueChanged.AddListener(val => _background.SetActive(!val));
        }
    }

    private void OnDisable()
    {
        if(_alwaysUpToggle != null)
        {
            _alwaysUpToggle.onValueChanged.RemoveAllListeners();
        }
        if(_transparentToggle != null)
        {
            _transparentToggle.onValueChanged.RemoveAllListeners();
        }
    }
}