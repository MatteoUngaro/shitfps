using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Gun : MonoBehaviour
{
    public Camera cam;
    public Transform muzzle;
    public float fireRate = 10f;
    public float range = 500f;
    public int damage = 10;
    public LayerMask hitMask = ~0;
    public GameObject impactFx; // metti un prefab semplice
    public DecalProjector bulletDecalPrefab;
    public float bulletDecalSize = 0.12f; // 12 cm
    public float bulletDecalLife = 12f;
    public LineRenderer tracerPrefab;
    public ParticleSystem muzzleFx; // assegna MuzzleFlash
    public ParticleSystem muzzleSmoke;
    public float bulletImpulse = 8f; // spinta per colpo
    public bool onlyPushPushables = true;

    [Header("Audio")]
    public AudioSource audioSource;   // aggiungi un AudioSource sull'arma
    public AudioClip[] fireSounds;    // inserisci qui i 4 suoni
    
    [Header("Audio Pitch Variation")]
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    float nextShot;

    void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    void Update()
    {
        if (Mouse.current.leftButton.isPressed && Time.time >= nextShot)
        {
            nextShot = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        if (!cam) { Debug.LogWarning("[Gun] Camera mancante"); return; }

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        Vector3 endPoint = cam.transform.position + cam.transform.forward * range;

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask, QueryTriggerInteraction.Ignore))
        {
            endPoint = hit.point;

            var d = hit.collider.GetComponentInParent<IDamageable>();
            if (d != null) d.TakeDamage(damage); // :contentReference[oaicite:1]{index=1}

            // KNOCKBACK / PUSH
            if (hit.rigidbody && (!onlyPushPushables || hit.rigidbody.GetComponentInParent<Pushable>()))
            {
                float mult = 1f;
                var p = hit.rigidbody.GetComponentInParent<Pushable>();
                if (p) mult = p.impulseMultiplier;

                // impulso nella direzione di tiro, applicato nel punto d'impatto (così prende anche coppia)
                hit.rigidbody.AddForceAtPosition(cam.transform.forward * bulletImpulse * mult, hit.point, ForceMode.Impulse);
            }

            if (impactFx)
            {
                var fx = Instantiate(impactFx, hit.point, Quaternion.LookRotation(hit.normal));
                foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
                    ps.Play();

                Destroy(fx, 2f); // o fx.GetComponent<ParticleSystem>().main.duration
            }

            PlaceBulletDecal(hit);

            Debug.Log($"[Gun] Hit: {hit.collider.name} @ {hit.distance:0.0}m");
        }
        else
        {
            Debug.Log("[Gun] Miss");
        }

        // linea visiva nella Scene/Game per capire dove ha sparato
        Debug.DrawLine(muzzle ? muzzle.position : cam.transform.position, endPoint, Color.white, 0.1f);

        // Play muzzle flash effect
        muzzleFx?.Play();

        // Play muzzle smoke
        if (muzzleSmoke)
        {
            muzzleSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleSmoke.Play(true);
        }

        if (tracerPrefab)
            StartCoroutine(DoTracer(muzzle ? muzzle.position : cam.transform.position, endPoint));

        // 🎵 Play random gunshot sound
        if (fireSounds != null && fireSounds.Length > 0 && audioSource != null)
        {
            int idx = Random.Range(0, fireSounds.Length);

            audioSource.pitch = Random.Range(minPitch, maxPitch); // variazione del pitch ±10%
            audioSource.PlayOneShot(fireSounds[idx]);
        }

        // Trigger visual recoil
        GetComponent<GunRecoil>()?.DoRecoil();
    }

    System.Collections.IEnumerator DoTracer(Vector3 start, Vector3 end)
    {
        var lr = Instantiate(tracerPrefab);
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        yield return new WaitForSeconds(0.02f); // Reduced from 0.05f to 0.02f for faster tracer
        if (lr) Destroy(lr.gameObject);
    }

    void PlaceBulletDecal(RaycastHit hit)
    {
        if (!bulletDecalPrefab) return;

        Transform parent = hit.rigidbody ? hit.rigidbody.transform : hit.collider.transform;

        var decal = Instantiate(bulletDecalPrefab, parent);
        decal.transform.SetPositionAndRotation(hit.point + hit.normal * 0.001f,
                                               Quaternion.LookRotation(-hit.normal));

        // misura del proiettore (X/Y = lato decal, Z = profondità di proiezione)
        decal.size = new Vector3(bulletDecalSize, bulletDecalSize, 0.1f); // Z più grande per evitare clipping

        // layer sicuro (visibile alla WorldCam)
        decal.gameObject.layer = LayerMask.NameToLayer("Default");

        // se hai Decal Layers attive in URP, sblocca tutto:
#if UNITY_URP_DECAL // simbolico: se non compila, togli l'#if
        decal.decalLayerMask = uint.MaxValue; // "Everything"
#endif

        decal.transform.Rotate(0f, 0f, Random.Range(0f, 360f), Space.Self);

        Destroy(decal.gameObject, bulletDecalLife);

        Debug.Log($"Decal spawned on {hit.collider.name} at {hit.point}");
    }
}
