using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingAIController : AIControllerBase
{
    CharacterController characterController;
    [SerializeField] private float flyDist = 5f;
    RaycastHit groundHit = new();

    protected override void ChaseTarget()
    {
        
        Vector3 dir = Target.position - transform.position;
        dir.y = 0;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = rot;
        
        if (!shouldStop)
        {
            characterController.Move(dir.normalized * 3f * Time.fixedDeltaTime);

        }

        Debug.DrawRay(transform.position, dir);
    }
   

    protected override void Awake()
    {
        base.Awake();
        
        characterController = GetComponent<CharacterController>();
       
    }

    protected override void Update()
    {
        base.Update();

        //transform.position = new Vector3(transform.position.x, 3f, transform.position.z);

    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();


        // Terrain이면 사용 가능 Mesh면 사용 불가능 이건 맵 결정되고 봐야할듯
        var h = Terrain.activeTerrain.SampleHeight(transform.position);
        transform.position = new Vector3(transform.position.x, h + flyDist, transform.position.z);
        /*
        Physics.Raycast(transform.position, Vector3.down, out groundHit, 10f, LayerMask.GetMask("Ground"));
        if (groundHit.collider)
        {
            Debug.Log(groundHit.point.y);
            transform.position = new Vector3(transform.position.x, groundHit.point.y + 3f, transform.position.z);
        }*/
    }

    protected override bool IsAttackable()
    {
        if (!base.IsAttackable())
        {
            return false;
        }

        Vector3 groundPosition = transform.position;
        groundPosition.y = 0;

        LayerMask targetLayer = LayerMask.GetMask(LayerMask.LayerToName(Target.gameObject.layer));
        int count = Physics.OverlapSphereNonAlloc(groundPosition, AttackRange, overlaps, targetLayer);
        if (count == 0) return false;

        for (int i = 0; i < count; i++)
        {
            if (overlaps[i].transform == Target)
                return true;
        }
        return false;

    }


    protected override void TryAttack()
    {

/*        Vector3 groundPosition = transform.position;
        groundPosition.y = 0;

        LayerMask targetLayer = LayerMask.GetMask(LayerMask.LayerToName(Target.gameObject.layer));

        int count = Physics.OverlapSphereNonAlloc(groundPosition, AttackRange, overlaps, targetLayer);

        if (count < 1) return;
*/
        if (weapon != null)
        {
            weapon.Attack(Target);
        }

        Debug.DrawRay(transform.position, Target.position - transform.position, Color.red, 3f);
        attackCoolDown = AttackCoolTime;
    }
}
