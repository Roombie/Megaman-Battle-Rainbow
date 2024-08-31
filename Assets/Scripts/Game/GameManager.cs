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
                Debug.Log("GameManager is NULL");
            }

            return instance;
        }
    }

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }

        DontDestroyOnLoad(this);
    }
    #endregion

    [Header("Lives")]
    public int playerLives = 3;
    public int score = 0;
    public float delayRestartDelay = 5f;
    public float gamePlayerReadyDelay = 3f;
    public Transform[] checkpoints;
    private Transform currentCheckpoint;
    private Megaman player;

    [Header("Pause Menu")]
    public GameObject pauseMenu;
    bool isGamePaused;
    bool pauseMusic;
    bool canPauseGame;
    float timeScale;

    [Header("Game Over")]
    public AudioClip gameOverSoundClip;

    private void Start()
    {
        player = FindObjectOfType<Megaman>();
        pauseMenu.SetActive(false);
    }

    public void ResetGame()
    {
        playerLives = 3;
        currentCheckpoint = checkpoints[0];
        player.currentHealth = player.maxHealth;
    }

    public void AddExtraLife(int extralife = 1)
    {
        playerLives += extralife;
        Debug.Log("Current lives: " + playerLives);
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

    // Respawn the player at the last checkpoint
    public void RespawnPlayer()
    {
        // After a short delay, re-enable the player's controls
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
        AudioManager.Instance.Play(gameOverSoundClip);
    }

    public void AddScorePoints(int points)
    {
        score += points;
    }

    public void RestoreFullHealth(AudioClip itemSound)
    {
        if (player.currentHealth < player.maxHealth)
        {
            StartCoroutine(IncrementHealth(player.maxHealth - player.currentHealth, itemSound));
        }
    }

    public void RestoreHealth(int amount, AudioClip itemSound, bool freezeEverything = true)
    {
        if (player.currentHealth != player.maxHealth)
        StartCoroutine(IncrementHealth(amount, itemSound, freezeEverything));
    }

    private IEnumerator IncrementHealth(int amount, AudioClip itemSound, bool freezeEverything = true)
    {
        int healthToRestore = Mathf.Clamp(amount, 0, player.maxHealth - player.currentHealth);
        if (freezeEverything) FreezeEverything(true);
        while (healthToRestore > 0)
        {
            AudioManager.Instance.Play(itemSound);
            player.currentHealth++;  // This increments player's health
            UIHealthBar.Instance.SetValue(player.currentHealth / (float)player.maxHealth);  // And then, update health bar UI
            Debug.Log("Current health: " + player.currentHealth);
            healthToRestore--;

            yield return new WaitForSeconds(0.1f); 
        }
        FreezeEverything(false);
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    #region Game Paused
    public bool IsGamePaused()
    {
        // return the game pause state
        return isGamePaused;
    }

    public void AllowGamePause(bool pause)
    {
        // flag to allow if the game can be paused
        canPauseGame = pause;
    }

    // Pause and resume game
    public void TogglePause(bool pauseMusic = true)
    {
        // if we can pause the game and it isn't already paused
        if (canPauseGame && !isGamePaused)
        {
            isGamePaused = true;
            timeScale = Time.timeScale;
            Time.timeScale = 0;
        }
        else if (isGamePaused)
        {
            // if the game is paused then unpause it, but not when in weapons menu
            isGamePaused = false;
            Time.timeScale = timeScale;
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
        // freeze all enemies
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            enemy.GetComponent<EnemyController>().FreezeEnemy(freeze);
        }
    }

    public void FreezeExplosions(bool freeze)
    {
        // freeze all explosions
        GameObject[] explosions = GameObject.FindGameObjectsWithTag("Explosion");
        foreach (GameObject explosion in explosions)
        {
            explosion.GetComponent<Explosion>().FreezeExplosion(freeze);
        }
    }

    public void FreezeEverything(bool freeze)
    {
        Debug.Log("Freezing everything");
        // one method to freeze everything except the player if needed
        FreezeEnemies(freeze);
        FreezeExplosions(freeze);
        FreezePlayer(freeze);
    }
    #endregion
}