using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFootsteps : MonoBehaviour
{
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f;     // intervallo medio camminando
    public float runStepMultiplier = 0.7f; // più rapido quando va veloce
    public float minSpeed = 0.1f;          // soglia per considerarlo fermo
    public float pitchMin = 0.95f, pitchMax = 1.05f;
    public LayerMask groundMask = ~0;      // opzionale, per check suolo
    public float groundCheckDist = 0.2f;

    AudioSource _audio;
    NavMeshAgent _agent;
    float _clock;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 1f; // 3D
        _agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (!_agent) return;
        if (!IsGrounded()) { _clock = 0f; return; }

        // velocità orizzontale reale dell'agent
        var v = _agent.velocity; v.y = 0;
        float speed = v.magnitude;
        if (speed < minSpeed) { _clock = 0f; return; }

        // riduci l'intervallo quando corre (più vicino a agent.speed = max)
        float k = Mathf.Clamp01(speed / Mathf.Max(0.01f, _agent.speed));
        float interval = stepInterval * Mathf.Lerp(1f, runStepMultiplier, k);

        _clock += Time.deltaTime;
        if (_clock >= interval)
        {
            _clock = 0f;
            PlayStep();
        }
    }

    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.05f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDist, groundMask, QueryTriggerInteraction.Ignore);
    }

    void PlayStep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        _audio.pitch = Random.Range(pitchMin, pitchMax);
        _audio.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
    }
}

