using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrickManager : MonoBehaviour
{

    public static event Action OnTick;
    public static event Action OnYear;
    public static event Action onEvent;

    void Start()
    {
        StartCoroutine(TickCount());
        StartCoroutine(TickYear());
        StartCoroutine(TickEvent());
    }

    //hunger loop    
    private IEnumerator TickCount()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            OnTick?.Invoke();

        }
    }

    //year loop
    private IEnumerator TickYear()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f);
            OnYear?.Invoke();
        }
    }

    //even loop
    private IEnumerator TickEvent()
    {
        while (true)
        {
            float lenght = Random.Range(80f, 855f);
            yield return new WaitForSeconds(lenght);
            onEvent?.Invoke();
        }
    }
}
