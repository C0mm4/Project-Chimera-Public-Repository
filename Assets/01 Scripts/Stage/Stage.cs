using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Stage : MonoBehaviour
{
    public Transform StructureTrans;
    public Transform StructureSeedTrans;
    public Transform ObjDropTrans;

    [SerializeField] NavMeshSurface playerSurface;
    [SerializeField] NavMeshSurface enemySurface;

    private Coroutine meshBuildCoroutine;

    private List<Wall> activateWallObjs = new();

    public SeedController seedController;

    private void Awake()
    {
        StageManager.Instance.Stage = this;

        ObjectPoolManager.Instance.CreatePool("GoldMining", StructureTrans);
        ObjectPoolManager.Instance.CreatePool("Tower", StructureTrans);
        ObjectPoolManager.Instance.CreatePool("Wall", StructureTrans);
        ObjectPoolManager.Instance.CreatePool("Barrack", StructureTrans);
    }

    private void Start()
    {
//        StartMeshBuild();
    }

    private void StartMeshBuild()
    {
        playerSurface.BuildNavMesh();
        enemySurface.BuildNavMesh();
    }

    public void WallEnable(Wall wall)
    {
        if (!activateWallObjs.Contains(wall))
        {
            activateWallObjs.Add(wall);
        }
    }

    public void WallDisable(Wall wall)
    {
        if (activateWallObjs.Contains(wall))
        {
            activateWallObjs.Remove(wall);
        }
    }
    public List<Wall> GetWalls()
    {
        return activateWallObjs;
    }
}
