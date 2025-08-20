using UnityEngine;
using UnityEngine.AI;

public class RobotDamageHub : MonoBehaviour
{
    [Header("Knockdown rules")]
    public bool fallOnFirstLegLost = true;   // cade alla prima gamba persa
    private int legsRemaining = 2;
    private bool isDown;

    public void OnLimbDetached(LimbType limb)
    {
        if (isDown) return;

        if (limb == LimbType.LeftLeg || limb == LimbType.RightLeg)
        {
            legsRemaining--;
            if (fallOnFirstLegLost || legsRemaining <= 1) KnockDown();
        }
    }

    void KnockDown()
    {
        if (isDown) return;
        isDown = true;

        // prendi riferimenti PRIMA di disabilitare
        var agent = GetComponent<NavMeshAgent>();
        float h = agent ? agent.height : 2.0f;
        float r = agent ? agent.radius : 0.4f;

        // spegni AI + pathfinding + animazioni
        var ai = GetComponent<SimpleChaseAI>(); if (ai) ai.enabled = false;
        if (agent) agent.enabled = false;
        var anim = GetComponent<Animator>();    if (anim) anim.enabled = false;

        // collider + rigidbody sul root per farlo cadere
        var col = GetComponent<Collider>() as CapsuleCollider;
        if (!col) col = gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, h * 0.5f, 0f);
        col.height = Mathf.Max(1.0f, h);
        col.radius = Mathf.Max(0.2f, r);
        col.direction = 1; // Y

        var rb = GetComponent<Rigidbody>();
        if (!rb) rb = gameObject.AddComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;

        // piccola spinta/torque per farlo sbilanciare
        rb.AddForce((transform.forward + Vector3.up) * 3f, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 6f, ForceMode.Impulse);
    }
}
