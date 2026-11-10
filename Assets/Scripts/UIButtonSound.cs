
using UnityEngine;

public class UIButtonSound : MonoBehaviour
{
    [Header("Sonido de los botones")]
    public AudioClip clickSound;

    [Range(0f, 3f)]
    public float volume = 1.5f;

    public void PlayClick()
    {
        if (clickSound != null)
        {
            AudioSource.PlayClipAtPoint(
                clickSound,
                Vector3.zero,
                volume
            );
        }
    }
}
