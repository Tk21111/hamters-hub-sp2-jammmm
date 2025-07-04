using UnityEngine;

public class Spawner : MonoBehaviour
{
    public Stat baseStat; // assign a template stat (could be a prefab)

    public GameObject creaturePrefab;

    public void SpawnCreature()
    {
        GameObject newCreature = Instantiate(creaturePrefab);
        Stat stat = newCreature.GetComponent<Stat>();

        if (stat != null && baseStat != null)
        {
            // Clone stat values
            stat.FromData(baseStat.ToData());
        }
    }
}
