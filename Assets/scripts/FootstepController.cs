using UnityEngine;
using StarterAssets; // Starter Assets FP Controller

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(AudioSource))]
public class FootstepController : MonoBehaviour
{
    public AudioClip[] footstepSounds;
    public float stepInterval = 0.5f;     // passo camminando
    public float runStepMultiplier = 0.7f; // passi più frequenti correndo

    AudioSource _audio;
    CharacterController _cc;
    StarterAssetsInputs _inputs;

    float _stepClock;

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        _audio.playOnAwake = false;
        _audio.spatialBlend = 1f; // 3D
        _cc = GetComponent<CharacterController>();
        _inputs = GetComponent<StarterAssetsInputs>();
    }

    void Update()
    {
        if (!_cc.isGrounded) { _stepClock = 0f; return; }

        Vector3 v = _cc.velocity; v.y = 0;
        float speed = v.magnitude;

        if (speed < 0.1f) { _stepClock = 0f; return; }

        float interval = stepInterval * (_inputs != null && _inputs.sprint ? runStepMultiplier : 1f);
        _stepClock += Time.deltaTime * speed; // più veloce = passi più ravvicinati

        if (_stepClock >= interval)
        {
            _stepClock = 0f;
            PlayStep();
        }
    }

    void PlayStep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0) return;
        _audio.pitch = Random.Range(0.95f, 1.05f);
        _audio.PlayOneShot(footstepSounds[Random.Range(0, footstepSounds.Length)]);
    }
}
