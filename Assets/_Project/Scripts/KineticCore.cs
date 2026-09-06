using UnityEngine;

/// <summary>
/// Núcleo coleccionable. Detecta al jugador por su PlayerController,
/// no depende únicamente del Tag "Player".
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KineticCore : MonoBehaviour
{
    private Vector3 startPosition;
    private bool collected;

    private void Start()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (collected) return;

        transform.position = startPosition +
            Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.12f);

        transform.Rotate(0f, 0f, 75f * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollect(other);
    }

    private void TryCollect(Collider2D other)
    {
        if (collected) return;

        PlayerController player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.CollectCore();

        gameObject.SetActive(false);
    }
}
