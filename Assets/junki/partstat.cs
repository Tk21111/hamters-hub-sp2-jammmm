using UnityEngine;

public class PartEffect : MonoBehaviour
{
    public Stat.StatDelta deltaStat;

    public void ApplyTo(BaseAI target)
    {
        target.ModifyStat(deltaStat);
    }
}