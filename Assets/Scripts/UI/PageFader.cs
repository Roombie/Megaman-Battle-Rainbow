using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
    private bool isTransitioning = false;

    public Dictionary<(int fromPage, int toPage), GameObject> transitionButtons = new Dictionary<(int, int), GameObject>();

    public List<TransitionButton> transitionButtonList;

    [System.Serializable]
    public class TransitionButton
    {
        public int fromPage;
        public int toPage;
        public GameObject button;
    }

    private void Start()
    {
        if (pages.Count > 0)
        {
            ShowPage(currentPageIndex);
        }

        foreach (var tb in transitionButtonList)
        {
            transitionButtons[(tb.fromPage, tb.toPage)] = tb.button;
        }
    }

    public void NextPage()
    {
        GoToPage((currentPageIndex + 1) % pages.Count);
    }

    public void PreviousPage()
    {
        GoToPage((currentPageIndex - 1 + pages.Count) % pages.Count);
    }

    public void GoToPage(int pageIndex)
    {
        if (isTransitioning || pages.Count == 0 || pageIndex < 0 || pageIndex >= pages.Count || pageIndex == currentPageIndex)
            return;

        StartCoroutine(SwitchPage(pageIndex));
    }

    private IEnumerator SwitchPage(int nextPageIndex)
    {
        isTransitioning = true;

        CanvasGroup currentPage = pages[currentPageIndex];
        CanvasGroup nextPage = pages[nextPageIndex];

        InvokePageExitEvent(currentPageIndex);

        EventSystem.current.SetSelectedGameObject(null);
        yield return new WaitForEndOfFrame();

        // Fade out the current page
        yield return StartCoroutine(FadeCanvasGroup(currentPage, 1f, 0f));
        yield return new WaitForSeconds(delayBeforeNextPage);

        // Switch to the next page
        ShowPage(nextPageIndex);
        nextPage.alpha = 0f; // Ensure the next page starts as fully transparent

        // Fade in the next page
        yield return StartCoroutine(FadeCanvasGroup(nextPage, 0f, 1f));

        int previousPageIndex = currentPageIndex;

        currentPageIndex = nextPageIndex;

        yield return new WaitForEndOfFrame();
        
        SetReturnButton(previousPageIndex, nextPageIndex);

        // Invoke event for entering the new page
        InvokeNewPageEnterEvent(currentPageIndex);

        isTransitioning = false;
    }

    private void SetReturnButton(int previousPageIndex, int nextPageIndex)
    {
        EventSystem.current.SetSelectedGameObject(null);

        if (transitionButtons.TryGetValue((previousPageIndex, nextPageIndex), out GameObject button))
        {
            if (button != null && button.activeInHierarchy)
            {
                EventSystem.current.SetSelectedGameObject(button);
                Debug.Log($"Button assigned correctly: {button.name} for the transition {previousPageIndex} -> {nextPageIndex}");
            }
            else
            {
                Debug.LogWarning($"The button transition {previousPageIndex} -> {nextPageIndex} is not active.");
            }
        }
        else
        {
            Debug.LogWarning($"No button found for the transition {previousPageIndex} -> {nextPageIndex}.");
        }
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].gameObject.SetActive(i == index);
        }
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