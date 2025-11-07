using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SpeedManager : Singleton<SpeedManager>
{
    private readonly float[] speedLevels = { 1f, 2f, 4f };
    private int currentSpeedIndex = 0;

    public event Action<float> OnSpeedChanged;

    private void Start()
    {
        ApplySpeed(currentSpeedIndex);
    }

    public void ToggleSpeed()
    {
        currentSpeedIndex = (currentSpeedIndex + 1) % speedLevels.Length;
        ApplySpeed(currentSpeedIndex);
    }

    private void ApplySpeed(int index)
    {
        currentSpeedIndex = index;
        float newSpeed = speedLevels[currentSpeedIndex];

        Time.timeScale = newSpeed;
        OnSpeedChanged?.Invoke(newSpeed);
    }

    public void ResetSpeed()
    {
        Time.timeScale = 1f;
        OnSpeedChanged?.Invoke(1f);
    }
}
