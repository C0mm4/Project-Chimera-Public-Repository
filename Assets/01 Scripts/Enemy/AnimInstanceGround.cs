using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimInstanceGround : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform model;

    public bool IsMoving { get; private set; }

    private async void Awake()
    {
        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();
        if (model == null)
        {
            model = transform.GetChild(0);
        }

        if (model.childCount == 0)
        {
            await ResourceManager.Instance.Create<GameObject>("Character/Range01", model);
        }
    }

    private void Update()
    {
        animator.SetBool(Animator.StringToHash("IsMoving"), IsMoving);
        

    }

    private void LateUpdate()
    {
        IsMoving = agent.velocity.sqrMagnitude > 0.01f;
    }

    public void PlayAttackAnimation()
    {
        animator.SetTrigger("Attack");

    }

    public void PlayDieAnim()
    {
        animator.SetTrigger(Animator.StringToHash("Die"));
        // 코루틴으로 
    }

    public void PlayHitAnim()
    {
        animator.SetTrigger(Animator.StringToHash("Hit"));

    }
}
