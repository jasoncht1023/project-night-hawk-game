using System.Collections;
using UnityEngine;

public class FiringController : MonoBehaviour
{
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
    public Animator animator;

    [Header("Sound Effects")]
    public AudioSource soundAudioSource;
    public AudioClip fireSoundClip;
    public AudioClip reloadSoundClip;

    [Header("Visual Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bloodEffect;

    private void Start()
    {
        inputManager = FindFirstObjectByType<InputManager>();
        playerMovement = FindFirstObjectByType<PlayerMovement>();
        currentMagazine = magazineCapacity;
        currentAmmo = maxAmmo;
    }

    private void Update()
    {
        if (inputManager.fireInput && inputManager.scopeInput && Time.time >= nextFireTime && currentMagazine > 0 && isReloading == false)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }

        if (inputManager.reloadInput && currentMagazine < magazineCapacity && currentAmmo > 0 && isReloading == false)
        {
            StartCoroutine(Reload());
            animator.SetTrigger("Reloading");
        }
    }

    void Fire()
    {
        muzzleFlash.Play();
        soundAudioSource.PlayOneShot(fireSoundClip);

        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, fireRange))
        {
            Debug.Log("Hit " + hit.transform.name);

            // Get Soldier component based on hit location
            Soldier soldier;
            if (hit.transform.name == "Head" || hit.transform.name == "Helmet")
            {
                soldier = hit.transform.parent.GetComponent<Soldier>();
                if (soldier != null) soldier.characterDie();
            }
            else
            {
                soldier = hit.transform.GetComponent<Soldier>();
            }

            // Handle damage and effects for Soldier
            if (soldier != null)
            {
                soldier.characterHitDamage(damage);
                CreateBloodEffect(hit);
            }

            // Handle Boss damage
            Boss boss = hit.transform.GetComponent<Boss>();
            if (boss != null)
            {
                boss.characterHitDamage(damage);
                CreateBloodEffect(hit);
            }
        }

        currentMagazine--;
    }

    private void CreateBloodEffect(RaycastHit hit)
    {
        GameObject bloodEffectGo = Instantiate(bloodEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(bloodEffectGo, 1f);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        playerMovement.SetReloading(isReloading);
        soundAudioSource.PlayOneShot(reloadSoundClip);

        int ammoToReload = Mathf.Min(magazineCapacity - currentMagazine, currentAmmo);
        yield return new WaitForSeconds(reloadTime);
        currentMagazine += ammoToReload;
        currentAmmo -= ammoToReload;

        if (currentAmmo < maxAmmo - magazineCapacity)
        {
            maxAmmo = currentAmmo + magazineCapacity;
        }
        isReloading = false;
        playerMovement.SetReloading(isReloading);
    }
}
