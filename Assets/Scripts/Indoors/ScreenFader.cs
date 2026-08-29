using System.Threading.Tasks;
using System.Collections;
using UnityEngine;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader instance;
    public CanvasGroup canvasGroup;
    public float fadeDuration = 2f;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    async Task Fade(float targetTransparency)
    {
        float start = canvasGroup.alpha, t = 0;
        while(t < fadeDuration)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, targetTransparency, t / fadeDuration);
            await Task.Yield();
        }
        canvasGroup.alpha = targetTransparency;
    }
    public async Task FadeOut()
    {
        await Fade(1);
    }
    public async Task FadeIn()
    {
        await Fade(0);
    }
}
