using UnityEngine;

public class TreeGiveable : MonoBehaviour, IgiveAble
{
    [SerializeField] private string itemName = "wood";
    [SerializeField] private int itemAmount = 1;
    [SerializeField] private float cooldownDuration = 5f;

    private float nextAvailableTime = 0f;

    public string GetItemName() => itemName;

    public int GetItemAmount()
    {
        if (!IsAvailable()) return 0;

        // Start cooldown after item is given
        nextAvailableTime = Time.time + cooldownDuration;
        return itemAmount;
    }

    public bool IsAvailable()
    {
        return Time.time >= nextAvailableTime;
    }

}
