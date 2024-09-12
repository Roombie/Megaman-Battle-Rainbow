using System.Collections;
using System.Collections.Generic;
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

    [Header("Weapons Menu")]
    private GameObject weaponsMenu;

    [Header("Game Over")]
    public AudioClip gameOverSoundClip;

    Megaman.WeaponTypes playerWeaponType;
    Megaman.WeaponsStruct[] playerWeapons;

    private void Start()
    {
        player = FindObjectOfType<Megaman>();
        // Find the WeaponsMenu GameObject in the scene
        weaponsMenu = GameObject.Find("WeaponsMenu");
        HideWeaponsMenu(); // Ensure the menu is hidden at the start
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
        if (weaponsMenu == null)
        {
            Debug.LogError("WeaponsMenu GameObject not found in the scene.");
            return;
        }

        weaponsMenu.GetComponent<WeaponsMenu>().SetMenuData(playerLives);
        weaponsMenu.GetComponent<WeaponsMenu>().ShowMenu();
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
        Debug.Log("Toggle pause");
        if (isGamePaused)
        {
            ShowWeaponsMenu();
        }
        else
        {
            HideWeaponsMenu();
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