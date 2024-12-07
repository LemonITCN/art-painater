using System;
using System.Collections;
using System.Collections.Generic;
using Service;
using UniGLTF;
using UnityEngine;

public class PainterD3SceneController : MonoBehaviour
{
    private RuntimeGltfInstance _runtimeGltfInstance;
    private GameObject _modelGameObject;
    private Vector2 previousTouchPosition; // 上一次触摸位置
    private Vector2 touchDelta; // 当前的触摸/鼠标移动量
    private bool isDragging = false; // 是否正在拖拽
    private readonly float rotationSpeed = 4f; // 旋转速度
    private readonly float inertiaFactor = 2f; // 惯性系数（阻尼效果）
    private float modelSize = 12f;
    private Vector2 velocity; // 旋转速度
    private Vector2 velocitySmoothDamp; // 平滑的旋转速度
    private readonly float screenFactor = Screen.width * 0.001f;

    private LocalModelLibraryManager _localModelLibraryManager;
    
    void Start()
    {
        _localModelLibraryManager = gameObject.AddComponent<LocalModelLibraryManager>();
        _localModelLibraryManager.DownloadResource("https://common-resource-1302067637.cos.ap-beijing.myqcloud.com/tyrannosaurus-rex.glb",
            resPath =>
            {
                GltfData gltfData = new GlbFileParser(resPath).Parse();
                var loader = new ImporterContext(gltfData);
                _runtimeGltfInstance = loader.Load();
                _modelGameObject = _runtimeGltfInstance.gameObject;
                _runtimeGltfInstance.EnableUpdateWhenOffscreen();
                _runtimeGltfInstance.ShowMeshes();
                _runtimeGltfInstance.transform.localPosition = new Vector3(0, 0, 0);
                // _runtimeGltfInstance.transform.localScale = new Vector3(.3f, .3f, .3f);
                // Collider collider = _modelGameObject.gameObject.GetComponent<Collider>();
                // Vector3 size = collider.bounds.size;
                // Debug.Log($"Object size: Width = {size.x}m, Height = {size.y}m, Depth = {size.z}m");
                Vector3 size = GetObjectSize(_modelGameObject);
                float maxDirection = Math.Max(size.x, Math.Max(size.y, size.z));
                _modelGameObject.transform.localScale = new Vector3(modelSize / maxDirection, modelSize / maxDirection, modelSize / maxDirection);
                Debug.Log("3D模型加载成功" + size);
            });
    }
    
    Vector3 GetObjectSize(GameObject obj)
    {
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            return renderer.bounds.size;
        }
        else
        {
            Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
            if (childRenderers.Length > 0)
            {
                Bounds combinedBounds = childRenderers[0].bounds;
                foreach (Renderer childRenderer in childRenderers)
                {
                    combinedBounds.Encapsulate(childRenderer.bounds);
                }
                return combinedBounds.size;
            }
        }

        Debug.LogWarning("No Renderer found on the GameObject or its children.");
        return Vector3.zero;
    }
    
    void Update()
    {
        #if UNITY_EDITOR // 如果在编辑器中运行，使用鼠标输入模拟触摸
        HandleMouseInput();
        #else // 如果在真实设备上运行，使用触摸输入
        HandleTouchInput();
        #endif

        // 应用惯性旋转
        if (!isDragging)
        {
            // 用平滑阻尼来模拟惯性效果，使速度逐渐减小
            velocitySmoothDamp = Vector2.Lerp(velocitySmoothDamp, Vector2.zero, inertiaFactor * Time.deltaTime);
            RotateTarget(velocitySmoothDamp);

            // 如果惯性速度小到一定程度，就停止旋转
            if (velocitySmoothDamp.magnitude < 0.1f)
            {
                velocitySmoothDamp = Vector2.zero;
            }
        }
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 1) // 单指触摸
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

                // 计算旋转速度（触摸移动量）
                Debug.Log($"speed: {rotationSpeed}");
                velocity = (touchDelta / this.screenFactor) * rotationSpeed * Time.deltaTime;
                velocitySmoothDamp = velocity; // 将当前速度设置为平滑速度的起始值
                RotateTarget(velocity);
            }
            else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                isDragging = false;
            }
        }
    }

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

            // 计算旋转速度（鼠标移动量）
            velocity = touchDelta * rotationSpeed * Time.deltaTime;
            velocitySmoothDamp = velocity; // 将当前速度设置为平滑速度的起始值
            RotateTarget(velocity);
        }
        else if (Input.GetMouseButtonUp(0)) // 鼠标左键松开
        {
            isDragging = false;
        }
    }

    void RotateTarget(Vector2 delta)
    {
        if (_modelGameObject != null)
        {
            float rotationX = delta.y;
            float rotationY = -delta.x;

            // 在世界空间中进行旋转
            _modelGameObject.transform.Rotate(rotationX, rotationY, 0, Space.World);
        }
    }
}
