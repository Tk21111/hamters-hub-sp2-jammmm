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
            DecreaseNPCHealth();
        }
    }

    private void DecreaseNPCHealth()
    {
        // Find all GameObjects with the "NPC" tag
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("npc");

        if (npcs.Length == 0)
        {
            Debug.LogWarning("No GameObjects found with the tag 'NPC'. Make sure your NPCs have this tag.");
            return;
        }

        Debug.Log($"Found {npcs.Length} NPCs. Decreasing their health.");

        foreach (GameObject npc in npcs)
        {
            // Try to get the NPCHealth component from the GameObject
            Stat stat = npc.GetComponent<Stat>();

            if (stat != null)
            {
                
                stat._health -= 10 / stat._resistanceEnv;
            }
            else
            {
                Debug.LogWarning($"GameObject '{npc.name}' with tag 'NPC' does not have an 'NPCHealth' component attached.");
            }
        }
    }
}
