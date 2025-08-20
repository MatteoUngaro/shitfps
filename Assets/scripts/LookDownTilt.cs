using UnityEngine;

[ExecuteAlways]
public class LookDownTilt : MonoBehaviour
{
    public Transform cameraPivot;                 // lascia vuoto: usa Camera.main
    public Transform[] tiltTargets;               // metti i tuoi 2 oggetti (o più)

    [Range(0,90)] public float maxDownAngle = 60f;// sotto questo angolo = tilt pieno
    [Range(0,45)] public float maxTilt = 12f;     // gradi di roll verso l'interno
    public float tiltSpeed = 12f;
    public bool invertDirection = false;

    // Asse locale su cui inclinare (molte armi vogliono Z per il roll)
    public Vector3 localAxis = Vector3.forward;

    private Quaternion[] baseLocalRot;

    void OnEnable()  { Init(); }
    void Start()     { Init(); }
    void OnValidate(){ Init(); }

    void Init()
    {
        if (cameraPivot == null && Camera.main != null)
            cameraPivot = Camera.main.transform;

        if (tiltTargets == null) return;

        if (baseLocalRot == null || baseLocalRot.Length != tiltTargets.Length)
            baseLocalRot = new Quaternion[tiltTargets.Length];

        for (int i = 0; i < tiltTargets.Length; i++)
            if (tiltTargets[i] != null)
                baseLocalRot[i] = tiltTargets[i].localRotation;
    }

    void LateUpdate()
    {
        if (cameraPivot == null || tiltTargets == null || tiltTargets.Length == 0) return;

        // Angolo "quanto stai guardando in giù" indipendente dal parent
        Vector3 fwd = cameraPivot.forward;
        Vector3 fwdOnPlane = Vector3.ProjectOnPlane(fwd, Vector3.up);
        float angleDown = Vector3.SignedAngle(fwdOnPlane, fwd, cameraPivot.right); // >0 quando guardi in basso
        float t = Mathf.Clamp01(Mathf.Max(0f, angleDown) / Mathf.Max(0.0001f, maxDownAngle));

        float dir = invertDirection ? -1f : 1f;
        float roll = dir * maxTilt * t;

        float s = 1f - Mathf.Exp(-tiltSpeed * Time.deltaTime);

        for (int i = 0; i < tiltTargets.Length; i++)
        {
            var tr = tiltTargets[i];
            if (!tr) continue;
            Quaternion target = baseLocalRot[i] * Quaternion.AngleAxis(roll, localAxis);
            tr.localRotation = Quaternion.Slerp(tr.localRotation, target, s);
        }
    }
}
