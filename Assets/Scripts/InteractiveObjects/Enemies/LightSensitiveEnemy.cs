using UnityEngine;
using System.Collections;

/// <summary>
/// An enemy that reacts to light by retreating or freezing
/// Perfect for your "enemy repulsion" mechanic
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class LightSensitiveEnemy : MonoBehaviour, ILightInteractable
{
    [Header("Light Reaction")]
    [SerializeField] private LightReactionType _reactionType = LightReactionType.Retreat;
    [SerializeField] private float _retreatSpeed = 5f;
    [SerializeField] private float _normalSpeed = 2f;
    [SerializeField] private float _freezeDuration = 1f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform[] _patrolPoints;
    [SerializeField] private bool _patrolWhenNotIlluminated = true;

    public bool IsIlluminated { get; private set; }

    public enum LightReactionType
    {
        Retreat,    // Moves away from light source
        Freeze,     // Stops moving when illuminated
        Dissolve    // Disappears temporarily when illuminated
    }

    private Rigidbody2D _rb;
    private Vector2 _retreatDirection;
    private int _currentPatrolIndex = 0;
    private bool _isFrozen = false;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _freezeCoroutine;
    private Coroutine _dissolveCoroutine;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!IsIlluminated && !_isFrozen && _patrolWhenNotIlluminated)
        {
            Patrol();
        }
    }

    public void OnIlluminated(LanternController lantern)
    {
        if (IsIlluminated) return;

        IsIlluminated = true;

        // Calculate retreat direction (away from lantern)
        _retreatDirection = ((Vector2)transform.position - (Vector2)lantern.transform.position).normalized;

        switch (_reactionType)
        {
            case LightReactionType.Retreat:
                StartRetreating();
                break;
            case LightReactionType.Freeze:
                StartFreezing();
                break;
            case LightReactionType.Dissolve:
                StartDissolving();
                break;
        }
    }

    public void OnLeftLight(LanternController lantern)
    {
        if (!IsIlluminated) return;

        IsIlluminated = false;
        StopReacting();
    }

    private void StartRetreating()
    {
        _rb.linearVelocity = _retreatDirection * _retreatSpeed;
    }

    private void StartFreezing()
    {
        if (_freezeCoroutine != null)
            StopCoroutine(_freezeCoroutine);

        _freezeCoroutine = StartCoroutine(FreezeRoutine());
    }

    private void StartDissolving()
    {
        if (_dissolveCoroutine != null)
            StopCoroutine(_dissolveCoroutine);

        _dissolveCoroutine = StartCoroutine(DissolveRoutine());
    }

    private void StopReacting()
    {
        _rb.linearVelocity = Vector2.zero;
        _isFrozen = false;

        if (_freezeCoroutine != null)
        {
            StopCoroutine(_freezeCoroutine);
            _freezeCoroutine = null;
        }

        if (_dissolveCoroutine != null)
        {
            StopCoroutine(_dissolveCoroutine);
            _dissolveCoroutine = null;
            _spriteRenderer.color = Color.white;
        }
    }

    private IEnumerator FreezeRoutine()
    {
        _isFrozen = true;
        _rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(_freezeDuration);

        _isFrozen = false;
    }

    private IEnumerator DissolveRoutine()
    {
        // Fade out
        Color startColor = _spriteRenderer.color;
        Color transparentColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float fadeTime = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeTime;
            _spriteRenderer.color = Color.Lerp(startColor, transparentColor, t);
            yield return null;
        }

        _spriteRenderer.color = transparentColor;
    }

    private void Patrol()
    {
        if (_patrolPoints == null || _patrolPoints.Length == 0) return;

        Transform targetPoint = _patrolPoints[_currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        _rb.linearVelocity = direction * _normalSpeed;

        // Check if we're close enough to the target point
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.5f)
        {
            _currentPatrolIndex = (_currentPatrolIndex + 1) % _patrolPoints.Length;
        }
    }
}