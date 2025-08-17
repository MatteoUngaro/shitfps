using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    public GameObject viewModel;         // arma in mano (attiva/disattiva)
    public GameObject worldPrefab;       // prefab fisico da buttare
    public Transform dropPoint;          // dove spawnarla (davanti al player)
    public float pickupRange = 2f;

    bool hasWeapon = true;

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && hasWeapon)
        {
            DropWeapon();
        }

        if (Keyboard.current.eKey.wasPressedThisFrame && !hasWeapon)
        {
            TryPickup();
        }
    }

    void DropWeapon()
    {
        hasWeapon = false;
        viewModel.SetActive(false);

        // istanzia arma fisica
        var dropped = Instantiate(worldPrefab, dropPoint.position, dropPoint.rotation);
        var rb = dropped.GetComponent<Rigidbody>();
        if (rb) rb.AddForce(dropPoint.forward * 2f, ForceMode.Impulse);
    }

    void TryPickup()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            // riprendi arma
            hasWeapon = true;
            viewModel.SetActive(true);

            Destroy(hit.collider.gameObject); // elimina world model
        }
    }
}
