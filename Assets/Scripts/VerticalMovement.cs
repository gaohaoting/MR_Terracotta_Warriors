using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalMovement : MonoBehaviour
{
    public float speed = 2.0f; // 移动速度
    public float range = 2.0f; // 移动范围

    private Vector3 initialLocalPosition;
    private float direction = 1.0f; // 移动方向（1为向上，-1为向下）

    void Start()
    {
        // 记录物体相对于父物体的初始位置
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        // 计算新的Y轴位置
        float newY = Mathf.PingPong(Time.time * speed, range) + initialLocalPosition.y;

        // 更新物体相对于父物体的位置
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}
