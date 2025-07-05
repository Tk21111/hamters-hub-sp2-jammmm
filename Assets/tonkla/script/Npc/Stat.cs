using System.Collections.Generic;
using UnityEngine;

public class Stat : MonoBehaviour, IreciveAble
{

 


    public float _health = 100f;

    //no eff by trick
    public float _strength = 0f;
    public float _weight = 0f;

    //eff by trick
    public float _resistanceIliness = 0f;
    public float _resistanceEnv = 0f;
    public float _fly = 0f;
    public float _drive = 0f;




    public struct StatDelta
    {
        public float strength;
        public float health;
        public float weight;
        public float resistanceIliness;
        public float resistanceEnv;
        public float fly;
        public float drive;
    }

    public void ApplyStatDelta(StatDelta delta)
    {
        _strength += delta.strength;
        _health = Mathf.Clamp(_health + delta.health, 0, 100f);
        _weight += delta.weight;
        _resistanceIliness += delta.resistanceIliness;
        _resistanceEnv += delta.resistanceEnv;
        _fly += delta.fly;
        _drive += delta.drive;
    }

    private void OnEnable()
    {
        TrickManager.OnTick += HandleTrick;
    }

    private void OnDisable()
    {
        TrickManager.OnTick -= HandleTrick;
    }

    public void HandleTrick()
    {
        _resistanceIliness -= 1f;
        _resistanceEnv -= 1f;
        _fly -= 1f;
        _drive -= 1f;
    }

    #region reviveAble
    public void ReciveItem(string itemName, int amount)
    {
        Debug.Log($"{gameObject.name} received {amount}x {itemName}");

        StatDelta delta = GetDeltaForItem(itemName, amount);
        
        ApplyStatDelta(delta);

        // Add the items to inventory
        AddItem(itemName, amount);
    }
    #endregion



    #region  storage sys

    private Dictionary<string, int> resource = new Dictionary<string, int>();


    public void AddItem(string itemName, int amount)
    {
        if (resource.ContainsKey(itemName))
        {
            resource[itemName] += amount;
        }
        else
        {
            resource[itemName] = amount;
        }
    }

    public int GetAmount(string itemName)
    {
        if (resource.TryGetValue(itemName, out int amount))
        {
            return amount;
        }
        return 0;
    }

    public string GetMaxAmount()
    {
        string maxResource = null;
        int maxAmount = 0;

        foreach (var pair in resource)
        {
            if (pair.Value > maxAmount)
            {
                maxAmount = pair.Value;
                maxResource = pair.Key;
            }
        }

        return maxResource;
    }

    public bool useResource(string itemName ,  int amount)
    {
        if (GetAmount(itemName) >= amount)
        {
            resource[itemName] -= amount;

            return true;
        }
        else
        {
            Debug.Log("useResource : fail to use");
            return false;
        }
    }

    #endregion


    #region  Helper

    private StatDelta GetDeltaForItem(string itemName, int amount)
    {
        StatDelta delta = new StatDelta();

        switch (itemName.ToLower())
        {
            case "apple":
                delta.health = 5f * amount;
                break;
            case "wood":
                delta.strength = 1f * amount;
                delta.weight = 0.5f * amount;
                break;
            case "medicine":
                delta.resistanceIliness = 2f * amount;
                break;
            case "boots":
                delta.drive = 1f * amount;
                break;
            case "glider":
                delta.fly = 1f * amount;
                break;
            // Add more items here as needed
            default:
                Debug.LogWarning($"Unknown item: {itemName}");
                break;
        }

        return delta;
    }

    #endregion

}
