using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Bullet : MonoBehaviour
{
    public float life = 3f;
    public int damage = 10;
    Rigidbody rb;

    void Awake() { rb = GetComponent<Rigidbody>(); }

    public void Fire(Vector3 pos, Vector3 dir, float speed)
    {
        transform.SetPositionAndRotation(pos, Quaternion.LookRotation(dir));
        rb.linearVelocity = dir * speed;
        CancelInvoke();
        Invoke(nameof(Despawn), life);   // life = 2–3s
    }

    void OnCollisionEnter(Collision c)
    {
        var d = c.collider.GetComponent<IDamageable>();
        if (d != null) d.TakeDamage(damage);
        Despawn();
    }

    void Despawn()
    {
        Destroy(gameObject); // <-- invece di SetActive(false)
    }
}
