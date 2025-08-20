using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Gun : MonoBehaviour
{
    public enum FireMouseButton { Left, Right }   // <-- scelta tasto

    [Header("Input")]
    public FireMouseButton fireMouseButton = FireMouseButton.Left;
    public bool singleShot = false;  // se true, non puoi tenere premuto per sparare

    public Camera cam;
    public Transform muzzle;
    public float fireRate = 10f;
    public float range = 500f;
    public int damage = 10;
    public LayerMask hitMask = ~0;
    public GameObject impactFx; // metti un prefab semplice
    
    [Header("Bullet Holes")]
    public GameObject bulletHolePrefab;   // prefab con plane/quad + materiale bullet hole
    public float bulletHoleSize = 0.12f;  // grandezza in metri
    public float bulletHoleLife = 12f;
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
        var mouse = Mouse.current;
        bool firePressed = false;
        bool fireJustPressed = false;

        if (mouse != null)
        {
            if (fireMouseButton == FireMouseButton.Left)
            {
                firePressed = mouse.leftButton.isPressed;
                fireJustPressed = mouse.leftButton.wasPressedThisFrame;
            }
            else
            {
                firePressed = mouse.rightButton.isPressed;
                fireJustPressed = mouse.rightButton.wasPressedThisFrame;
            }
        }

        bool shouldShoot = singleShot ? fireJustPressed : firePressed;
        
        if (shouldShoot && (singleShot || Time.time >= nextShot))
        {
            if (!singleShot) nextShot = Time.time + 1f / fireRate;
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

            Debug.Log($"HIT: {hit.collider.name}  hasDamageable:{hit.collider.GetComponentInParent<IDamageable>()!=null}");

            var d = hit.collider.GetComponentInParent<IDamageable>();
            if (d != null) d.TakeDamage(damage);

            var piece = hit.collider.GetComponentInParent<BreakablePiece>();
            if (piece != null)
            {
                var rb = piece.GetComponent<Rigidbody>();
                if (rb) rb.AddForceAtPosition(cam.transform.forward * bulletImpulse, hit.point, ForceMode.Impulse);
            }

            if (hit.rigidbody && (!onlyPushPushables || hit.rigidbody.GetComponentInParent<Pushable>()))
            {
                float mult = 1f;
                var p = hit.rigidbody.GetComponentInParent<Pushable>();
                if (p) mult = p.impulseMultiplier;
                hit.rigidbody.AddForceAtPosition(cam.transform.forward * bulletImpulse * mult, hit.point, ForceMode.Impulse);
            }

            if (impactFx)
            {
                var fx = Instantiate(impactFx, hit.point, Quaternion.LookRotation(hit.normal));
                foreach (var ps in fx.GetComponentsInChildren<ParticleSystem>())
                    ps.Play();
                Destroy(fx, 2f);
            }

            PlaceBulletHole(hit);
            Debug.Log($"[Gun] Hit: {hit.collider.name} @ {hit.distance:0.0}m");
        }
        else
        {
            Debug.Log("[Gun] Miss");
        }

        Debug.DrawLine(muzzle ? muzzle.position : cam.transform.position, endPoint, Color.white, 0.1f);

        muzzleFx?.Play();

        if (muzzleSmoke)
        {
            muzzleSmoke.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            muzzleSmoke.Play(true);
        }

        if (tracerPrefab)
            StartCoroutine(DoTracer(muzzle ? muzzle.position : cam.transform.position, endPoint));

        if (fireSounds != null && fireSounds.Length > 0 && audioSource != null)
        {
            int idx = Random.Range(0, fireSounds.Length);
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(fireSounds[idx]);
        }

        GetComponent<GunRecoil>()?.DoRecoil();
    }

    System.Collections.IEnumerator DoTracer(Vector3 start, Vector3 end)
    {
        var lr = Instantiate(tracerPrefab);
        lr.useWorldSpace = true;
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        yield return new WaitForSeconds(0.02f);
        if (lr) Destroy(lr.gameObject);
    }

    void PlaceBulletHole(RaycastHit hit)
    {
        if (!bulletHolePrefab) return;
        if (hit.rigidbody && hit.rigidbody.GetComponentInParent<Pushable>() != null) return;

        var hole = Instantiate(
            bulletHolePrefab,
            hit.point + hit.normal * 0.001f,
            Quaternion.LookRotation(-hit.normal)
        );

        hole.transform.localScale = Vector3.one * bulletHoleSize;
        hole.transform.Rotate(0f, 0f, Random.Range(0f, 360f), Space.Self);
        Destroy(hole, bulletHoleLife);
    }
}
