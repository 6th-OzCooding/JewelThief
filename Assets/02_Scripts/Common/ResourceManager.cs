using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager
{
    private const int MAX_LOAD_COUNT = 4;

    private Dictionary<string, AsyncOperationHandle> _handles = new();

    private int _progressCount = 0;

    public async UniTask Init(System.Action<float> onProgress = null)
    {
        _progressCount = 0; ;
        onProgress?.Invoke(0f);

        var dataTable = GameManager.DataTable.GetPreLoadAssetDataTable();

        int totalCount = dataTable.Count;

        if (dataTable.Count == 0)
        {
            onProgress?.Invoke(1f);
            return;
        }

        List<UniTask> loadTasks = new(totalCount);
        using SemaphoreSlim semaphore = new(MAX_LOAD_COUNT);


        foreach (PreLoadAssetData preLoadData in dataTable.Values)
        {
            loadTasks.Add(LoadWithSemaphoreAsync(preLoadData, semaphore, 
                () =>
                {
                    _progressCount++;
                    onProgress?.Invoke(_progressCount / (float)totalCount);
                }));
        }

        await UniTask.WhenAll(loadTasks);

        onProgress?.Invoke(1f);
    }

    private async UniTask LoadWithSemaphoreAsync(PreLoadAssetData preLoadData, SemaphoreSlim semaphore, System.Action onCompleted)
    {
        await semaphore.WaitAsync();

        try
        {
            switch (preLoadData.AssetType)
            {
                case "Mesh":
                    await PreLoadAssetAsync<Mesh>(preLoadData.Address);
                    break;

                case "Material":
                    await PreLoadAssetAsync<Material>(preLoadData.Address);
                    break;

                case "Prefab":
                case "GameObject":
                    await PreLoadAssetAsync<GameObject>(preLoadData.Address);
                    break;

                default:
                    await PreLoadAssetAsync<UnityEngine.Object>(preLoadData.Address);
                    break;
            }
        }
        finally
        {
            onCompleted?.Invoke();
            semaphore.Release();
        }
    }

    public async UniTask<T> LoadAssetAsync<T>(string address) where T : UnityEngine.Object
    {
        if (_handles.TryGetValue(address, out AsyncOperationHandle cachedHandle))
            return cachedHandle.Result as T;

        AsyncOperationHandle<T> loadHandle = Addressables.LoadAssetAsync<T>(address);

        try
        {
            T result = await loadHandle.Task;

            _handles[address] = loadHandle;
            return result;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"에셋 로드 실패: {address}, Exception: {ex}");

            if (loadHandle.IsValid())
                Addressables.Release(loadHandle);

            return null;
        }
    }


    public T GetLoadedAsset<T>(string address) where T : UnityEngine.Object
    {
        if (!_handles.TryGetValue(address, out AsyncOperationHandle handle))
        {
            Debug.LogWarning($"로드되지 않은 에셋입니다: {address}");
            return null;
        }

        if (!handle.IsValid())
        {
            Debug.LogWarning($"유효하지 않은 에셋 핸들입니다: {address}");
            return null;
        }

        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogWarning($"에셋 로드가 완료되지 않았거나 실패한 에셋입니다: {address}");
            return null;
        }

        if (handle.Result is not T asset)
        {
            Debug.LogWarning($"에셋 타입이 일치하지 않습니다: {address}");
            return null;
        }

        return asset;
    }

    public void Release(string address)
    {
        if (_handles.TryGetValue(address, out AsyncOperationHandle handle))
        {
            Addressables.Release(handle);
            _handles.Remove(address);
            Debug.Log($"에셋 메모리 해제 완료: {address}");
        }
    }

    public void ReleaseAll()
    {
        foreach (var handle in _handles.Values)
        {
            Addressables.Release(handle);
        }
        _handles.Clear();
        Debug.Log("모든 에셋 메모리 해제 완료");
    }

    private async UniTask PreLoadAssetAsync<T>(string address)
    {
        if (_handles.TryGetValue(address, out AsyncOperationHandle cacedHandle))
        {
            if (cacedHandle.IsValid() && cacedHandle.Status == AsyncOperationStatus.Succeeded)
                return;

            if (cacedHandle.IsValid())
                Addressables.Release(cacedHandle);

            _handles.Remove(address);
        }

        AsyncOperationHandle<T> loadHandle = Addressables.LoadAssetAsync<T>(address);

        try
        {
            T result = await loadHandle.Task;

            if (result == null)
            {
                Debug.LogWarning($"에셋 로드 결과가 null입니다: {address}");
                if (loadHandle.IsValid())
                    Addressables.Release(loadHandle);

                return;
            }

            _handles[address] = loadHandle;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"에셋 로드 실패: {address}, Exception: {ex}");

            if (loadHandle.IsValid())
                Addressables.Release(loadHandle);
        }
    }
}
