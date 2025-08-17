using UnityEngine;

public class SpeedCalculator : MonoBehaviour
{
    public Animator anim;                  // Animator del gnomo (child)
    public CharacterController cc;         // Se usi CharacterController
    public Rigidbody rb;                   // Se usi Rigidbody
    public float maxWalkSpeed = 5f;

    void Reset() {
        if (!anim) anim = GetComponentInChildren<Animator>();
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        float speed = 0f;
        if (cc) speed = new Vector3(cc.velocity.x, 0, cc.velocity.z).magnitude;
        else if (rb) speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

        float normalized = Mathf.Clamp01(speed / maxWalkSpeed);
        if (anim) anim.SetFloat("Speed", normalized);
    }
}
