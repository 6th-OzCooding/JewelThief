using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager
{
    Dictionary<string, AsyncOperationHandle> _handles = new();
    string[] preLoadAddresses = { };

    public async UniTask Init(System.Action<float> onProgress = null)
    {
        // TODO(김익환 2026-06-14): 크기가 큰 에셋들은 미리 로드하기 - audio, material, mesh, texture 등
        // preLoadAddresses에 미리 로드될 address 넣기
        int totalCount = preLoadAddresses.Length;

        if(totalCount == 0)
        {
            onProgress?.Invoke(1f);
            return;
        }

        for(int i = 0; i < totalCount; i++)
        {
            string address = preLoadAddresses[i];

            await PreLoadAssetAsync(address, i, totalCount, onProgress);

            float progress = (i + 1) / (float)totalCount;
            onProgress?.Invoke(progress);
        }

        onProgress?.Invoke(1f);
    }

    public async UniTask<T> LoadAssetAsync<T>(string address) where T : Object
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
            Debug.LogError($"에셋 로드 실패: {address}, Exception: {ex}");

            if (loadHandle.IsValid())
                Addressables.Release(loadHandle);

            return null;
        }
    }

    public T GetLoadedAsset<T>(string address) where T : Object
    {
        if(!_handles.TryGetValue(address, out AsyncOperationHandle handle))
        {
            Debug.LogError($"로드되지 않은 에셋입니다: {address}");
            return null;
        }

        if(!handle.IsValid())
        {
            Debug.LogError($"유효하지 않은 에셋 핸들입니다: {address}");
            return null;
        }

        if(handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"에셋 로드가 완료되지 않았거나 실패한 에셋입니다: {address}");
            return null;
        }

        T asset = handle.Result as T;

        if (null == asset)
        {
            Debug.LogError($"에셋 타입이 일치하지 않습니다: {address}");
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

    private async UniTask PreLoadAssetAsync(string address, int loadedIndex, int totalCount, System.Action<float> onProgress)
    {
        if(_handles.TryGetValue(address, out AsyncOperationHandle cacedHandle))
        {
            if (cacedHandle.IsValid() && cacedHandle.Status == AsyncOperationStatus.Succeeded)
                return;

            if(cacedHandle.IsValid())
                Addressables.Release(cacedHandle);

            _handles.Remove(address);
        }

        AsyncOperationHandle<Object> loadHandle = Addressables.LoadAssetAsync<Object>(address);

        while(!loadHandle.IsDone)
        {
            float currentProgress = loadHandle.PercentComplete;
            float progress = (loadedIndex + currentProgress) / totalCount;

            onProgress?.Invoke(progress);

            await UniTask.Yield();
        }

        if(loadHandle.Status == AsyncOperationStatus.Succeeded)
        {
            _handles[address] = loadHandle;
        }
        else
        {
            Debug.LogError($"에셋 로드 실패: {address}, Exection: {loadHandle.OperationException}");

            if (loadHandle.IsValid())
                Addressables.Release(loadHandle);
        }
    }
}
