using UnityEngine;

/// <summary>
/// Controla exclusivamente la apariencia de Kiro.
/// La física sigue viviendo en PlayerController/Rigidbody2D.
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
        bool pushing = grounded && Mathf.Abs(input) > 0.05f && IsPushingCrate(input);

        Sprite next;

        if (!grounded)
        {
            next = rb.linearVelocity.y > 0.1f
                ? (facingRight ? jumpRight : jumpLeft)
                : (facingRight ? fallRight : fallLeft);
        }
        else if (pushing)
        {
            next = facingRight ? pushRight : pushLeft;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.15f)
        {
            bool frame2 = Mathf.FloorToInt(Time.time * runFramesPerSecond) % 2 == 1;
            if (facingRight)
                next = frame2 ? runRight2 : runRight1;
            else
                next = frame2 ? runLeft2 : runLeft1;
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
        float direction = Mathf.Sign(input);
        Vector2 center = (Vector2)transform.position + Vector2.right * direction * 0.48f;
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, new Vector2(0.35f, 0.80f), 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == null || hit == ownCollider || hit.transform.root == transform.root)
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
