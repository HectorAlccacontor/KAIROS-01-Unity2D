using UnityEngine;

/// <summary>
/// Control principal de Kiro.
/// Input.GetAxisRaw proporciona respuesta horizontal inmediata.
/// El salto solo se ejecuta cuando isGrounded es verdadero.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float moveSpeed = 4.4f;
    [SerializeField] private float jumpForce = 11.5f;

    [Header("Detección de suelo")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckDistance = 0.08f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private float horizontalInput;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // -1, 0 o 1. No hay aceleración flotante.
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Mientras asciende no se habilita otro salto, aunque Kiro esté
        // atravesando una plataforma semisólida.
        isGrounded =
            rb.linearVelocity.y <= 0.05f &&
            CheckGrounded();

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            horizontalInput * moveSpeed,
            rb.linearVelocity.y
        );
    }

    /// <summary>
    /// Tres rayos cortos salen exclusivamente desde la zona de los pies.
    /// Así una mano, cabeza o lateral rozando una plataforma no cuenta
    /// falsamente como "suelo".
    /// </summary>
    private bool CheckGrounded()
    {
        if (bodyCollider == null)
            return false;

        Bounds b = bodyCollider.bounds;

        float y = groundCheck != null
            ? groundCheck.position.y
            : b.min.y + 0.04f;

        float inset = b.extents.x * 0.52f;

        Vector2 center = new Vector2(b.center.x, y);
        Vector2 left   = new Vector2(b.center.x - inset, y);
        Vector2 right  = new Vector2(b.center.x + inset, y);

        return GroundRay(left) || GroundRay(center) || GroundRay(right);
    }

    private bool GroundRay(Vector2 origin)
    {
        RaycastHit2D hit = Physics2D.Raycast(
            origin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Si Kiro golpea un techo sólido se elimina solo la componente
        // ascendente; nunca rebota ni pierde el control horizontal.
        if (rb.linearVelocity.y <= 0f)
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y < -0.55f)
            {
                rb.linearVelocity =
                    new Vector2(rb.linearVelocity.x, 0f);
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D col = bodyCollider != null
            ? bodyCollider
            : GetComponent<Collider2D>();

        if (col == null)
            return;

        Bounds b = col.bounds;

        float y = groundCheck != null
            ? groundCheck.position.y
            : b.min.y + 0.04f;

        float inset = b.extents.x * 0.52f;

        Gizmos.color = Color.yellow;

        Vector3 a = new Vector3(b.center.x - inset, y, 0f);
        Vector3 c = new Vector3(b.center.x, y, 0f);
        Vector3 d = new Vector3(b.center.x + inset, y, 0f);

        Gizmos.DrawLine(a, a + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(c, c + Vector3.down * groundCheckDistance);
        Gizmos.DrawLine(d, d + Vector3.down * groundCheckDistance);
    }
}

