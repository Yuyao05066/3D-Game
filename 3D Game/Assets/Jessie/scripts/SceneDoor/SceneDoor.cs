using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTeleport : MonoBehaviour
{
    public string targetScene;        // 要切换到哪个场景
    public string targetSpawnName;    // 在目标场景出生点的名字

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // 告诉下一个场景 应该去哪一个 spawn
            SpawnPointManager.nextSpawnPoint = targetSpawnName;

            // 切换场景
            SceneManager.LoadScene(targetScene);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
            Debug.Log("玩家进入触发范围");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
    
}

