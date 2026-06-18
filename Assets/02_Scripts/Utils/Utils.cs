using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using static PoolManager;

public static class Utils
{
    public static T GetOrAddComponent<T>(GameObject obj) where T : Component
    {
        if (null == obj) return null;

        T component = obj.GetComponent<T>();
        if (null == component) component = obj.AddComponent<T>();

        return component;
    }

    public static T GetOrAddComponentInChild<T>(GameObject obj, string name) where T : Component
    {
        if (null == obj) return null;

        T component = FindChild<T>(obj, name);
        if(null == component)
        {
            GameObject newGameObject = new GameObject(name);
            component = newGameObject.AddComponent<T>();
            newGameObject.transform.SetParent(obj.transform);
        }

        return component;
    }

    public static T FindChild<T>(GameObject obj, string name = null, bool recursive = false) where T : Object
    {
        if (null == obj) return null;

        if (!recursive)
        {
            Transform transform = obj.transform.Find(name);
            if (null != transform) return transform.GetComponent<T>();
        }
        else
        {
            foreach (T component in obj.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static async UniTaskVoid LoadAndPlayAudioClip(AudioSource audioSource, string path, bool isLoop = false)
    {
        AudioClip clip = await GameManager.Resource.LoadAssetAsync<AudioClip>(path);
        if (null == clip)
        {
            Debug.LogError($"오디오 클립 로드 실패: {path}");
            return;
        }

        if (isLoop)
        {
            audioSource.clip = clip;
            audioSource.loop = true;
            audioSource.Play();
        }
        else
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public static async UniTaskVoid LoadAndSetSprite(Image targetImage,string path)
    {
        targetImage.gameObject.SetActive(false);
        Sprite sprite = await GameManager.Resource.LoadAssetAsync<Sprite>(path);
        if (null == sprite)
        {
            Debug.LogError($"스프라이트 로드 실패: {path}");
            return;
        }
        targetImage.sprite = sprite;
        targetImage.gameObject.SetActive(true);
    }

    public static T ResourcesLoad<T>(string path) where T : Object
    {
        return Resources.Load<T>(path);
    }
}
