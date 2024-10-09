using UnityEngine;
using TMPro;

public class WeaponsMenu : MonoBehaviour
{
    [SerializeField] private AudioClip menuSelectClip;
    [SerializeField] private TMP_Text playerLivesText;

    [Header("Additional menu text")]
    [SerializeField] private TMP_Text energyTankText;
    [SerializeField] private TMP_Text mysteryTankText;
    [SerializeField] private TMP_Text guardPowerUpText;
    [SerializeField] private TMP_Text eddieCallText;
    [SerializeField] private TMP_Text weaponTankText;
    [SerializeField] private TMP_Text superTankText;
    [SerializeField] private TMP_Text lifeTankText;
    [SerializeField] private TMP_Text screwText;
    [SerializeField] private TMP_Text beatCallText;
    [SerializeField] private TMP_Text shockGuardText;
    [SerializeField] private TMP_Text energyBalancerText;

    private int playerLives;
    private WeaponTypes playerWeaponType;
    private Megaman.WeaponsStruct[] playerWeaponsData;

    public void SetMenuData(int lives, WeaponTypes weaponType, Megaman.WeaponsStruct[] weaponsData)
    {
        playerLives = lives;
        playerWeaponType = weaponType;
        playerWeaponsData = weaponsData;
        UpdatePlayerLives();
    }

    public WeaponTypes GetWeaponSelection()
    {
        // return the selected weapon
        return playerWeaponType;
    }

    void UpdatePlayerLives()
    {
        // Display the player's lives as text
        playerLivesText.text = playerLives.ToString("00");
    }

    void InitializeMenu()
    {
        // Call this method to initialize the menu when it's opened
        UpdatePlayerLives();
    }

    public void ShowMenu()
    {
        // enable the menu
        Debug.Log("ShowMenu called");
        gameObject.SetActive(true);
        Debug.Log("Weapons menu active state: " + gameObject.activeSelf);
    }

    public void ExitMenu()
    {
        // exits the menu
        gameObject.SetActive(false);
        Debug.Log("Deactivate weapons menu");
    }
}
