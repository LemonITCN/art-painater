using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Utils;

namespace Service
{
    public class LocalModelLibraryManager : MonoBehaviour
    {
        // 下载任意资源文件
        public void DownloadResource(string resUrl, Action<string> onComplete)
        {
            LogUtil.Info("开始准备下载资源:" + resUrl);
            string[] parts = resUrl.Split('/');
            string fileNameWithExtension = parts[parts.Length - 1];
            string[] fileNameParts = fileNameWithExtension.Split('.');
            string fileName = fileNameParts[0];
            string fileExtension = fileNameParts[1];
            var localDirPath = Path.Combine(Application.persistentDataPath, "ModelLibrary",
                fileName + "." + fileExtension);
            StartCoroutine(DownloadFileAsync(resUrl, localDirPath,
                () => { onComplete.Invoke(localDirPath); }));
        }


        // 下载文件异步
        IEnumerator DownloadFileAsync(string url, string path, Action onSuccess)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            if (!File.Exists(path))
            {
                LogUtil.Info("开始下载文件Async:" + url);
                string directoryPath = Path.GetDirectoryName(path);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);  // 创建目录
                    LogUtil.Info("文件夹不存在，已创建：" + directoryPath);
                }
                UnityWebRequest www = UnityWebRequest.Get(url);
                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError ||
                    www.result == UnityWebRequest.Result.ProtocolError)
                {
                    LogUtil.Error("文件下载出错Async: " + url);
                }
                else
                {
                    File.WriteAllBytes(path, www.downloadHandler.data);
                    LogUtil.Info("文件下载成功Async:" + path);
                    onSuccess.Invoke();
                }
            }
            else
            {
                LogUtil.Info("文件在本地已存在:" + path);
                onSuccess.Invoke();
            }
        }
    }
}