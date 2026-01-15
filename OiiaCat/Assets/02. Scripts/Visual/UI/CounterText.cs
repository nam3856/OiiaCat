using TMPro;
using UnityEngine;

public class CounterText : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI counter;

    [SerializeField]
    private GlobalInputActivityDetector_Windows inputCounter;

    private void Start()
    {
        inputCounter.OnActivity += InputCounter_OnCountChanged;
    }

    private void InputCounter_OnCountChanged(uint newCount)
    {
        counter.text = newCount.ToString();
    }
}
