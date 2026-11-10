
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class TimerUI : MonoBehaviour
{
    public TMP_Text timerText;

    private void Update()
    {
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsListening)
        {
            return;
        }

        GameTimer timer = FindFirstObjectByType<GameTimer>();

        if (timer == null)
            return;

        float time = Mathf.Max(0f, timer.TimeRemaining.Value);

        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timerText.text = minutes.ToString("00") + ":" +
                         seconds.ToString("00");
    }
}
