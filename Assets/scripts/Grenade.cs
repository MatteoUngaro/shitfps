using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float fuse = 1.2f;
    public float radius = 2f;       // raggio richiesto
    public int damage = 100;        // danno richiesto
    public float explosionForce = 700f;
    public float upwardModifier = 0.5f;
    public LayerMask damageMask = ~0;   // cosa colpire
    public GameObject explosionFx;      // opzionale

    [Header("Audio")]
    public AudioClip explosionSfx;   // assegna un .wav/.mp3
    [Range(0f, 1f)] public float sfxVolume = 1f; // altissimo = 1
    public float sfxPitch = 1f;
    public float sfxMinDistance = 12f; // pieno volume fino a 12m
    public float sfxMaxDistance = 80f;

    bool exploded;

    void OnEnable() { StartCoroutine(Fuse()); }

    IEnumerator Fuse()
    {
        yield return new WaitForSeconds(fuse);
        Explode();
    }

    void Explode()
    {
        if (exploded) return; 
        exploded = true;

        // --- FX come in Health.cs ---
        if (explosionFx)
        {
            var fx = Instantiate(explosionFx, transform.position + Vector3.up * 0.05f, Quaternion.identity);

            // layer "sicuro" su tutta la gerarchia
            SetLayerRecursively(fx, LayerMask.NameToLayer("Default"));

            // forza PLAY su tutti i particle (anche se PlayOnAwake è OFF)
            var pss = fx.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in pss) { ps.Clear(true); ps.Play(true); }

            // auto-destroy di backup (se il prefab non si autodistrugge da solo)
            Destroy(fx, 5f);
        }

        // --- danno + spinta ---
        var cols = Physics.OverlapSphere(transform.position, radius, damageMask, QueryTriggerInteraction.Ignore);
        var hitRoots = new HashSet<Transform>();
        foreach (var c in cols)
        {
            var dmg = c.GetComponentInParent<IDamageable>();
            if (dmg != null)
            {
                var root = c.GetComponentInParent<Transform>();
                if (hitRoots.Add(root)) dmg.TakeDamage(damage);
            }
            if (c.attachedRigidbody)
                c.attachedRigidbody.AddExplosionForce(explosionForce, transform.position, radius, upwardModifier, ForceMode.Impulse);
        }

        // --- SOUND SUPER FORTE ---
        if (explosionSfx)
        {
            var go = new GameObject("ExplosionSFX");
            go.transform.position = transform.position;
            var src = go.AddComponent<AudioSource>();
            src.clip = explosionSfx;
            src.spatialBlend = 1f;          // 3D
            src.volume = sfxVolume;         // 1 = altissimo
            src.pitch = sfxPitch;
            src.minDistance = sfxMinDistance;
            src.maxDistance = sfxMaxDistance;
            src.Play();
            Destroy(go, explosionSfx.length / Mathf.Max(0.01f, sfxPitch) + 0.1f);
        }

        // (se usi il suono: lascialo su un GO separato, NON figlio della granata)
        Destroy(gameObject);
    }

    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }

    void OnDrawGizmosSelected() { Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, radius); }
}
