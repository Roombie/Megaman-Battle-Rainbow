using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderHandlers : MonoBehaviour
{
    void Awake()
    {
        // Get the dimensions of the ladder sprite
        float width = GetComponent<SpriteRenderer>().size.x;
        float height = GetComponent<SpriteRenderer>().size.y;

        // Get references to the top and bottom handler transforms
        Transform topHandler = transform.GetChild(0).transform;
        Transform bottomHandler = transform.GetChild(1).transform;

        // Position the top handler at the top of the ladder
        topHandler.position = new Vector3(transform.position.x, transform.position.y + (height / 2), 0);

        // Position the bottom handler at the bottom of the ladder
        bottomHandler.position = new Vector3(transform.position.x, transform.position.y - (height / 2), 0);
        GetComponent<BoxCollider2D>().offset = Vector2.zero; // Center the collider
        GetComponent<BoxCollider2D>().size = new Vector2(width, height); // Set collider size to match ladder sprite
    }
}
