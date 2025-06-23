using UnityEngine;
using System.Collections;

/// <summary>
/// Visual effect that plays when a lantern is collected
/// Automatically destroys itself after playing
/// </summary>
public class LanternPickupEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    [SerializeField] private ParticleSystem[] _particleSystems;
    [SerializeField] private float _effectDuration = 2f;
    [SerializeField] private AnimationCurve _scaleCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private Color _glowColor = Color.yellow;

    private void Start()
    {
        PlayEffect();

        // Auto-destroy after duration
        Destroy(gameObject, _effectDuration);
    }

    private void PlayEffect()
    {
        // Start particle systems
        foreach (var ps in _particleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = _glowColor;
                ps.Play();
            }
        }

        // Start scaling animation
        StartCoroutine(ScaleAnimation());
    }

    private IEnumerator ScaleAnimation()
    {
        Vector3 startScale = Vector3.zero;
        Vector3 targetScale = Vector3.one;
        float elapsedTime = 0f;

        while (elapsedTime < _effectDuration * 0.5f) // Scale up for first half
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (_effectDuration * 0.5f);
            float curveValue = _scaleCurve.Evaluate(t);

            transform.localScale = Vector3.Lerp(startScale, targetScale, curveValue);
            yield return null;
        }

        // Scale down for second half
        elapsedTime = 0f;
        while (elapsedTime < _effectDuration * 0.5f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / (_effectDuration * 0.5f);
            float curveValue = _scaleCurve.Evaluate(1f - t);

            transform.localScale = Vector3.Lerp(Vector3.zero, targetScale, curveValue);
            yield return null;
        }
    }
}