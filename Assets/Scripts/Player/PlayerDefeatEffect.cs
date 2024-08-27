using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDefeatEffect : MonoBehaviour
{
    public AudioClip playerDeathSound;
    public float explosionSpeed = 0.75f;
    GameObject[] explosions = new GameObject[12];
    Vector3[] explosionVectors =
    {
        new Vector3(-1f, 0, 0),
        new Vector3(1f, 0, 0),
        new Vector3(0, -1f, 0),
        new Vector3(0, 1f, 0),
        new Vector3(-0.75f, -0.75f, 0),
        new Vector3(-0.75f, 0.75f, 0),
        new Vector3(0.75f, -0.75f, 0),
        new Vector3(0.75f, 0.75f, 0),
        new Vector3(-0.5f, 0, 0),
        new Vector3(0.5f, 0, 0),
        new Vector3(0, 0.5f, 0),
        new Vector3(0, -0.5f, 0),
    };

    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance.Play(playerDeathSound);
        for (int i = 0; i < explosions.Length; i++)
        {
            string explosionName = "Explosion" + (i + 1).ToString();
            explosions[i] = transform.Find(explosionName).gameObject;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < explosions.Length; i++)
        {
            Vector3 position = explosions[i].transform.position;
            position.x += explosionVectors[i].x * explosionSpeed * Time.deltaTime;
            position.y += explosionVectors[i].y * explosionSpeed * Time.deltaTime;
            position.z += explosionVectors[i].z * explosionSpeed * Time.deltaTime;
            explosions[i].transform.position = position;
        }
    }
}
