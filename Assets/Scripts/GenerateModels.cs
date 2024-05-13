using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateModels : MonoBehaviour
{
    public GameObject modelPrefab; // 要生成的模型预制体

    void Start()
    {
        // 两秒后执行GenerateModelsAtChildPositions方法
        Invoke("GenerateModelsAtChildPositions", 2f);
    }

    void GenerateModelsAtChildPositions()
    {
        // 获取母物体下的所有子物体
        for (int i = 0; i < 10f; i++)
        {
            Transform child = transform.GetChild(i);

            // 获取预制体中对应的子物体
            Transform newModelTransform = modelPrefab.transform.GetChild(i);

            // 在子物体的位置生成新的子物体，并使用预制体中对应的子物体进行替换
            GameObject newModel = Instantiate(newModelTransform.gameObject, child.position, child.rotation, transform);
            
            // 保留原子物体的缩放信息
            newModel.transform.localScale = child.localScale;
            
            // 销毁原有的子物体
            Destroy(child.gameObject);
            Debug.Log(i);
        }
    }
}
