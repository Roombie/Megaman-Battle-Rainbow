using UnityEngine;
using TMPro;

public class WeaponsMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text playerLivesText;
    [SerializeField] private AudioClip menuSelectClip;

    private int playerLives;

    public void SetMenuData(int lives)
    {
        playerLives = lives;
        UpdatePlayerLives();
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
        gameObject.SetActive(true);
        Debug.Log("Activate weapons menu");
    }

    public void ExitMenu()
    {
        // exits the menu
        gameObject.SetActive(false);
        Debug.Log("Deactivate weapons menu");
    }
}
