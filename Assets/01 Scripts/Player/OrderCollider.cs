using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderCollider : MonoBehaviour
{
    [SerializeField] GameObject firstObj;
    [SerializeField] GameObject secondObj;

    float playTime;
    bool isDiffuse;


    private void Update()
    {
        if (isDiffuse)
        {
            playTime += Time.deltaTime;
            if (playTime > 3f)
            {
                firstObj.SetActive(false);
                secondObj.SetActive(true);
                isDiffuse = false;
            }

        }
    }

    public void StartDiffuse()
    {
        playTime = 0;
        isDiffuse = true;
        firstObj.SetActive(true);
        secondObj.SetActive(false);
    }
}
