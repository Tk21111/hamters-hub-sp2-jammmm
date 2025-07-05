using UnityEngine;

public enum BiomeType
{
    None,
    Forest,
    Plains,
    Desert
    // Add any other biomes you need
}

[RequireComponent(typeof(Collider))]
public class BiomeZone : MonoBehaviour
{
    public BiomeType biomeType;

    private void Awake()
    {
        // Ensure the collider is a trigger so objects can pass through
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        // When an AI enters this zone, tell it what biome it's in
        BaseAI ai = other.GetComponent<BaseAI>();
        if (ai != null)
        {
            ai.OnEnterBiome(biomeType);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // When an AI leaves, let it know it's no longer in this specific biome
        BaseAI ai = other.GetComponent<BaseAI>();
        if (ai != null)
        {
            ai.OnExitBiome(biomeType);
        }
    }
}