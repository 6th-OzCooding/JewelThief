using Cysharp.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class RunTimeBakeNavMesh
{
    private NavMeshSurface _navMeshSurface;

    public void Init(NavMeshSurface navMeshSurface)
    {
        _navMeshSurface = navMeshSurface;
    }

    public async UniTask BakeAfterMapGeneratedAsync()
    {
        await UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
        Debug.Log($"[NavMesh Debug] Bake 시작 직전. MapRoot 자식 개수: {_navMeshSurface.transform.childCount}");
        if (_navMeshSurface.navMeshData != null)
        {
            _navMeshSurface.RemoveData();
        }

        _navMeshSurface.BuildNavMesh();
        Debug.Log($"[NavMesh Debug] Bake 완료. navMeshData null 여부: {_navMeshSurface.navMeshData == null}");

        if (!HasValidNavMesh())
        {
            Debug.LogWarning("NavMesh가 생성되지 않았습니다.");
        }
    }

    private bool HasValidNavMesh()
    {
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
        return triangulation.vertices != null && triangulation.vertices.Length > 0;
    }
}
