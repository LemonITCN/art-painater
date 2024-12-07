using System.Collections;
using System.Collections.Generic;
using UniGLTF;
using UnityEngine;

public class PainterD3SceneController : MonoBehaviour
{
    private RuntimeGltfInstance _runtimeGltfInstance;
    private GameObject _modelGameObject;
    
    void Start()
    {
        GltfData gltfData = new GlbFileParser(Application.streamingAssetsPath + "/Models/1.glb").Parse();
        var loader = new ImporterContext(gltfData);
        _runtimeGltfInstance = loader.Load();
        _modelGameObject = _runtimeGltfInstance.gameObject;
        _runtimeGltfInstance.EnableUpdateWhenOffscreen();
        _runtimeGltfInstance.ShowMeshes();
        _runtimeGltfInstance.transform.localPosition = new Vector3(0, 0, 0);
        _runtimeGltfInstance.transform.localScale = new Vector3(1f, 1f, 1f);
        Debug.Log("3D模型加载成功");
    }

    void Update()
    {
        
    }
}
