using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RayFire;

public class ActiveOthers : MonoBehaviour
{
    public RayfireRigid scriptInstance;

    void Start()
    {
        // 获取 Script的实例
        scriptInstance = GameObject.Find("BMY Crack").GetComponent<RayfireRigid>();
        
        // 检查 Script 是否为空
        if (scriptInstance != null)
        {
            // 改变 Script 中的变量
            //scriptInstance.InitType = scriptBInstance.InitType.AtStart;
            
            // 激活 Script中的函数
            scriptInstance.Initialize();
        }
        else
        {
            Debug.LogError("ScriptB instance not found!");
        }
    }
}