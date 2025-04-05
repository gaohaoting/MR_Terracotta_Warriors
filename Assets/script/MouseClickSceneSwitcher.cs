using UnityEngine;
using UnityEngine.SceneManagement;

public class MouseClickSceneSwitcher : MonoBehaviour
{
    // 在 Unity 编辑器中设置要切换到的目标场景的名称
    public string targetSceneName;

    void Update()
    {
        // 检测鼠标左键是否被点击
        if (Input.GetMouseButtonDown(0))
        {
            // 切换到目标场景
            SceneManager.LoadScene(targetSceneName);
        }
    }
}

