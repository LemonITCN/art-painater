using System;
using System.Collections;
using System.Collections.Generic;
using Service;
using UniGLTF;
using UnityEngine;
using Utils;

public class PainterD3SceneController : MonoBehaviour
{
    private RuntimeGltfInstance _runtimeGltfInstance;
    private GameObject _modelGameObject;
    private Vector2 previousTouchPosition; // 上一次触摸位置
    private Vector2 touchDelta; // 当前的触摸/鼠标移动量
    private bool isDragging = false; // 是否正在拖拽
    private readonly float rotationSpeed = 5f; // 旋转速度
    private readonly float inertiaFactor = 2f; // 惯性系数（阻尼效果）
    private float modelSize = 12f;
    private Vector2 velocity; // 旋转速度
    private Vector2 velocitySmoothDamp; // 平滑的旋转速度
    private readonly float screenFactor = Screen.width * 0.001f;

    private LocalModelLibraryManager _localModelLibraryManager;

    private Vector2 initialTouchDistance; // 双指初始距离
    private float defaultScale; // 初始模型缩放比例
    private float initialScale; // 初始模型缩放比例
    private Vector3 initialModelPosition; // 初始模型位置
    private bool isPinching = false; // 是否正在缩放
    private bool isPanning = false; // 是否正在拖动
    private Vector2 previousPanPosition; // 双指拖动时的上一次中心点位置

    void Start()
    {
        _localModelLibraryManager = gameObject.AddComponent<LocalModelLibraryManager>();
        _localModelLibraryManager.DownloadResource("https://common-resource-1302067637.cos.ap-beijing.myqcloud.com/football.glb",
        // _localModelLibraryManager.DownloadResource("https://common-resource-1302067637.cos.ap-beijing.myqcloud.com/tyrannosaurus-rex.glb",
            resPath =>
            {
                GltfData gltfData = new GlbFileParser(resPath).Parse();
                var loader = new ImporterContext(gltfData);
                _runtimeGltfInstance = loader.Load();
                _modelGameObject = _runtimeGltfInstance.gameObject;
                _runtimeGltfInstance.EnableUpdateWhenOffscreen();
                _runtimeGltfInstance.ShowMeshes();
                _runtimeGltfInstance.transform.localPosition = new Vector3(0, 0, 0);
                Vector3 size = GetObjectSize(_modelGameObject);
                float maxDirection = Math.Max(size.x, Math.Max(size.y, size.z));
                _modelGameObject.transform.localScale = new Vector3(modelSize / maxDirection, modelSize / maxDirection, modelSize / maxDirection);
                defaultScale = _modelGameObject.transform.localScale.x;
                Debug.Log("3D模型加载成功" + size);
            });
    }

    Vector3 GetObjectSize(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            Bounds combinedBounds = renderers[0].bounds;
            foreach (Renderer renderer in renderers)
            {
                combinedBounds.Encapsulate(renderer.bounds);
            }
            return combinedBounds.size;
        }

        Debug.LogWarning("No Renderer found on the GameObject or its children.");
        return Vector3.zero;
    }

    void Update()
    {
        #if UNITY_EDITOR
                HandleMouseInput();
        #else
            HandleTouchInput();
        #endif

        // 应用惯性旋转
        if (!isDragging)
        {
            // 用平滑阻尼来模拟惯性效果，使速度逐渐减小
            velocitySmoothDamp = Vector2.Lerp(velocitySmoothDamp, Vector2.zero, inertiaFactor * Time.deltaTime);
            RotateTarget(velocitySmoothDamp);

            // 如果惯性速度小到一定程度，就停止旋转
            if (velocitySmoothDamp.magnitude < 0.01f) // 停止条件调整为更小的阈值
            {
                velocitySmoothDamp = Vector2.zero;
            }
        }
    }
    
    // 鼠标输入处理
    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // 鼠标左键按下
        {
            previousTouchPosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButton(0) && isDragging) // 鼠标左键拖动
        {
            touchDelta = (Vector2)Input.mousePosition - previousTouchPosition;
            previousTouchPosition = Input.mousePosition;

            // 更新旋转速度（鼠标移动量）
            velocity = touchDelta * rotationSpeed * Time.deltaTime;
            if (velocity.magnitude > 0.01f) // 如果有足够的移动量，更新惯性初始值
            {
                velocitySmoothDamp = velocity;
            }
            RotateTarget(velocity);
        }
        else if (Input.GetMouseButtonUp(0)) // 鼠标左键松开
        {
            isDragging = false; // 停止拖拽，进入惯性模式
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1) // 单指触摸：旋转
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                previousTouchPosition = touch.position;
                isDragging = true;
            }
            else if (touch.phase == TouchPhase.Moved && isDragging)
            {
                touchDelta = touch.position - previousTouchPosition;
                previousTouchPosition = touch.position;

                // 计算旋转速度
                velocity = (touchDelta / this.screenFactor) * rotationSpeed * Time.deltaTime;
                velocitySmoothDamp = velocity;
                RotateTarget(velocity);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
        else if (Input.touchCount == 2) // 双指触摸：缩放 & 平移
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1Pos = touch1.position;
            Vector2 touch2Pos = touch2.position;

            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                // 初始化双指操作
                initialTouchDistance = touch1Pos - touch2Pos;
                initialScale = _modelGameObject.transform.localScale.x;
                previousPanPosition = (touch1Pos + touch2Pos) / 2;
                initialModelPosition = _modelGameObject.transform.position;
                isPinching = true;
                isPanning = true;
            }
            else if (isPinching || isPanning)
            {
                // 缩放处理
                Vector2 currentTouchDistance = touch1Pos - touch2Pos;
                float scaleFactor = currentTouchDistance.magnitude / initialTouchDistance.magnitude;
                
                Vector3 newScale = Vector3.one * initialScale * scaleFactor;
                _modelGameObject.transform.localScale = new Vector3(
                    Mathf.Clamp(newScale.x, defaultScale * 0.5f, defaultScale * 5.0f),
                    Mathf.Clamp(newScale.y, defaultScale * 0.5f, defaultScale * 5.0f),
                    Mathf.Clamp(newScale.z, defaultScale * 0.5f, defaultScale * 5.0f));

                // 平移处理
                Vector2 currentPanPosition = (touch1Pos + touch2Pos) / 2;
                Vector2 panDelta = currentPanPosition - previousPanPosition;
                previousPanPosition = currentPanPosition;

                // 将屏幕空间的拖动量转换为世界空间
                Vector3 worldDelta = new Vector3(panDelta.x, panDelta.y, 0) * 0.01f; // 调整系数
                _modelGameObject.transform.position += worldDelta;
            }
        }
        else
        {
            isPinching = false;
            isPanning = false;
        }
    }

    void RotateTarget(Vector2 delta)
    {
        if (_modelGameObject != null)
        {
            float rotationX = delta.y;
            float rotationY = -delta.x;

            // 在世界空间中旋转模型
            _modelGameObject.transform.Rotate(rotationX, rotationY, 0, Space.World);
        }
    }
}
