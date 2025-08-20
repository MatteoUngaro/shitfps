using UnityEngine;

public class BreakablePiece : MonoBehaviour, IDamageable
{
    public int maxHealth = 5;
    public float impulse = 6f;
    public LimbType limb = LimbType.None;

    int hp; bool broken;
    SkinnedMeshRenderer smr;
    Collider hitCol;
    RobotDamageHub hub;

    void Awake()
    {
        hp = maxHealth;
        smr = GetComponent<SkinnedMeshRenderer>();
        hitCol = GetComponent<Collider>() ?? gameObject.AddComponent<BoxCollider>();
        if (smr) smr.updateWhenOffscreen = true; // evita culling
        hub = GetComponentInParent<RobotDamageHub>();
    }

    public void TakeDamage(int amount)
    {
        if (broken) return;
        hp -= amount;
        if (hp <= 0) BreakOff();
    }

    void BreakOff()
    {
        if (broken || !smr) return;
        broken = true;

        // snapshot mondo
        Vector3 wPos = transform.position;
        Quaternion wRot = transform.rotation;

        // bake della mesh (world space)
        var baked = new Mesh();
        smr.BakeMesh(baked);

        var mats = smr.sharedMaterials;
        int layer = gameObject.layer;
        string tag = gameObject.tag;

        // crea nuovo oggetto rigido
        var go = new GameObject(name + "_DETACHED");
        go.layer = layer; go.tag = tag;
        go.transform.position = wPos;
        go.transform.rotation = wRot;
        go.transform.localScale = Vector3.one;   // 👈 forza scala a 1

        var mf = go.AddComponent<MeshFilter>();   mf.sharedMesh = baked;
        var mr = go.AddComponent<MeshRenderer>(); mr.sharedMaterials = mats;

        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = baked; mc.convex = true;

        var rb = go.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // spinta iniziale
        var dir = Camera.main ? Camera.main.transform.forward : Vector3.up;
        rb.AddForce(dir * impulse, ForceMode.Impulse);

        // avvisa il root che questo arto è perso
        if (hub) hub.OnLimbDetached(limb);

        // disattiva l'originale
        smr.enabled = false;
        if (hitCol) hitCol.enabled = false;
    }
}
