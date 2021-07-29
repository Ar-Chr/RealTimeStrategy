using Mirror;
using UnityEngine;

public class Projectile : NetworkBehaviour
{
    [SerializeField] private float lifetime;
    [SerializeField] private new Rigidbody rigidbody;

    public int Damage { get; set; }
    public float MuzzleVelocity { get; set; }

    private Vector3 lastPosition;

    private void Start()
    {
        rigidbody.velocity = transform.forward * MuzzleVelocity;
        lastPosition = transform.position;
    }

    [ServerCallback]
    private void FixedUpdate()
    {
        Vector3 direction = transform.position - lastPosition;
        Debug.DrawLine(lastPosition, transform.position, Color.red, 1.0f);

        Ray ray = new Ray(lastPosition, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, direction.magnitude))
            HandleCollided(hit.collider);

        lastPosition = transform.position;
    }

    public override void OnStartServer()
    {
        Invoke(nameof(DestroySelf), lifetime);
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        HandleCollided(other);
    }

    public void HandleCollided(Collider other)
    {
        if (!other.TryGetComponent(out NetworkIdentity identity))
            return;

        if (identity.connectionToClient == connectionToClient)
            return;

        if (!other.TryGetComponent(out Health health))
            return;

        health.TakeDamage(Damage);
        DestroySelf();
    }

    [Server]
    private void DestroySelf()
    {
        NetworkServer.Destroy(gameObject);
    }
}
