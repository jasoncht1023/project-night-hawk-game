using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour {
    public GameObject ammoDisplaySection;
    public Text currentMagazineText;
    public Text totalAmmoText;

    public GameObject weaponSelectionBorderImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {

    }

    public void UpdateMagazineCount(int magazine) {
        currentMagazineText.text = magazine.ToString();
    }

    public void UpdateTotalAmmoCount(int ammo) {
        totalAmmoText.text = ammo.ToString();
    }

    public void UpdateWeaponSelection(bool pistolActive) {
        RectTransform borderImageTransform = weaponSelectionBorderImage.GetComponent<RectTransform>();
        if (pistolActive == true) {
            ammoDisplaySection.SetActive(true);
            borderImageTransform.localPosition = new Vector3(-10f, 40f, 0f);
        }
        else {
            ammoDisplaySection.SetActive(false);
            borderImageTransform.localPosition = new Vector3(-10f, -20f, 0f);
        }
    }
}
