using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.EventSystems;

public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance;
    public Image fadeImage;
    public float fadeDuration = 0.25f;
    public bool IsTransitioning { get; private set; } = false;
    public bool IsFadingOut { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (fadeImage == null)
        {
            Debug.LogError("ScreenFader: No se ha asignado la imagen para el fade.");
        }
    }

    private void Start()
    {
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        IsTransitioning = true;
        EventSystem.current.sendNavigationEvents = false; 
        fadeImage.gameObject.SetActive(true);
        yield return Fade(1f, 0f); 
        fadeImage.gameObject.SetActive(false); 
        EventSystem.current.sendNavigationEvents = true;
        IsTransitioning = false;
    }

    public IEnumerator FadeOut()
    {
        IsTransitioning = true;
        IsFadingOut = true;
        EventSystem.current.sendNavigationEvents = false; 
        fadeImage.gameObject.SetActive(true);
        yield return Fade(0f, 1f);
        IsFadingOut = false;
    }

    public void ShakeUI(RectTransform uiElement)
    {
        Vector3 originalPos = uiElement.localPosition;

        LeanTween.moveLocalX(uiElement.gameObject, originalPos.x + 10f, 0.05f)
            .setEase(LeanTweenType.easeShake)
            .setLoopPingPong(2)
            .setOnComplete(() => 
            {
                uiElement.localPosition = originalPos; // Restaurar la posición original
            });
    }

    public void FadeToScene(object sceneIdentifier)
    {
        if (IsTransitioning) return; // To avoid spam

        if (sceneIdentifier is int sceneIndex)
        {
            StartCoroutine(FadeOutAndLoadScene(sceneIndex));
        }
        else if (sceneIdentifier is string sceneName)
        {
            StartCoroutine(FadeOutAndLoadScene(sceneName));
        }
        else
        {
            Debug.LogError("ScreenFader: El identificador de la escena no es válido.");
        }
    }

    private IEnumerator FadeOutAndLoadScene(int sceneIndex)
    {
        yield return FadeOut();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        yield return FadeIn();
    }

    private IEnumerator FadeOutAndLoadScene(string sceneName)
    {
        yield return FadeOut();

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);
        yield return FadeIn();
    }

    private IEnumerator Fade(float startAlpha, float targetAlpha)
    {
        float timer = 0f;
        Color color = fadeImage.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(color.r, color.g, color.b, targetAlpha);
    }
}