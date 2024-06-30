using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Tooltip("The transform of the player character that the camera will follow.")]
    public Transform player;

    [Tooltip("The time it takes for the camera to smoothly follow the player.")]
    public float timeOffset;

    [Tooltip("The offset from the player's position to the camera's position.")]
    public Vector3 offsetPos;

    [Tooltip("The minimum boundary for the camera's position.")]
    public Vector3 boundsMin;

    [Tooltip("The maximum boundary for the camera's position.")]
    public Vector3 boundsMax;

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 startPos = transform.position;
            Vector3 targetPos = player.position;

            targetPos.x += offsetPos.x;
            targetPos.y += offsetPos.y;
            targetPos.z = transform.position.z; // Keep the camera's Z position

            // Clamp the target position to stay within bounds
            targetPos.x = Mathf.Clamp(targetPos.x, boundsMin.x, boundsMax.x);
            targetPos.y = Mathf.Clamp(targetPos.y, boundsMin.y, boundsMax.y);

            // Smoothly interpolate between the current position and the target position
            float t = 1f - Mathf.Pow(1f - timeOffset, Time.deltaTime * 30);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw min bounds in green
        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMin.y, boundsMin.z), new Vector3(boundsMax.x, boundsMin.y, boundsMin.z));
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMin.y, boundsMin.z), new Vector3(boundsMin.x, boundsMax.y, boundsMin.z));
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMin.y, boundsMin.z), new Vector3(boundsMin.x, boundsMin.y, boundsMax.z));

        // Draw max bounds in red
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(boundsMax.x, boundsMax.y, boundsMax.z), new Vector3(boundsMin.x, boundsMax.y, boundsMax.z));
        Gizmos.DrawLine(new Vector3(boundsMax.x, boundsMax.y, boundsMax.z), new Vector3(boundsMax.x, boundsMin.y, boundsMax.z));
        Gizmos.DrawLine(new Vector3(boundsMax.x, boundsMax.y, boundsMax.z), new Vector3(boundsMax.x, boundsMax.y, boundsMin.z));

        // Connect min and max bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMax.y, boundsMin.z), new Vector3(boundsMax.x, boundsMax.y, boundsMin.z));
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMin.y, boundsMax.z), new Vector3(boundsMax.x, boundsMin.y, boundsMax.z));
        Gizmos.DrawLine(new Vector3(boundsMax.x, boundsMin.y, boundsMin.z), new Vector3(boundsMax.x, boundsMax.y, boundsMin.z));

        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMax.y, boundsMax.z), new Vector3(boundsMax.x, boundsMax.y, boundsMax.z));
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMin.y, boundsMax.z), new Vector3(boundsMin.x, boundsMax.y, boundsMax.z));
        Gizmos.DrawLine(new Vector3(boundsMax.x, boundsMin.y, boundsMax.z), new Vector3(boundsMax.x, boundsMin.y, boundsMin.z));
    }
}
