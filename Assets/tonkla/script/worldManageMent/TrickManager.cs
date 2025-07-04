using System;
using System.Collections;
using UnityEngine;

public class TrickManager : MonoBehaviour
{

    public static event Action OnTick;
    public float interval = 2f;

    void Start()
    {
        StartCoroutine(TickCount());    
    }

    private IEnumerator TickCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(interval);
            OnTick?.Invoke();
            
        }
    }
}
