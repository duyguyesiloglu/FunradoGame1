using TMPro;
using UnityEngine;

public class TimerController : MonoBehaviour
{
    public TextMeshProUGUI timer;
    private float startTime;
    private bool isTimerRunning;

    void Start()
    {
        startTime = Time.time;
        isTimerRunning = true;
    }

    void Update()
    {
        if (isTimerRunning)
        {
            UpdateTimer();
        }
    }

    void UpdateTimer()
    {
        float timeSinceStart = Time.time - startTime;
        int minutes = (int)(timeSinceStart / 60);
        int seconds = (int)(timeSinceStart % 60);
        float milliseconds = (timeSinceStart * 1000) % 1000;

        timer.text = string.Format("{0}m{1:00}.{2:000}s", minutes, seconds, milliseconds);
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void StartTimer()
    {
        startTime = Time.time;
        isTimerRunning = true;
    }
}
