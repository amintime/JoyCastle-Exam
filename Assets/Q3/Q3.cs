using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

/**
**该题目校招岗位可以不作答，社招需要作答。**

按照要求在 {@link Q3.onStartBtnClick} 中编写一段异步任务处理逻辑，具体执行步骤如下：
1. 调用 {@link Q3.loadConfig} 加载配置文件，获取资源列表
2. 根据资源列表调用 {@link Q3.loadFile} 加载资源文件
3. 资源列表中的所有文件加载完毕后，调用 {@link Q3.initSystem} 进行系统初始化
4. 系统初始化完成后，打印日志

附加要求
1. 加载文件时，需要做并发控制，最多并发 3 个文件
2. 加载文件时，需要添加超时控制，超时时间为 3 秒
3. 加载文件失败时，需要对单文件做 backoff retry 处理，重试次数为 3 次
4. 对错误进行捕获并打印输出
*/

public class Q3 : MonoBehaviour
{
    // 并发文件数量
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(3, 3);
    // 超时时间
    private const int FILE_LOAD_TIME_OUT = 3000;
    // 单文件最大重试数
    private const int MAX_RETRY_COUNT = 3;


    public async void OnStartBtnClick()
    {
        // TODO: 请在此处开始作答
        try
        {
            string[] fileList = await LoadConfig();
            Debug.Log($"hat 配置文件加载成功，共有 {fileList.Length} 个文件");

            var loadTasks = new List<Task>();
            foreach(var file in fileList)
            {
                string currentFile = file;
                loadTasks.Add(LoadFileWithRetryAndTimeOut(currentFile));
            }

            // 等待所有文件加载
            await Task.WhenAll(loadTasks);
            Debug.Log("hat 所有文件加载完毕");

            // 系统初始化
            await InitSystem();
            Debug.Log("hat 系统初始化完毕");
        }
        catch (Exception e)
        {
            Debug.LogError($"hat 异步加载时报错 : {e.Message}");
        }
    }

    // 带并发控制、超时的单文件加载方法
    private async Task LoadFileWithRetryAndTimeOut(string file)
    {
        int retryCount = 0;
        while(retryCount < MAX_RETRY_COUNT)
        {
            await _semaphore.WaitAsync();
            try
            {
                // 启动一个3s的Task，用于判断loadTask是否超时
                var loadTask = LoadFile(file);
                var timeoutTask = Task.Run(() => Task.Delay(FILE_LOAD_TIME_OUT));

                var completedTask = await Task.WhenAny(loadTask, timeoutTask);

                if(completedTask == timeoutTask)
                {
                    // 超时
                    throw new TimeoutException($"文件 {file} 加载超时");
                }

                await loadTask;
                // 加载成功，退出循环
                return;
            }
            catch(Exception e)
            {
                retryCount++;
                if(retryCount >= MAX_RETRY_COUNT)
                {
                    Debug.LogError($"hat 文件 {file} 加载失败 : {e.Message}");
                    throw;
                }
                else
                {
                    // 过一段时间后重试
                    int delayMS = (int)Math.Pow(2, retryCount) * 1000;
                    Debug.LogWarning($"hat 文件 {file} 第 {retryCount} 次加载失败 : {e.Message}");
                    await Task.Delay(delayMS);
                }
            }
            finally
            {
                // 释放信号量
                _semaphore.Release();
            }
        }
    }

    // #region 以下是辅助测试题而写的一些 mock 函数，请勿修改

    /// <summary>
    /// 加载配置文件
    /// </summary>
    /// <returns>文件列表</returns>
    public async Task<string[]> LoadConfig()
    {
        Debug.Log("load config start");
        await Task.Delay(1000);
        if (Random.value > 0.01f)
        {
            Debug.Log("load config success");
            string[] files = new string[100];
            for (int i = 0; i < 100; i++)
            {
                files[i] = $"file-{i}";
            }
            return files;
        }
        else
        {
            Debug.Log("load config failed");
            throw new System.Exception("Load config failed");
        }
    }

    /// <summary>
    /// 加载文件
    /// </summary>
    /// <param name="file">文件名</param>
    /// <returns></returns>
    public async Task LoadFile(string file)
    {
        Debug.Log($"load file start: {file}");
        await Task.Delay(Random.Range(1000, 5000));
        if (Random.value > 0.01f)
        {
            Debug.Log($"load file success: {file}");
        }
        else
        {
            Debug.Log($"load file failed: {file}");
            throw new System.Exception($"Load file failed: {file}");
        }
    }

    /// <summary>
    /// 初始化系统
    /// </summary>
    /// <returns></returns>
    public async Task InitSystem()
    {
        Debug.Log("init system start");
        await Task.Delay(1000);
        Debug.Log("init system success");
    }

    // #endregion
}
