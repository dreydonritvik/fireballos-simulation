using System;
using TMPro;
using UnityEngine;

public class SystemClock : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timeText;

    private void Start()
    {
        if (timeText == null)
        {
            timeText = GetComponent<TextMeshProUGUI>();
        }

        UpdateTime();
        InvokeRepeating(nameof(UpdateTime), 1f, 1f);
    }

    private void UpdateTime()
    {
        if (timeText != null)
        {
            timeText.text = DateTime.Now.ToString("HH:mm");
        }
    }
}