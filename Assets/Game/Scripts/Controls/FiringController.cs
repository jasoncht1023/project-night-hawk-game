using System.Collections;
using UnityEngine;

public class FiringController : MonoBehaviour {
    public Transform firePoint;
    public float fireRate = 0.8f;
    public float reloadTime = 3f;
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
    PlayerUIManager playerUIManager;
    public Animator animator;

    [Header("Sound Effects")]
    public AudioSource soundAudioSource;
    public AudioClip fireSoundClip;
    public AudioClip reloadSoundClip;

    [Header("Visual Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bloodEffect;

    private void Start() {
        inputManager = FindFirstObjectByType<InputManager>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        playerUIManager = FindFirstObjectByType<PlayerUIManager>();
        currentMagazine = magazineCapacity;
        currentAmmo = maxAmmo;
        playerUIManager.UpdateMagazineCount(currentMagazine);
        playerUIManager.UpdateTotalAmmoCount(currentAmmo);
    }

    private void Update() {
        if (inputManager.fireInput && inputManager.scopeInput && Time.time >= nextFireTime && currentMagazine > 0 && isReloading == false) {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }

        if (inputManager.reloadInput && currentMagazine < magazineCapacity && currentAmmo > 0 && isReloading == false) {
            StartCoroutine(Reload());
            animator.SetTrigger("Reloading");
        }
    }

    void Fire() {
        muzzleFlash.Play();
        soundAudioSource.PlayOneShot(fireSoundClip);

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, fireRange)) {
            Debug.Log("Hit " + hit.transform.name);

            // Get Soldier component based on hit location
            Soldier soldier;

            if (hit.transform.name == "mixamorig:Head") {
                soldier = hit.transform.parent.parent.parent.parent.parent.parent.GetComponent<Soldier>();

                if (soldier != null && soldier.enabled) {
                    soldier.characterDie();
                    CreateBloodEffect(hit);
                }
            }
            else {
                if (hit.transform.name == "mixamorig:LeftArm" || hit.transform.name == "mixamorig:RightArm") {
                    soldier = hit.transform.parent.parent.parent.parent.parent.parent.GetComponent<Soldier>();
                }
                else if (hit.transform.name == "mixamorig:LeftForeArm" || hit.transform.name == "mixamorig:RightForeArm") {
                    soldier = hit.transform.parent.parent.parent.parent.parent.parent.parent.GetComponent<Soldier>();
                }
                else {
                    soldier = hit.transform.GetComponent<Soldier>();
                }

                if (soldier != null && soldier.enabled) {
                    soldier.characterHitDamage(damage);
                    CreateBloodEffect(hit);
                }
            }

            // Handle Boss damage
            Boss boss = hit.transform.GetComponent<Boss>();
            if (boss != null && boss.enabled) {
                boss.characterHitDamage(damage);
                CreateBloodEffect(hit);
            }
        }

        currentMagazine--;
        playerUIManager.UpdateMagazineCount(currentMagazine);
    }

    private void CreateBloodEffect(RaycastHit hit) {
        GameObject bloodEffectGo = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(bloodEffectGo, 1f);
    }

    IEnumerator Reload() {
        isReloading = true;
        playerMovement.SetReloading(isReloading);
        soundAudioSource.PlayOneShot(reloadSoundClip);

        int ammoToReload = Mathf.Min(magazineCapacity - currentMagazine, currentAmmo);
        yield return new WaitForSeconds(reloadTime);
        currentMagazine += ammoToReload;
        currentAmmo -= ammoToReload;
        playerUIManager.UpdateMagazineCount(currentMagazine);
        playerUIManager.UpdateTotalAmmoCount(currentAmmo);

        if (currentAmmo < maxAmmo - magazineCapacity) {
            maxAmmo = currentAmmo + magazineCapacity;
        }
        isReloading = false;
        playerMovement.SetReloading(isReloading);
    }
}
