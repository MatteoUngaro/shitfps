using UnityEngine;

public class ExplosionFx : MonoBehaviour
{
    [Header("Physics")]
    public float radius = 4f;
    public float force = 600f;
    public float upwardsModifier = 0.5f;
    public LayerMask affectMask = ~0; // o solo Default

    [Header("Light Flash (optional)")]
    public Light flashLight;      // assegna la Point Light se la usi
    public float flashTime = 0.08f;

    void Start()
    {
        // Spinta fisica
        var cols = Physics.OverlapSphere(transform.position, radius, affectMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var rb = c.attachedRigidbody;
            if (!rb) continue;
            rb.AddExplosionForce(force, transform.position, radius, upwardsModifier, ForceMode.Impulse);
        }

        // Flash veloce
        if (flashLight) StartCoroutine(FadeFlash());
    }

    System.Collections.IEnumerator FadeFlash()
    {
        float t = flashTime;
        float start = flashLight.intensity;
        while (t > 0f)
        {
            t -= Time.deltaTime;
            flashLight.intensity = Mathf.Lerp(0f, start, t / flashTime);
            yield return null;
        }
        flashLight.enabled = false;
    }
}

