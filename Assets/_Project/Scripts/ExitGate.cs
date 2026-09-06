using UnityEngine;

/// <summary>
/// Puerta final: sólida y cerrada antes del núcleo;
/// verde/abierta y atravesable después de recogerlo.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class ExitGate : MonoBehaviour
{
    [SerializeField] private SpriteRenderer visualRenderer;
    [SerializeField] private Sprite lockedSprite;
    [SerializeField] private Sprite unlockedSprite;

    private Collider2D gateCollider;
    private bool unlocked;

    private void Awake()
    {
        gateCollider = GetComponent<Collider2D>();

        if (visualRenderer == null)
            visualRenderer = GetComponentInChildren<SpriteRenderer>();

        ApplyState(false);
    }

    private void Update()
    {
        bool ready = GameManager.Instance != null && GameManager.Instance.HasCore;

        if (ready != unlocked)
            ApplyState(ready);
    }

    private void ApplyState(bool open)
    {
        unlocked = open;

        if (gateCollider != null)
            gateCollider.isTrigger = unlocked;

        if (visualRenderer != null)
        {
            visualRenderer.color = Color.white;
            visualRenderer.sprite = unlocked ? unlockedSprite : lockedSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (unlocked && other.CompareTag("Player"))
            GameManager.Instance.CompleteTest();
    }
}
