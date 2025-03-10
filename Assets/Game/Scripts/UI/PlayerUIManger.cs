using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour {
    [Header("Health UI")]
    public Image healthBar;

    [Header("Ammo UI")]
    public GameObject ammoDisplaySection;
    public Text currentMagazineText;
    public Text totalAmmoText;

    [Header("Weapon Selection UI")]
    public Image weaponSelectionBorderImage;

    [Header("Action UI")]
    public Text ActionText;

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

    public void UpdateHealthBar(int currentHealth, int maxHealth) {
        healthBar.fillAmount = Mathf.Clamp01(currentHealth / (float) maxHealth);
    }

    public void ActionUIText(string Text) {
        ActionText.text = Text;
    }
}
