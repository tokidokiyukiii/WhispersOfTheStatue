using UnityEngine;
using UnityEngine.UI;

public class BonusEffects : MonoBehaviour
{
    [Header("Animation Settings")]
    public float fadeDuration = 1f;
    public float moveUpDistance = 40f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Text _text;
    private RectTransform _rect;
    private float _startTime;
    private Vector2 _startPos;

    /// <summary>
    /// Initialize and start the floating animation
    /// </summary>
    public void Init(string text, Color color, Transform parent)
    {
        _text = GetComponent<Text>();
        _rect = GetComponent<RectTransform>();

        if (_text != null)
        {
            _text.text = text;
            _text.color = color;
            _text.fontSize = 50;
            _text.fontStyle = FontStyle.Bold;

            // Add outline for visibility
            var outline = GetComponent<Outline>();
            if (outline == null) outline = gameObject.AddComponent<Outline>();
            outline.effectColor = Color.black;
            outline.effectDistance = new Vector2(2, 2);
        }

        // Position at parent center
        transform.SetParent(parent, false);
        _rect.anchoredPosition = Vector2.zero;
        _startPos = _rect.anchoredPosition;
        _startTime = Time.time;

        // Auto-destroy after animation
        Destroy(gameObject, fadeDuration + 0.2f);
    }

    private void Update()
    {
        if (_text == null) return;

        float elapsed = Time.time - _startTime;
        float t = Mathf.Clamp01(elapsed / fadeDuration);

        // Move up
        _rect.anchoredPosition = _startPos + Vector2.up * moveUpDistance * t;

        // Fade out
        Color c = _text.color;
        c.a = 1f - fadeCurve.Evaluate(t);
        _text.color = c;
    }
}
