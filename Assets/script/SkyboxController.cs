using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkyboxController : MonoBehaviour
{
    public Material[] skyboxMaterials; // 用于存储多个天空盒材质的数组
    private int currentIndex = 0; // 当前天空盒的索引

    void Start()
    {
        RenderSettings.skybox = skyboxMaterials[currentIndex]; // 设置初始天空盒材质
    }


    void ChangeSkybox()
    {
        // 增加索引以选择下一个天空盒，如果达到数组末尾，则回到第一个天空盒
        currentIndex = (currentIndex + 1) % skyboxMaterials.Length;
        RenderSettings.skybox = skyboxMaterials[currentIndex]; // 设置新的天空盒材质
    }
}
