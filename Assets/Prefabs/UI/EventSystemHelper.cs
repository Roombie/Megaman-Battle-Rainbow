using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemHelper : MonoBehaviour
{
    public void ClearSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void SetSelection(GameObject target)
    {
        EventSystem.current.SetSelectedGameObject(target);
    }
}