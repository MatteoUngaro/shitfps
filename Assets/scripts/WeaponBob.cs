using UnityEngine;
using StarterAssets;

public class WeaponBob : MonoBehaviour
{
    public Transform weapon;                 // se vuoto usa questo transform
    public CharacterController controller;   // dal player
    public StarterAssetsInputs input;        // opzionale (per sprint)

    [Header("Walk (Verticale)")]
    public float walkAmplitude = 0.015f;
    public float walkFrequency = 10f;

    [Header("Sprint (Verticale)")]
    public float sprintAmplitude = 0.03f;
    public float sprintFrequency = 14f;

    [Header("Orizzontale")]
    public float walkHorizontalAmplitude = 0.01f;   // ampiezza X camminata
    public float sprintHorizontalAmplitude = 0.02f; // ampiezza X sprint
    public float horizontalFreqMult = 2f;           // X oscilla più veloce (classico 2:1)

    [Header("Misc")]
    public float speedForMaxBob = 5f;  // velocità a cui il bob raggiunge l'ampiezza max
    public float returnSpeed = 10f;    // velocità di ritorno a riposo

    Vector3 startLocalPos;
    float t;

    void Awake() {
        if (!weapon) weapon = transform;
        if (!controller) controller = GetComponentInParent<CharacterController>();
        if (!input) input = GetComponentInParent<StarterAssetsInputs>();
        startLocalPos = weapon.localPosition;
    }

    void Update() {
        float speed = 0f;
        bool grounded = true;

        if (controller) {
            var v = controller.velocity; 
            speed = new Vector3(v.x, 0, v.z).magnitude;
            grounded = controller.isGrounded;
        }

        bool moving = grounded && speed > 0.05f;
        bool sprinting = input && input.sprint;

        // parametri verticali
        float ampY = sprinting ? sprintAmplitude : walkAmplitude;
        float freqY = sprinting ? sprintFrequency : walkFrequency;

        // parametri orizzontali
        float ampX = sprinting ? sprintHorizontalAmplitude : walkHorizontalAmplitude;

        // scala gli effetti con la velocità (0..1)
        float speedScale = Mathf.Clamp01(speed / speedForMaxBob);
        ampY *= speedScale;
        ampX *= speedScale;

        if (moving) {
            t += Time.deltaTime * freqY;

            // classico pattern: Y = sin(t), X = sin(t*2) per un dondolio laterale più rapido
            float y = Mathf.Sin(t) * ampY;
            float x = Mathf.Sin(t * horizontalFreqMult) * ampX;

            Vector3 target = startLocalPos + new Vector3(x, y, 0f);
            weapon.localPosition = Vector3.Lerp(weapon.localPosition, target, Time.deltaTime * 20f);
        } else {
            t = 0f;
            weapon.localPosition = Vector3.Lerp(weapon.localPosition, startLocalPos, Time.deltaTime * returnSpeed);
        }
    }
}
