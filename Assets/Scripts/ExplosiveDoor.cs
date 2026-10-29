using UnityEngine;

public class ExplosiveDoor : MonoBehaviour
{
    [Header("Efectos Cómicos")]
    public float explosionForce = 500f;
    public GameObject explosionFX;

    private void OnCollisionEnter(Collision collision)
    {
        // Si impacta la tortuga
        if (collision.gameObject.CompareTag("Turtle"))
        {
            ExplodeDoor();
        }
    }

    void ExplodeDoor()
    {
        if (explosionFX) Instantiate(explosionFX, transform.position, Quaternion.identity);

        // Añadir Rigidbody si no tiene para hacer salir volando la puerta
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();

        Vector3 randomDirection = (Vector3.up + Random.onUnitSphere).normalized;
        rb.AddForce(randomDirection * explosionForce, ForceMode.Impulse);

        // Desactivar collider para permitir el paso
        GetComponent<Collider>().enabled = false;

        Destroy(gameObject, 3.0f); // Limpieza pasados unos segundos
    }
}