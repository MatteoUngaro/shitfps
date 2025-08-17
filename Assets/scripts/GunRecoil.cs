using UnityEngine;

public class GunRecoil : MonoBehaviour
{
    public Transform weaponHolder;        // se questo script è sul WeaponHolder puoi lasciarlo vuoto
    public float recoilBack = 0.05f;      // metri indietro
    public float recoilUp = 2f;           // gradi su
    public float returnSpeed = 12f;       // velocità ritorno

    Vector3 defaultPos;
    Quaternion defaultRot;
    float back;   // offset accumulato
    float up;     // rotazione accumulata

    void Awake()
    {
        if (!weaponHolder) weaponHolder = transform;
    }

    void Start()
    {
        defaultPos = weaponHolder.localPosition;
        defaultRot = weaponHolder.localRotation;
    }

    public void DoRecoil()        // chiamato a ogni colpo
    {
        back += recoilBack;
        up   += recoilUp;
    }

    void LateUpdate()             // dopo tutto, così non "combatte" con altri movimenti
    {
        // rientro verso 0
        back = Mathf.Lerp(back, 0f, Time.deltaTime * returnSpeed);
        up   = Mathf.Lerp(up,   0f, Time.deltaTime * returnSpeed);

        // applica
        weaponHolder.localPosition = defaultPos + Vector3.back * back;
        weaponHolder.localRotation = defaultRot * Quaternion.Euler(-up, 0, 0);
    }
}
