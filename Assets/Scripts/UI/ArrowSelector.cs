using System.Collections;
using UnityEngine;

public class ArrowSelector : MonoBehaviour
{
    [SerializeField] RectTransform[] buttons;
    [SerializeField] RectTransform arrowIndicator;

    [SerializeField] Vector2 arrowOffset;

    int lastSelected = -1;

    bool firstFrame = true;

    void LateUpdate()
    {
        if (firstFrame)
        {
            firstFrame = false;
        }
    }

    public void PointerEnter(int b)
    {
        // //print("enter: " + b);
        // MoveIndicator(b);
    }

    public void PointerExit(int b)
    {
        // //print("exit: " + b);
        // MoveIndicator(lastSelected);
    }

    public void ButtonSelected(int b)
    {
        //print("selected: " + b);
        lastSelected = b;
        MoveIndicator(b);
    }

    // Move the indicator based on keyboard input or mouse input
    public void MoveIndicator(int b)
    {
        if (firstFrame)
        {
            StartCoroutine(MoveIndicatorLaterCoroutine(b));
            return;
        }

        print(buttons[b].position);
        if (b < 0 || b >= buttons.Length)
        {
            // make cursor invisible
            arrowIndicator.gameObject.SetActive(false);
            return;
        }
        arrowIndicator.gameObject.SetActive(true);
        //indicator.position = menuBtn[b].position + ((Vector3)indicatorOffset * (Screen.width / 1920f));
        arrowIndicator.position = buttons[b].position + ((Vector3)arrowOffset * (Screen.height / 1080f));
        print("set pos to " + arrowIndicator.position);
    }

    // Fix for weird issue where the button position is wrong on the first frame
    IEnumerator MoveIndicatorLaterCoroutine(int b)
    {
        yield return null;
        MoveIndicator(b);
    }
    void OnDrawGizmos()
    {
        if (arrowIndicator != null && buttons != null && buttons.Length > 0)
        {
            // Visualize the position based on the last selected button
            if (lastSelected >= 0 && lastSelected < buttons.Length)
            {
                // Calculate the position using the same formula
                Vector3 calculatedPosition = buttons[lastSelected].position + ((Vector3)arrowOffset * (Screen.height / 1080f));

                // Draw a sphere at the calculated position
                Gizmos.color = Color.red; // Set the color of the Gizmo
                Gizmos.DrawSphere(calculatedPosition, 10f); // Draw a sphere at the calculated position, with a radius of 10 units

                // Optionally, draw a line from the button to the calculated position
                Gizmos.color = Color.green;
                Gizmos.DrawLine(buttons[lastSelected].position, calculatedPosition);
            }
        }
    }

}
