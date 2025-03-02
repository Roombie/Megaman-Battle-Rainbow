using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class PageFader : MonoBehaviour
{
    [System.Serializable]
    public class PageEvent
    {
        public UnityEvent onPageExit;
        public UnityEvent onNewPageEnter;
    }

    public List<CanvasGroup> pages;
    public List<PageEvent> pageEvents;
    public float fadeDuration = 0.5f;
    public float delayBeforeNextPage = 0.5f;
    private int currentPageIndex = 0;

    private void Start()
    {
        if (pages.Count > 0)
        {
            ShowPage(currentPageIndex);
        }
    }

    public void NextPage()
    {
        StartCoroutine(SwitchPage((currentPageIndex + 1) % pages.Count));
    }

    public void PreviousPage()
    {
        StartCoroutine(SwitchPage((currentPageIndex - 1 + pages.Count) % pages.Count));
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].gameObject.SetActive(i == index);
        }
    }

    private IEnumerator SwitchPage(int nextPageIndex)
    {
        if (pages.Count == 0 || nextPageIndex == currentPageIndex)
            yield break;

        CanvasGroup currentPage = pages[currentPageIndex];
        CanvasGroup nextPage = pages[nextPageIndex];

        InvokePageExitEvent(currentPageIndex);

        // Fade out the current page
        yield return StartCoroutine(FadeCanvasGroup(currentPage, 1f, 0f));
        yield return new WaitForSeconds(delayBeforeNextPage);

        // Switch to the next page
        ShowPage(nextPageIndex);
        nextPage.alpha = 0f; // Ensure the next page starts as fully transparent

        // Fade in the next page
        yield return StartCoroutine(FadeCanvasGroup(nextPage, 0f, 1f));

        // Update the current page index
        currentPageIndex = nextPageIndex;

        // Invocar evento al entrar en la nueva página
        InvokeNewPageEnterEvent(currentPageIndex);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha)
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            canvasGroup.alpha = alpha;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = endAlpha; // Ensure the final value is set
    }

    private void InvokePageExitEvent(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < pageEvents.Count)
        {
            pageEvents[pageIndex]?.onPageExit?.Invoke();
        }
    }

    private void InvokeNewPageEnterEvent(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < pageEvents.Count)
        {
            pageEvents[pageIndex]?.onNewPageEnter?.Invoke();
        }
    }
}
