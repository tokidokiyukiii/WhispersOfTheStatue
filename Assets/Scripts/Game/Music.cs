using UnityEngine;

public class Music : MonoBehaviour
{
    public AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void OnDestroy()
    {
        if (audioSource != null)
            audioSource.Stop();
    }
}
