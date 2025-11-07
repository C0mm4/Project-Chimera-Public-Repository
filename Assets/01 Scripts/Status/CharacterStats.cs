using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    [SerializeField] protected BaseStatusSO originData;

    protected AIControllerBase aiController;

    [SerializeField] public StatusData data;
    public Renderer model;
    [SerializeField] protected Color hitColor = Color.red;
    public event Action<float> OnHealthChanged;
    public event Action OnDeath;


    public GaugeBarUI gaugebarUI;

    protected Coroutine hitCoroutine;

    [SerializeField] private float hitEffectTime = 1f;

    protected virtual void Awake()
    {
        aiController = GetComponent<AIControllerBase>();
        
        if(originData != null)
        {
            data.maxHealth = originData.maxHealth;

            data.currentHealth = data.maxHealth;
            data.moveSpeed = originData.moveSpeed;

        }

        
    }

    protected async virtual void OnEnable()
    {
        if (gaugebarUI == null)
        {
            ObjectPoolManager.Instance.CreatePool("Pref_110000", transform);

            GameObject obj = await ObjectPoolManager.Instance.GetPool("Pref_110000", transform);
            gaugebarUI = obj.GetComponent<GaugeBarUI>();
        }

        // 게이지 바 로딩 중 비활성화되면 끊도록
        if (!gameObject.activeSelf) return;
        if (gaugebarUI != null)
        {
            gaugebarUI.SetFillPercent(1f);
            OnHealthChanged += gaugebarUI.SetFillPercent;
        }

        StageManager.Instance.OnStageClear -= Revive;
        StageManager.Instance.OnStageClear += Revive;
        StageManager.Instance.OnStageFail -= Revive;
        StageManager.Instance.OnStageFail += Revive;

        if(model != null)
        {
            var skinnedRenderer = model.GetComponent<SkinnedMeshRenderer>();
            if(skinnedRenderer != null)
            {
                var materials = skinnedRenderer.materials;
                // 원래 색으로 복원
                for (int i = 0; i < materials.Length; i++)
                    materials[i].color = Color.white;
            }

        }
    }

    private void OnDisable()
    {
        StageManager.Instance.OnStageFail -= Revive;
        StageManager.Instance.OnStageClear -= Revive;
    }

    protected void Heal()
    {
        data.currentHealth = data.maxHealth;
    }

    protected virtual void Revive()
    {
        Heal();
    }


    public void TakeDamage(Transform instigator, float damageAmount)
    {
        BaseWeapon baseWeapon = instigator.GetComponentInChildren<BaseWeapon>();
        if (baseWeapon != null)
        {
            SoundManager.Instance.PlaySFXRandom(baseWeapon.GetWeaponData().impact_fx);
        }
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);

        }

        //        Debug.Log(data);
        //        if (data == null) return;
        data.currentHealth -= damageAmount;
        data.currentHealth = Mathf.Clamp(data.currentHealth, 0, data.maxHealth);

        // Debug.Log("때리는 주체 : " + instigator);
        // Debug.Log(data.currentHealth);

        float percent = data.currentHealth / data.maxHealth;
        OnHealthChanged?.Invoke(percent);
        //데미지를 받을때 여기밖에 안거치는거같음
        // UpdateHealthUI(percent);
        
        if (gameObject.CompareTag("Player") && PlayerController.Instance != null)
        {
            // PlayerController의 사운드 재생 함수를 호출
            PlayerController.Instance.PlayHitSound();
        }
        
        if (aiController != null)
        {
            aiController.OnHit(instigator);
        }

        if(data.currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Death();
        }
        else
        {
            hitCoroutine = StartCoroutine(HitEffect(hitEffectTime));
        }
    }
    protected virtual IEnumerator HitEffect(float duration)
    {
        var skinnedRenderer = model.GetComponent<SkinnedMeshRenderer>();
        if (skinnedRenderer == null)
            yield break;

        var materials = skinnedRenderer.materials;


        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = hitColor;
        }

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float lerp = Mathf.Clamp01(time / duration);
            Color currentColor = Color.Lerp(hitColor, Color.white, lerp);

            for (int i = 0; i < materials.Length; i++)
                materials[i].color = currentColor;

            yield return null;
        }

        // 원래 색으로 복원
        for (int i = 0; i < materials.Length; i++)
            materials[i].color = Color.white;

        
    }

    protected virtual void Death()
    {
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);

        }
        if(model != null)
        {
            var skinnedRenderer = model.GetComponent<SkinnedMeshRenderer>();

            if (skinnedRenderer == null)
                return;

            var materials = skinnedRenderer.materials;
            for (int i = 0; i < materials.Length; i++)
                materials[i].color = Color.white;
        }

    }
    private void UpdateHealthUI(float percent)
    {
        gaugebarUI.SetFillPercent(percent);
    }

    public virtual void SetLevelStatus(int level)
    {

    }
}

[Serializable]
public struct StatusData
{
    public float currentHealth;
    public float maxHealth;

    public float moveSpeed;
}