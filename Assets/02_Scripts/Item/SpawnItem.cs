using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnItem : MonoBehaviour ///테스트용
{
    [SerializeField] private GameObject genericItemPrefab;
    // 인스펙터에서 직접 등록할 메쉬와 마테리얼 리스트
    [SerializeField] private List<Mesh> testMeshList = new();
    [SerializeField] private List<Material> testMaterialList = new();
    [SerializeField] private List<Material> testMaterialListCash = new();
    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.digit1Key.wasPressedThisFrame) TestJewelSpawner(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) TestJewelSpawner(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) TestJewelSpawner(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) TestJewelSpawner(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) TestCashSpawner(4);
        //if (Keyboard.current.digit6Key.wasPressedThisFrame) TestAsyncSpawner("Item/model/Crobar[Crowbar]", "Item/model/Crobar[SimpleItems]").Forget();
    }

    void TestJewelSpawner(int index)
    {
        GameObject spawnedObj = Instantiate(genericItemPrefab, transform.position, Quaternion.identity);
        MeshFilter meshFilter = spawnedObj.GetComponentInChildren<MeshFilter>();
        MeshRenderer meshRenderer = spawnedObj.GetComponentInChildren<MeshRenderer>();
        MeshCollider meshCollider = spawnedObj.GetComponentInChildren<MeshCollider>();
        if (meshFilter != null && meshRenderer != null && meshCollider != null)
        {
            Mesh targetMesh = testMeshList[index];
            meshFilter.mesh = testMeshList[index];
            meshRenderer.material = testMaterialList[index];
            meshCollider.sharedMesh = targetMesh;
            meshCollider.convex = true;

            Debug.Log($"[테스트 스폰 성공] {index}번 메쉬('{testMeshList[index].name}')로 생성되었습니다.");
        }
    }
    void TestCashSpawner(int index)
    {
        GameObject spawnedObj = Instantiate(genericItemPrefab, transform.position, Quaternion.identity);
        MeshFilter meshFilter = spawnedObj.GetComponentInChildren<MeshFilter>();
        MeshRenderer meshRenderer = spawnedObj.GetComponentInChildren<MeshRenderer>();
        MeshCollider meshCollider = spawnedObj.GetComponentInChildren<MeshCollider>();
        if (meshFilter != null && meshRenderer != null && meshCollider != null)
        {
            Mesh targetMesh = testMeshList[index];
            meshFilter.mesh = testMeshList[index];
           // meshRenderer.material = testMaterialList[index];
            meshCollider.sharedMesh = targetMesh;
            meshCollider.convex = true;

            Debug.Log($"[테스트 스폰 성공] {index}번 메쉬('{testMeshList[index].name}')로 생성되었습니다.");
        }
        if (testMaterialListCash != null && testMaterialListCash.Count > 0)
        {
            
            Material[] targetMaterials = new Material[testMaterialListCash.Count];

            for (int i = 0; i < testMaterialListCash.Count; i++)
            {
                Material mat = testMaterialListCash[i];

                if (mat != null)
                {
                    targetMaterials[i] = mat;
                }
                else
                {
                    Debug.LogError($"마테리얼 로드 실패 [{i}]: ");
                }
            }

            meshRenderer.sharedMaterials = targetMaterials;
        }
    }
    async UniTask TestAsyncSpawner(string meshAddress, string matAddress)
    {
       
        GameObject spawnedObj = Instantiate(genericItemPrefab, transform.position, Quaternion.identity);
      
        MeshFilter meshFilter = spawnedObj.GetComponentInChildren<MeshFilter>();
        MeshRenderer meshRenderer = spawnedObj.GetComponentInChildren<MeshRenderer>();
        MeshCollider meshCollider = spawnedObj.GetComponentInChildren<MeshCollider>();

      
        Mesh targetMesh = await GameManager.Resource.LoadAssetAsync<Mesh>(meshAddress);
        Material targetMaterial = await GameManager.Resource.LoadAssetAsync<Material>(matAddress);

       
        if (targetMesh != null)
        {
            meshFilter.sharedMesh = targetMesh;

            if (meshCollider != null)
            {
                meshCollider.sharedMesh = targetMesh;
                meshCollider.convex = true;
            }
        }
        else
        {
            Debug.LogError($"메쉬 로드 실패: {meshAddress}");
        }

        if (targetMaterial != null)
        {
            // 마찬가지로 최적화를 위해 .material 대신 .sharedMaterial을 사용합니다.
            meshRenderer.sharedMaterial = targetMaterial;
        }
        else
        {
            Debug.LogError($"마테리얼 로드 실패: {matAddress}");
        }
    }
}
