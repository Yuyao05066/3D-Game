using UnityEngine;

public class SpawnPointManager : MonoBehaviour
{
    public static string nextSpawnPoint;

    void Start()
    {
        Debug.Log("SpawnPointManager Start, nextSpawnPoint = " + nextSpawnPoint);

        if (!string.IsNullOrEmpty(nextSpawnPoint))
        {
            GameObject player = GameObject.FindWithTag("Player");
            GameObject spawn = GameObject.Find(nextSpawnPoint);

            Debug.Log("Found player: " + player);
            Debug.Log("Found spawn: " + spawn);

            if (player != null && spawn != null)
            {
                player.transform.position = spawn.transform.position;
                player.transform.rotation = spawn.transform.rotation;
                Debug.Log("Player moved to spawn");
            }
            else
            {
                Debug.LogWarning("Player 或 Spawn 找不到！");
            }
        }
    }

}