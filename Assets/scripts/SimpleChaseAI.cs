using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class SimpleChaseAI : MonoBehaviour
{
    public Transform target;
    public float repathInterval = 0.1f;
    // distanza a cui vuoi che si fermi davanti a te
    public float frontOffset = 1.0f;   // esponilo nel tuo script

    NavMeshAgent agent;
    Animator anim;
    float repathTimer;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
        // il NavMeshAgent guida il movimento, non la root motion
        if (anim) anim.applyRootMotion = false;
    }

    void Update()
    {
        if (!target) return;

        // punto davanti al player (sulla sua forward)
        Vector3 front = target.position + target.forward * frontOffset;

        // snappa al NavMesh
        if (NavMesh.SamplePosition(front, out var hitNM, 1.0f, NavMesh.AllAreas))
            agent.SetDestination(hitNM.position);
        else
            agent.SetDestination(target.position);

        // ruota verso il player quando vicino
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation((target.position - transform.position).normalized, Vector3.up),
                                                  Time.deltaTime * 10f);

        // animator (come già fatto)
        float speed = agent.velocity.magnitude;
        if (anim) { anim.applyRootMotion = false; anim.SetFloat("Speed", speed, 0.1f, Time.deltaTime); }
    }
}
