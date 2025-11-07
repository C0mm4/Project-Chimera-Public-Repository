using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class EnemyIndicator : MonoBehaviour
{
    private class IndicatorGroup
    {
        public string monsterName;
        public Vector2 direction;
        public int count;
        public GameObject indicator;
    }

    [SerializeField] private Transform monsterArrowBox;
    [SerializeField] private Camera cameras;

    private bool indicatorStart = false;

    private void Awake()
    {
        ObjectPoolManager.Instance.CreatePool("Pref_000000", monsterArrowBox);
    }

    private void FixedUpdate()
    {
        if (transform.childCount > 0 && !indicatorStart)
        {
            UpdateIndicators();
        }
        else if (transform.childCount == 0 && !indicatorStart)
        {
            StartIndicatorCleanup();
        }
    }

    private async void UpdateIndicators()
    {
        List<IndicatorGroup> groups = CreateIndicatorGroups();

        foreach (IndicatorGroup group in groups)
        {
            if (group.indicator == null)
            {
                group.indicator = await ObjectPoolManager.Instance.GetPool("Pref_000000", monsterArrowBox);
            }

            group.indicator.SetActive(true);
            PositionIndicator(group);
        }

        CleanupUnusedIndicators(groups);
    }

    private List<IndicatorGroup> CreateIndicatorGroups()
    {
        List<IndicatorGroup> groups = new();

        foreach (Transform child in transform)
        {
            GameObject monster = child.gameObject;
            if (monster == null || !monster.activeInHierarchy)
                continue;

            Vector3 viewportPos = cameras.WorldToViewportPoint(monster.transform.position);
            if (IsInView(viewportPos))
                continue;

            string monsterName = GetName(monster.transform);
            Vector2 direction = GetDirection(viewportPos);

            bool merged = false;
            foreach (IndicatorGroup group in groups)
            {
                if (group.monsterName == monsterName &&
                    Vector2.Angle(group.direction, direction) < 15f)
                {
                    group.direction = (group.direction + direction).normalized;
                    group.count++;
                    merged = true;
                    break;
                }
            }

            if (!merged)
            {
                groups.Add(new IndicatorGroup
                {
                    monsterName = monsterName,
                    direction = direction,
                    count = 1
                });
            }
        }

        return groups;
    }

    private void PositionIndicator(IndicatorGroup group)
    {
        Vector2 screenCenter = new(Screen.width / 2f, Screen.height / 2f);
        float offset = 50f;

        Vector2 indicatorScreenPos = screenCenter + group.direction * (Mathf.Min(screenCenter.x, screenCenter.y) - offset);
        indicatorScreenPos.x = Mathf.Clamp(indicatorScreenPos.x, offset, Screen.width - offset);
        indicatorScreenPos.y = Mathf.Clamp(indicatorScreenPos.y, offset, Screen.height - offset);

        RectTransform indicatorRect = group.indicator.GetComponent<RectTransform>();
        RectTransform canvasRect = monsterArrowBox.GetComponent<RectTransform>();
        Canvas canvas = monsterArrowBox.GetComponentInParent<Canvas>();
        Camera uiCamera = canvas.renderMode == RenderMode.ScreenSpaceCamera ? canvas.worldCamera : null;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, indicatorScreenPos, uiCamera, out Vector2 localPos);
        indicatorRect.localPosition = localPos;

        float angle = Mathf.Atan2(group.direction.y, group.direction.x) * Mathf.Rad2Deg;
        indicatorRect.rotation = Quaternion.Euler(0, 0, angle - 90f);

        UpdateIndicatorText(group.indicator, group.monsterName, group.count);
    }

    private void UpdateIndicatorText(GameObject indicator, string name, int count)
    {
        TextMeshProUGUI text = indicator.GetComponentInChildren<TextMeshProUGUI>();

        if (text != null)
        {
            text.text = $"{name} x{count}";
        }
    }

    private void CleanupUnusedIndicators(List<IndicatorGroup> activeGroups)
    {
        foreach (Transform child in monsterArrowBox)
        {
            GameObject arrow = child.gameObject;
            bool isUsed = activeGroups.Exists(g => g.indicator == arrow);
            if (!isUsed)
            {
                arrow.SetActive(false);
                ObjectPoolManager.Instance.ResivePool("Pref_000000", arrow, monsterArrowBox);
            }
        }
    }

    private async void StartIndicatorCleanup()
    {
        indicatorStart = true;
        await UniTask.Delay(3000);

        if (monsterArrowBox != null)
        {
            foreach (Transform child in monsterArrowBox)
            {
                child.gameObject.SetActive(false);
            }
        }
        indicatorStart = false;
    }

    private bool IsInView(Vector3 screenPos)
    {
        return screenPos.z > 0 && screenPos.x > 0f && screenPos.x < 1f && screenPos.y > 0 && screenPos.y < 1f;
    }

    private Vector2 GetDirection(Vector3 viewportPos)
    {
        Vector2 center = new(0.5f, 0.5f);
        Vector2 dir = ((Vector2)viewportPos - center).normalized;
        return viewportPos.z < 0 ? -dir : dir;
    }

    private string GetName(Transform enemy)
    {
        try
        {
            return enemy.GetComponent<EnemyController>().enemyData.enemyName;

        }
        catch
        {

            return "";
        }
    }

    //private string ChangeName(string rawName)
    //{

    //    //Clone 혹시나 지우기
    //    string name = rawName.Replace("(Clone)", "").Trim();

    //    if (name.StartsWith("Pref_"))
    //    {
    //        if (int.TryParse(name.Replace("Pref_", ""), out int code))
    //        {
    //            if (code >= 400001 && code <= 400004)
    //                return "병사";
    //            else if (code >= 410001 && code <= 410004)
    //                return "날파리";
    //        }
    //    }

    //    return name;
    //}

}

