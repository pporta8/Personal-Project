using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject[] Enemies;
    public GameObject enemyprefab;
    public int maxEnemies, currentEnemies;
    public float spawnTimer, maxTimer;
    void Start()
    {
        spawnTimer = maxTimer;
        Enemies = GameObject.FindGameObjectsWithTag("Enemy");
        currentEnemies = Enemies.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentEnemies < maxEnemies)
        {
            spawnTimer -= Time.deltaTime;
        }

        if(currentEnemies < maxEnemies && spawnTimer <= 0)
        {
            Spawn();
            Enemies = GameObject.FindGameObjectsWithTag("Enemy");
            currentEnemies = Enemies.Length;
            spawnTimer = maxTimer;
        }
    }

    void Spawn()
    {
        var enemy = Instantiate(enemyprefab, this.transform.position, Quaternion.identity) as GameObject;
    }
}
