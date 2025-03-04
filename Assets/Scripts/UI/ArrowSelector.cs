using System.Collections;
using UnityEngine;

public class ArrowSelector : MonoBehaviour
{
    [System.Serializable]
    public struct ButtonData
    {
        public RectTransform button;
        public Vector2 arrowOffset;
    }

    [SerializeField] ButtonData[] buttons;
    [SerializeField] RectTransform arrowIndicator;
    [HideInInspector] public bool isSelectingOption = false;

    [HideInInspector] public int lastSelected = -1;
    bool firstFrame = true;

    void LateUpdate()
    {
        if (firstFrame)
        {
            firstFrame = false;
        }

        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null || isSelectingOption)
    {
        arrowIndicator.gameObject.SetActive(false);
    }
    }

    public void PointerEnter(int b)
    {
        // MoveIndicator(b);
    }

    public void PointerExit(int b)
    {
        // MoveIndicator(lastSelected);
    }

    public void ButtonSelected(int b)
    {
        lastSelected = b;
        MoveIndicator(b);
    }

    public void MoveIndicator(int b)
    {
        if (isSelectingOption || firstFrame)
        {
            StartCoroutine(MoveIndicatorLaterCoroutine(b));
            return;
        }

        if (b < 0 || b >= buttons.Length || buttons[b].button == null)
        {
            arrowIndicator.gameObject.SetActive(false);
            return;
        }

        arrowIndicator.gameObject.SetActive(true);
        Vector3 calculatedPosition = buttons[b].button.position + ((Vector3)buttons[b].arrowOffset * (Screen.height / 1080f));
        arrowIndicator.position = calculatedPosition;
    }

    IEnumerator MoveIndicatorLaterCoroutine(int b)
    {
        yield return null;
        MoveIndicator(b);
    }

    void OnDrawGizmos()
    {
        if (buttons == null || buttons.Length == 0) return;

        Gizmos.color = Color.blue;

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].button != null)
            {
                Vector3 buttonPos = buttons[i].button.position;
                Vector3 offsetPos = buttonPos + ((Vector3)buttons[i].arrowOffset * (Screen.height / 1080f));

                Gizmos.DrawSphere(buttonPos, 5f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(offsetPos, 7f);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(buttonPos, offsetPos);
            }
        }

        if (lastSelected >= 0 && lastSelected < buttons.Length && buttons[lastSelected].button != null)
        {
            Gizmos.color = Color.red;
            Vector3 selectedPos = buttons[lastSelected].button.position + ((Vector3)buttons[lastSelected].arrowOffset * (Screen.height / 1080f));
            Gizmos.DrawSphere(selectedPos, 10f);
        }
    }
}