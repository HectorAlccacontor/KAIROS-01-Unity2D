using UnityEngine;

/// <summary>
/// Control visual de Kiro. La animación nunca modifica la física.
/// </summary>
[RequireComponent(typeof(PlayerController), typeof(Rigidbody2D))]
public class KiroVisualController : MonoBehaviour
{
    [Header("Renderer")]
    [SerializeField] private SpriteRenderer visualRenderer;

    [Header("Derecha")]
    [SerializeField] private Sprite idleRight;
    [SerializeField] private Sprite runRight1;
    [SerializeField] private Sprite runRight2;
    [SerializeField] private Sprite jumpRight;
    [SerializeField] private Sprite fallRight;
    [SerializeField] private Sprite pushRight;

    [Header("Izquierda")]
    [SerializeField] private Sprite idleLeft;
    [SerializeField] private Sprite runLeft1;
    [SerializeField] private Sprite runLeft2;
    [SerializeField] private Sprite jumpLeft;
    [SerializeField] private Sprite fallLeft;
    [SerializeField] private Sprite pushLeft;

    [Header("Animación")]
    [SerializeField] private float runFramesPerSecond = 10f;

    private Rigidbody2D rb;
    private PlayerController controller;
    private Collider2D ownCollider;
    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        ownCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");

        if (input > 0.05f) facingRight = true;
        else if (input < -0.05f) facingRight = false;

        bool grounded = controller != null && controller.IsGrounded;
        bool pushing =
            grounded &&
            Mathf.Abs(input) > 0.05f &&
            IsPushingCrate(input);

        Sprite next;

        if (!grounded)
        {
            next = rb.linearVelocity.y > 0.08f
                ? (facingRight ? jumpRight : jumpLeft)
                : (facingRight ? fallRight : fallLeft);
        }
        else if (pushing)
        {
            next = facingRight ? pushRight : pushLeft;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.12f)
        {
            bool frame2 =
                Mathf.FloorToInt(Time.time * runFramesPerSecond) % 2 == 1;

            next = facingRight
                ? (frame2 ? runRight2 : runRight1)
                : (frame2 ? runLeft2 : runLeft1);
        }
        else
        {
            next = facingRight ? idleRight : idleLeft;
        }

        if (visualRenderer != null && next != null)
            visualRenderer.sprite = next;
    }

    private bool IsPushingCrate(float input)
    {
        if (ownCollider == null)
            return false;

        Bounds b = ownCollider.bounds;
        float direction = Mathf.Sign(input);

        Vector2 center = new Vector2(
            b.center.x + direction * (b.extents.x + 0.16f),
            b.center.y
        );

        Vector2 size = new Vector2(
            0.30f,
            b.size.y * 0.72f
        );

        Collider2D[] hits =
            Physics2D.OverlapBoxAll(center, size, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == ownCollider)
                continue;

            Rigidbody2D hitRb = hit.attachedRigidbody;

            if (hitRb != null &&
                hitRb.bodyType == RigidbodyType2D.Dynamic &&
                hit.gameObject.name.Contains("Crate"))
            {
                return true;
            }
        }

        return false;
    }
}
