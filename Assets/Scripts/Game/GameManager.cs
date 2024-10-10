using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                Debug.LogError("GameManager is NULL");
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this);
        }
    }
    #endregion

    [Header("Lives")]
    public int playerLives = 3;
    public int gamePlayerStartLives = 3;
    public int score = 0;
    public float delayRestartDelay = 5f;
    public float gamePlayerReadyDelay = 3f;
    public Transform[] checkpoints;
    private Transform currentCheckpoint;
    private Megaman player;

    [Header("Pause")]
    bool isGamePaused;
    bool canPauseGame;
    [SerializeField] private AudioClip openPauseMenuSoundClip;
    [SerializeField] private AudioClip closePauseMenuSoundClip;

    [Header("Screws")]
    public int screwsCollected = 0;

    [Header("Weapons Menu")]
    public GameObject weaponsMenu;

    [Header("Game Over")]
    [SerializeField] AudioClip gameOverSoundClip;

    WeaponTypes playerWeaponType;
    Megaman.WeaponsStruct[] playerWeapons; // Store the player's weapons

    private void Start()
    {
        player = FindObjectOfType<Megaman>();
        var weaponsMenus = Object.FindObjectsOfType<WeaponsMenu>(true);

        if (weaponsMenus.Length > 0)
        {
            weaponsMenu = weaponsMenus[0].gameObject; // Assuming you want the first instance
            Debug.Log("WeaponsMenu found successfully");
        }
        else
        {
            Debug.LogError("WeaponsMenu not found in the scene");
        }

        // Ensure we fetch the player's weapons
        playerWeapons = player.GetWeapons();
        // Fetch the player's current weapon type
        playerWeaponType = player.GetCurrentWeaponType();
    }

    #region Lives
    public void AddExtraLife(int extralife = 1)
    {
        playerLives += extralife;
        Debug.Log($"Current lives: {playerLives}");
    }

    public void LoseLife()
    {
        playerLives--;

        if (playerLives >= 0)
        {
            RespawnPlayer();
        }
        else
        {
            GameOver();
        }
    }
    #endregion

    #region Respawn & Game Over
    public void RespawnPlayer()
    {
        StartCoroutine(PlayerRespawnRoutine());
    }

    private IEnumerator PlayerRespawnRoutine()
    {
        yield return new WaitForSeconds(delayRestartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        player.currentHealth = player.maxHealth;
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    private IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(delayRestartDelay);
        SceneManager.LoadScene("GameOverScreen");
        AudioManager.Instance.Play(gameOverSoundClip, SoundCategory.Music);
    }
    #endregion

    #region Screws
    public void AddScrew(int amount)
    {
        screwsCollected += amount;
        Debug.Log($"Screws collected: {screwsCollected}");
    }

    public bool SpendScrews(int amount)
    {
        if (screwsCollected >= amount)
        {
            screwsCollected -= amount;
            Debug.Log($"Spent {amount} screws. Remaining screws: {screwsCollected}");
            return true;
        }
        else
        {
            Debug.Log("Not enough screws.");
            return false;
        }
    }
    #endregion

    public void AddScorePoints(int points)
    {
        score += points;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    #region Weapon Menu
    public void ShowWeaponsMenu()
    {
        // Find all WeaponsMenu instances, including inactive ones
        var weaponsMenus = Object.FindObjectsOfType<WeaponsMenu>(true);

        if (weaponsMenus.Length > 0)
        {
            // Access the first found WeaponsMenu
            weaponsMenu = weaponsMenus[0].gameObject;

            // Get player's weapon data
            playerWeapons = player.GetWeapons();  // Get the player's weapons data
            playerWeaponType = player.GetCurrentWeaponType(); // Get the player's current weapon type

            // Set menu data and show the menu
            weaponsMenu.GetComponent<WeaponsMenu>().SetMenuData(playerLives, playerWeaponType, playerWeapons);
            weaponsMenu.GetComponent<WeaponsMenu>().ShowMenu();

            Debug.Log($"Lives: {playerLives}, Weapon Type: {playerWeaponType}, Information: {playerWeapons}");
        }
        else
        {
            Debug.LogError("WeaponsMenu not found when trying to show the menu");
        }
    }


    public void HideWeaponsMenu()
    {
        if (weaponsMenu != null)
        {
            weaponsMenu.GetComponent<WeaponsMenu>().ExitMenu();
        }
    }
    #endregion

    #region Game Paused
    public bool IsGamePaused()
    {
        return isGamePaused;
    }

    public void AllowGamePause(bool pause)
    {
        canPauseGame = pause;
    }

    public void ToggleWeaponsMenu(bool pauseMusic = true)
    {
        isGamePaused = !isGamePaused;
        Debug.Log("Toggle pause: " + isGamePaused);

        if (isGamePaused)
        {
            AudioManager.Instance.Play(openPauseMenuSoundClip);
            ShowWeaponsMenu(); // Show weapons menu when pausing
        }
        else
        {
            AudioManager.Instance.Play(closePauseMenuSoundClip);
            HideWeaponsMenu(); // Hide weapons menu when unpausing
        }
    }
    #endregion

    #region Freeze entities
    public void FreezePlayer(bool freeze)
    {
        if (player != null)
        {
            player.GetComponent<Megaman>().FreezeInput(freeze);
            player.GetComponent<Megaman>().FreezePlayer(freeze);
        }
    }

    public void FreezeEnemies(bool freeze)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyController>().FreezeEnemy(freeze);
        }
    }

    public void FreezeExplosions(bool freeze)
    {
        GameObject[] explosions = GameObject.FindGameObjectsWithTag("Explosion");
        foreach (GameObject explosion in explosions)
        {
            explosion.GetComponent<Explosion>().FreezeExplosion(freeze);
        }
    }

    public void FreezeEverything(bool freeze)
    {
        Debug.Log("Freezing everything");
        FreezeEnemies(freeze);
        FreezeExplosions(freeze);
        FreezePlayer(freeze);
    }
    #endregion
}
