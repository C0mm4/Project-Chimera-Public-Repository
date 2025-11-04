using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ConstructureModel : MonoBehaviour
{
    private Renderer model;
    public float modelY;
    public float modelZ;

    [SerializeField] public Vector3 modelScale;
    [SerializeField] private List<NavMeshObstacle> obstacles = new();
    private Color[] materialColors;

    public void Initialize(bool isGhost = false)
    {
        if (!isGhost)
        {
            if (obstacles.Count == 0)
            {
                var objs = GetComponent<NavMeshObstacle>();
                if(objs != null)
                {
                    obstacles.Add(objs);
                }
                else
                {
                    obstacles.Add(gameObject.AddComponent<NavMeshObstacle>());
                }
            }

            foreach(NavMeshObstacle obstacle in obstacles)
            {
                obstacle.carving = true;
                obstacle.enabled = true;
            }
        }
        else
        {
            foreach (NavMeshObstacle obstacle in obstacles)
            {
                obstacle.enabled = false;
            }
        }
        modelScale = Vector3.Max(modelScale, Vector3.one);
        transform.localScale = modelScale;
        model = GetComponentInChildren<Renderer>();
        var bounds = model.bounds;
        modelZ = bounds.size.z;
        modelY = bounds.size.y;

        materialColors = new Color[model.materials.Length];
        for(int i = 0; i <  model.materials.Length; i++)
        {
            materialColors[i] = model.materials[i].color;
        }
    }

    public void SetActive()
    {
        for (int i = 0; i < model.materials.Length; i++)
        {
            model.materials[i].color = materialColors[i];
        }
        foreach (NavMeshObstacle objstacle in obstacles)
        {
            objstacle.enabled = true;
        }
    }

    public void SetDeActive()
    {
        for (int i = 0; i < model.materials.Length; i++)
        {
            model.materials[i].color = Color.red;
        }
        foreach(NavMeshObstacle objstacle in obstacles)
        {
            objstacle.enabled = false;
        }
    }

    public IEnumerator HitEffect(float duration, Color targetColor)
    {
        for(int i = 0; i < model.materials.Length; i++)
        {
            model.materials[i].color = targetColor;
        }

        float time = 0;
        while(time < duration)
        {
            time += Time.deltaTime;
            for(int i = 0; i < model.materials.Length; i++)
            {
                model.materials[i].color = Color.Lerp(targetColor, materialColors[i], time / duration);
            }
            yield return null;
        }

        for(int i = 0; i < model.materials.Length; i++)
        {
            model.materials[i].color = materialColors[i];
        }
    }
}
