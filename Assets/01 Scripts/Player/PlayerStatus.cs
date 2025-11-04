using UnityEngine;

public class PlayerStatus : CharacterStats
{
    [SerializeField] private PlayerStatusSO statusData;
    [SerializeField] private PlayerController controller;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        GameManager.Instance.Player = this;
        StageManager.Instance.OnStageFail += OnStageFail;
        StageManager.Instance.OnStageClear += OnStageClear;
    }


    private void OnDisable()
    {
        StageManager.Instance.OnStageFail -= OnStageFail;
        StageManager.Instance.OnStageClear -= OnStageClear;

    }


    private void OnStageClear()
    {
        data.currentHealth = data.maxHealth;

    }


    private void OnStageFail()
    {
        data.currentHealth = data.maxHealth;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void Update()
    {
       
    }

    protected override void Death()
    {
        base.Death();

        if(controller != null)
        {
            controller.OnDeath();
        }

        StageManager.Instance.FailStage(false);
    }

}
