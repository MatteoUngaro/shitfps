using UnityEngine;
using UnityEngine.InputSystem;

public class GrenadeThrower : MonoBehaviour
{
    public Camera cam;
    public Transform throwPoint;     // opzionale; se nullo usa la camera
    public GameObject grenadePrefab; // prefab con script Grenade
    public float throwForce = 12f;
    public float cooldown = 0.8f;
    public float spawnForward = 0.6f; // offset in avanti
    public float spawnRight   = 0.2f; // leggero offset a destra
    public float spawnUp      = -0.1f;// un filo sotto la camera
    public float arcUp        = 3f;   // spinta verso l'alto

    float nextTime;

    void Awake() { if (!cam) cam = Camera.main; if (!throwPoint) throwPoint = cam.transform; }

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame && Time.time >= nextTime)
        {
            nextTime = Time.time + cooldown;
            Throw();
        }
    }

    void Throw()
    {
        var t = throwPoint ? throwPoint : cam.transform;
        Vector3 fwd = t.forward, up = t.up, right = t.right;

        // spawn un po' davanti/lato per non collidere col player
        Vector3 spawnPos = t.position + fwd * spawnForward + right * spawnRight + up * spawnUp;

        var go = Instantiate(grenadePrefab, spawnPos, Quaternion.LookRotation(fwd));
        var rb = go.GetComponent<Rigidbody>();
        var gcol = go.GetComponent<Collider>();

        // ignora le collisioni col player per 0.2s
        foreach (var pc in GetComponentsInParent<Collider>())
            if (pc && gcol) Physics.IgnoreCollision(gcol, pc, true);
        StartCoroutine(ReenablePlayerCollisions(gcol, 0.2f));

        // velocità iniziale + arco
        if (rb)
            rb.linearVelocity = fwd * throwForce + up * arcUp; // lancia davvero
    }

    System.Collections.IEnumerator ReenablePlayerCollisions(Collider gcol, float delay)
    {
        yield return new WaitForSeconds(delay);
        foreach (var pc in GetComponentsInParent<Collider>())
            if (pc && gcol) Physics.IgnoreCollision(gcol, pc, false);
    }
}
