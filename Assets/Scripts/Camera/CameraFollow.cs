using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
     [Tooltip("The transform of the player character that the camera will follow.")]
    public Transform player;

    [Tooltip("The time it takes for the camera to smoothly follow the player.")]
    public float smoothDampTime;

    [Tooltip("The offset from the player's position to the camera's position.")]
    public Vector3 lookAhead;

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

            targetPos.x += lookAhead.x;
            targetPos.y += lookAhead.y;
            targetPos.z = transform.position.z; // Keep the camera's Z position

            // Clamp the target position to stay within bounds
            targetPos.x = Mathf.Clamp(targetPos.x, boundsMin.x, boundsMax.x);
            targetPos.y = Mathf.Clamp(targetPos.y, boundsMin.y, boundsMax.y);

            // Smoothly interpolate between the current position and the target position
            float t = 1f - Mathf.Pow(1f - smoothDampTime, Time.deltaTime * 30);
            transform.position = Vector3.Lerp(startPos, targetPos, t);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw a rectangle representing the bounds
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMin.y, transform.position.z), new Vector3(boundsMax.x, boundsMin.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(boundsMax.x, boundsMin.y, transform.position.z), new Vector3(boundsMax.x, boundsMax.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(boundsMax.x, boundsMax.y, transform.position.z), new Vector3(boundsMin.x, boundsMax.y, transform.position.z));
        Gizmos.DrawLine(new Vector3(boundsMin.x, boundsMax.y, transform.position.z), new Vector3(boundsMin.x, boundsMin.y, transform.position.z));
    }
}
