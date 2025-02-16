using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    // 在 Unity 编辑器中设置要生成的多种物体的预制体数组
    public GameObject[] objectPrefabs;

    // 预定义的位置数组
    public Vector3[] objectPositions;



    void Start()
    {
        

        if (objectPrefabs.Length != objectPositions.Length || PlayerPositionRecorder.playerPosition == null)
        {
            Debug.LogError("The number of prefabs and positions does not match!");
            return;
        }

        // 生成数组中所有预制体的多个物体
        GenerateObjects();
    }



    void GenerateObjects()
    {
        // 遍历 objectPrefabs 数组
        for (int i = 0; i < objectPrefabs.Length; i++)
        {
            // 在预定义位置生成预制体的物体
            Instantiate(objectPrefabs[i],PlayerPositionRecorder.playerPosition + objectPositions[i], PlayerPositionRecorder.rotation);
            Debug.Log("Position of " +  i + ": " + (PlayerPositionRecorder.playerPosition));
        }
    }
}
