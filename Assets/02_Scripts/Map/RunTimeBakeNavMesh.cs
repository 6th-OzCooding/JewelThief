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

        if (_navMeshSurface.navMeshData != null)
        {
            _navMeshSurface.RemoveData();
        }

        _navMeshSurface.BuildNavMesh();

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
