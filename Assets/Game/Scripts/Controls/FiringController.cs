using System.Collections;
using UnityEngine;

public class FiringController : MonoBehaviour {
    public Transform firePoint;
    public float fireRate = 1f;
    public float reloadTime = 2.7f;
    public int magazineCapacity = 8;
    public int maxAmmo = 40;
    public float fireRange = 100f;
    public float damage = 5f;

    private float nextFireTime;
    private int currentMagazine;
    private int currentAmmo;
    private bool isReloading;

    InputManager inputManager;
    PlayerMovement playerMovement;
    public Animator animator;

    private void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        currentMagazine = magazineCapacity;
        currentAmmo = maxAmmo;
    }

    private void Update() {
        if (inputManager.fireInput && Time.time >= nextFireTime && currentMagazine > 0 && isReloading == false) {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }

        if (inputManager.reloadInput && currentMagazine < magazineCapacity && currentAmmo > 0 && isReloading == false) {
            StartCoroutine(Reload());
            animator.SetTrigger("Reloading");
        }
    }

    void Fire() {
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, fireRange)) {
            Debug.Log("Hit" + hit.transform.name);

            Soldier soldier = hit.transform.GetComponent<Soldier>();

            if (soldier != null) {
                soldier.characterHitDamage(damage);
            }

            Boss boss = hit.transform.GetComponent<Boss>();

            if (boss != null) {
                boss.characterHitDamage(damage);
            }
        }
        currentMagazine--;
    }

    IEnumerator Reload() {
        isReloading = true;
        playerMovement.SetReloading(isReloading);

        int ammoToReload = Mathf.Min(magazineCapacity - currentMagazine, currentAmmo);
        yield return new WaitForSeconds(reloadTime);
        currentMagazine += ammoToReload;
        currentAmmo -= ammoToReload;

        if (currentAmmo < maxAmmo - magazineCapacity) {
            maxAmmo = currentAmmo + magazineCapacity;
        }
        isReloading = false;
        playerMovement.SetReloading(isReloading);
    }
}
