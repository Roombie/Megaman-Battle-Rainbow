using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponsMenu : MonoBehaviour
{
    public static WeaponsMenu Instance;

    [System.Serializable]
    public class ItemButton
    {
        public Item.ItemType itemType;
        public TMP_Text countText;   // Use TMP_Text for displaying item counts
        public Button button;        // The button for the item
    }

    [Tooltip("Buttons for weapons/items in the inventory")]
    public ItemButton[] itemButtons;  // Array for item buttons (weapon/utility buttons)
    [Tooltip("UI Text to display player lives")]
    public TMP_Text playerLivesText;  // TMP for displaying player lives
    [Tooltip("UI Image to display weapon icons")]
    public Image[] weaponIcons;       // Array for weapon icons in the UI
    [Tooltip("UI Text to display weapon names")]
    public TMP_Text[] weaponNames;    // Array for weapon names in the UI
    public UIEnergyBar[] weaponEnergyBars;  // Array for weapon energy bars

    private int playerLives;
    private WeaponTypes currentWeaponType;
    private Megaman.WeaponsStruct[] weaponsData; // Now using WeaponsStruct[] to fetch all data

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Inventory Management
    private void OnEnable()
    {
        // Subscribe to inventory updates
        InventoryManager.InventoryUpdated += UpdateInventoryDisplay;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        InventoryManager.InventoryUpdated -= UpdateInventoryDisplay;
    }
    #endregion

    public void SetPlayerLives(int lives)
    {
        playerLives = lives;
        UpdatePlayerLives();
    }

    void UpdatePlayerLives()
    {
        if (playerLivesText != null)
        {
            playerLivesText.text = playerLives.ToString("00");
        }
    }

    public void SetMenuData(int lives, WeaponTypes weaponType, Megaman.WeaponsStruct[] playerWeapons)
    {
        // Set the player lives
        SetPlayerLives(lives);

        // Set the current weapon type
        currentWeaponType = weaponType;

        // Update the weapons data for the player
        weaponsData = playerWeapons;

        // Update the UI with weapon data
        UpdateWeaponDisplays();
    }

    void UpdateWeaponDisplays()
    {
        // Loop through the weapon data and update the corresponding UI elements
        for (int i = 0; i < weaponsData.Length && i < weaponIcons.Length; i++)
        {
            if (weaponIcons[i] == null || weaponNames[i] == null || weaponEnergyBars[i] == null)
                continue; // Skip if any element is missing

            Megaman.WeaponsStruct currentWeapon = weaponsData[i];
            WeaponData weaponData = currentWeapon.weaponData;

            if (currentWeapon.weaponData.isEnabled) // Check if the weapon is unlocked
            {
                // Set weapon icon, name, and energy bar using the WeaponData
                weaponIcons[i].sprite = weaponData.weaponIcon;

                // Check if this is the current weapon
                if (currentWeaponType == currentWeapon.weaponType)
                {
                    weaponIcons[i].sprite = weaponData.weaponIcon; // Use the selected weapon icon
                }
                else
                {
                    weaponIcons[i].sprite = weaponData.weaponIconNotSelected; // Use the unselected weapon icon
                }

                weaponIcons[i].enabled = true;  // Enable the icon if it's disabled

                weaponNames[i].text = weaponData.weaponName;

                // Calculate the energy percentage and update the energy bar
                // float energyPercentage = (float)currentWeapon.weaponData.currentEnergy / weaponData.maxEnergy;
                // weaponEnergyBars[i].SetValue(energyPercentage);
            }
            else
            {
                // Hide or disable display if the weapon is not enabled
                // weaponIcons[i].enabled = false;
                // eaponNames[i].text = "Locked";  // Show placeholder text for locked weapons
                // weaponEnergyBars[i].SetVisibility(false); // Hide the energy bar
            }
        }
    }

    public void SelectWeapon(WeaponTypes selectedWeaponType)
    {
        currentWeaponType = selectedWeaponType;
        Debug.Log("Weapon selected: " + selectedWeaponType);
    }

    #region Enable/Disable menu
    public void ShowMenu()
    {
        gameObject.SetActive(true);
        Debug.Log("Weapons menu active state: " + gameObject.activeSelf);
    }

    public void ExitMenu()
    {
        gameObject.SetActive(false);
        Debug.Log("Deactivate weapons menu");
    }
    #endregion

    #region Inventory
    public void UpdateInventoryDisplay(Item.ItemType itemType, int count)
    {
        foreach (var itemButton in itemButtons)
        {
            if (itemButton.itemType == itemType)
            {
                itemButton.countText.text = count.ToString();  // Update the count text
                itemButton.button.interactable = count > 0;    // Disable the button if count is 0
                return;
            }
        }
    }

    public void UseItem(Item.ItemType itemType)
    {
        if (InventoryManager.Instance.UseItem(itemType))
        {
            Debug.Log($"Used {itemType}!");
        }
        else
        {
            Debug.Log($"No {itemType} left!");
        }
    }

    public void AssignButtonListeners()
    {
        foreach (var itemButton in itemButtons)
        {
            itemButton.button.onClick.AddListener(() => UseItem(itemButton.itemType));
        }
    }

    public void AssignWeaponButtonListeners()
    {
        // Ensure we don't go beyond the length of either array
        int buttonCount = Mathf.Min(itemButtons.Length, weaponsData.Length);

        for (int i = 0; i < buttonCount; i++)
        {
            if (itemButtons[i].button != null)
            {
                WeaponTypes weaponType = weaponsData[i].weaponType;  // Get weapon type for the button

                // Remove previous listeners to avoid stacking multiple listeners
                itemButtons[i].button.onClick.RemoveAllListeners();

                // Add listener to select the correct weapon when the button is clicked
                itemButtons[i].button.onClick.AddListener(() => SelectWeapon(weaponType));
            }
        }
    }
    #endregion
}
