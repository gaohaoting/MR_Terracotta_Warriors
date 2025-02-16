using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateAndDestroy : MonoBehaviour
{
    public GameObject[] objectsToActivate; // 需要延迟激活的物体数组
    public GameObject[] objectsToDisable; // 需要禁用的物体数组
    public float delayBeforeActivation = 2f; // 延迟激活的时间
    public string Tag;

    private void OnCollisionEnter(Collision collision)
    {
        // 检查碰撞的游戏对象是否为特定对象
        if (collision.gameObject.CompareTag(Tag))
        {
            // 延迟激活物体
            Invoke("ActivateObjects", delayBeforeActivation);

            // 同时禁用物体
            Invoke("DisableObjects", delayBeforeActivation);

            // 延迟销毁当前物体
            Destroy(gameObject, delayBeforeActivation + 0.1f); // + 0.1s 作为保险
        }
    }

    private void ActivateObjects()
    {
        // 延迟激活物体
        foreach (GameObject obj in objectsToActivate)
        {
            obj.SetActive(true);
        }
    }

    private void DisableObjects()
    {
        // 同时禁用物体
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(false);
        }
    }
}
