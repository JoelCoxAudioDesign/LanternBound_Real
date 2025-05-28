using UnityEngine;
using System.Collections;

/// <summary>
/// A platform that becomes visible/solid when illuminated by the lantern
/// Perfect for your "reveal hidden platforms" mechanic
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class RevealablePlatform : MonoBehaviour, ILightInteractable
{
    [Header("Reveal Settings")]
    [SerializeField] private bool _startVisible = false;
    [SerializeField] private float _fadeSpeed = 2f;
    [SerializeField] private bool _solidWhenVisible = true;

    [Header("Visual Settings")]
    [SerializeField] private Color _visibleColor = Color.white;
    [SerializeField] private Color _hiddenColor = new Color(1f, 1f, 1f, 0.2f);

    public bool IsIlluminated { get; private set; }

    private SpriteRenderer _spriteRenderer;
    private Collider2D _platformCollider;
    private Coroutine _fadeCoroutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _platformCollider = GetComponent<Collider2D>();

        // Set initial state
        if (_startVisible)
        {
            _spriteRenderer.color = _visibleColor;
            _platformCollider.enabled = _solidWhenVisible;
        }
        else
        {
            _spriteRenderer.color = _hiddenColor;
            _platformCollider.enabled = false;
        }
    }

    public void OnIlluminated(LanternController lantern)
    {
        if (IsIlluminated) return;

        IsIlluminated = true;
        RevealPlatform();
    }

    public void OnLeftLight(LanternController lantern)
    {
        if (!IsIlluminated) return;

        IsIlluminated = false;
        HidePlatform();
    }

    private void RevealPlatform()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeToColor(_visibleColor));

        if (_solidWhenVisible)
            _platformCollider.enabled = true;
    }

    private void HidePlatform()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);

        _fadeCoroutine = StartCoroutine(FadeToColor(_hiddenColor));

        if (_solidWhenVisible)
            _platformCollider.enabled = false;
    }

    private IEnumerator FadeToColor(Color targetColor)
    {
        Color startColor = _spriteRenderer.color;
        float elapsedTime = 0f;

        while (elapsedTime < 1f / _fadeSpeed)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime * _fadeSpeed;
            _spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        _spriteRenderer.color = targetColor;
    }
}