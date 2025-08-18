using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Explosion Settings")]
    public GameObject explosionFx; // assegna un prefab di esplosione
    public bool destroyOnDeath = true;
    public float destroyDelay = 2f;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage. HP = {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (explosionFx)
        {
            var fx = Instantiate(explosionFx, transform.position + Vector3.up * 0.05f, Quaternion.identity);

            // layer sicuro (Default) su tutta la gerarchia
            SetLayerRecursively(fx, LayerMask.NameToLayer("Default"));

            // forza PLAY su tutti i ParticleSystem (anche se PlayOnAwake è OFF)
            var pss = fx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss) { ps.Clear(true); ps.Play(true); }

            // fallback auto-destroy se il prefab non usa Stop Action = Destroy
            Destroy(fx, 5f);
        }

        // distruggi l'oggetto colpito (dopo aver istanziato l'FX)
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform) SetLayerRecursively(t.gameObject, layer);
    }
}
