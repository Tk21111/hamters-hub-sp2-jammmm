using UnityEngine;

public class TreeGiveable : MonoBehaviour, IgiveAble
{
    [SerializeField] private string itemName = "wood";
    [SerializeField] private int itemAmount = 1;

    public string GetItemName() => itemName;
    public int GetItemAmount() => itemAmount;
}
