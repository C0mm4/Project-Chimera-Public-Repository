using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChainLightning : BaseWeapon
{
    [SerializeField] int transitionCount = 2;
    [SerializeField] int segmentCount = 10;
    [SerializeField] float transitionDelay = 0.5f;
    [SerializeField] float nextTargetDetectRange = 2f;
    [SerializeField] LayerMask targetLayerMask;
    [SerializeField] float lightningNoise = 0.5f;
    [SerializeField] Transform testTarget;

    Dictionary<Transform, bool> prevTarget = new();

    Collider[] overlaps = new Collider[10];
    WaitForSeconds waitForTransition;

    LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;

        waitForTransition = new WaitForSeconds(transitionDelay);
        
    }

    protected override void PerformAttack(Transform target)
    {
        base.PerformAttack(target);
        StartCoroutine(ChannelLightning(target));
    }

    private Transform FindNextTarget(Transform currentTarget)
    {
        int count = Physics.OverlapSphereNonAlloc(currentTarget.position, nextTargetDetectRange, overlaps, targetLayerMask);

        if (count < 1) // 다음 타겟 못 찾음 (공격 종료)
        {
            return null;
        }

        float minDist = 12345;
        int minIdx = -1;

        for (int i = 0; i < count; i++)
        {
            if (prevTarget.ContainsKey(overlaps[i].transform)) continue;

            float dist = Vector3.SqrMagnitude(overlaps[i].transform.position - currentTarget.position);
            if (dist < minDist)
            {
                minDist = dist;
                minIdx = i;
            }
        }

        if (minIdx < 0) return null;

        prevTarget.Add(overlaps[minIdx].transform, true);
        return overlaps[minIdx].transform;
    }

    private void DrawLightning(Vector3 start, Vector3 end)
    {
        lineRenderer.positionCount = segmentCount + 1;

        lineRenderer.SetPosition(0, start);
        lineRenderer.SetPosition(segmentCount, end);

        for (int i = 0; i <= segmentCount; ++i)
        {
            if (i == 0 || i == segmentCount) continue;

            float t = (float)i / segmentCount;
            Vector3 pos = Vector3.Lerp(start, end, t);

            pos += Random.insideUnitSphere * lightningNoise;

            lineRenderer.SetPosition(i, pos);
        }

    }

    IEnumerator ChannelLightning(Transform target)
    {
        prevTarget.Clear();
        prevTarget.Add(target, true);

        Transform current = transform;
        Transform next = target;
        if (target == null) yield break;

        DrawLightning(current.position, next.position);
        lineRenderer.enabled = true;
        target.GetComponent<CharacterStats>().TakeDamage(InstigatorTrans, weaponData.Damage);
        yield return waitForTransition;
        lineRenderer.enabled = false;

        for (int i = 0; i < transitionCount; ++i)
        {
            current = next;
            next = FindNextTarget(current);
            if (next == null) yield break;

            DrawLightning(current.position, next.position);
            lineRenderer.enabled = true;
            next.GetComponent<CharacterStats>().TakeDamage(InstigatorTrans, weaponData.Damage);
            yield return waitForTransition;
            lineRenderer.enabled = false;
        }

    }

}
