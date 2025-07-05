using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Stat : MonoBehaviour, IreciveAble, IDamageAble
{
    public float _health = 100f;

    // Not affected by trick
    public float _strength = 0f;
    public float _weight = 0f;

    // Affected by trick
    public float _resistanceIliness = 0f;
    public float _resistanceEnv = 0f;
    public float _fly = 0f;
    public float _drive = 0f;
    public float _intelligent = 50f;

    //encouraage
    public string _encouragedStat;
    public int _encouragedStatTime;

    public float _encouragedStatCoolDown;

    public struct StatDelta
    {
        public float strength;
        public float health;
        public float weight;
        public float resistanceIliness;
        public float resistanceEnv;
        public float fly;
        public float drive;
        public float intelligent;
    }

    #region trick
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

    #endregion

    #region Receiveable

    public void ReciveItem(string itemName, int amount)
    {
        Debug.Log($"{gameObject.name} received {amount}x {itemName}");

        StatDelta delta = GetDeltaForItem(itemName, amount);

        // Apply the change
        ApplyStatDelta(delta);

        // Add item to resource inventory
        AddItem(itemName, amount);


    }

    #endregion

    #region Storage System

    protected Dictionary<string, int> resource = new Dictionary<string, int>();

    public void AddItem(string itemName, int amount)
    {
        Debug.Log("adddddd");
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
        return resource.TryGetValue(itemName, out int amount) ? amount : 0;
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

    public bool useResource(string itemName, int amount)
    {
        if (GetAmount(itemName) >= amount)
        {
            resource[itemName] -= amount;
            return true;
        }
        else
        {
            Debug.Log("useResource: failed to use");
            return false;
        }
    }

    #endregion

    #region Stat Application

    public void ApplyStatDelta(StatDelta delta)
    {

        _strength += delta.strength;
        _health = Mathf.Clamp(_health + delta.health, 0, 100f);
        _weight += delta.weight;
        _resistanceIliness += delta.resistanceIliness;
        _resistanceEnv += delta.resistanceEnv;
        _fly += delta.fly;
        _drive += delta.drive;
        _intelligent += delta.intelligent;

        //set encouraged
        GetEncouragedStat(delta);
    }

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
            case "book":
                delta.intelligent = 3f * amount;
                break;
            default:
                Debug.LogWarning($"Unknown item: {itemName}");
                break;
        }

        return delta;
    }

    #endregion

    #region Damageable

    public void Damage(float amount)
    {
        _health -= amount;
        if (_health <= 0)
        {
            Destroy(gameObject);
        }
    }

    #endregion

    #region Encouragement System

    private void GetEncouragedStat(StatDelta delta)
    {
        Dictionary<string, float> statChanges = new Dictionary<string, float>
        {
            { "strength", delta.strength },
            { "health", delta.health },
            { "weight", delta.weight },
            { "resistanceIliness", delta.resistanceIliness },
            { "resistanceEnv", delta.resistanceEnv },
            { "fly", delta.fly },
            { "drive", delta.drive },
            { "intelligent", delta.intelligent }
        };

        string encouragedStat = null;
        float maxChange = 0f;

        foreach (var kvp in statChanges)
        {
            if (kvp.Value > maxChange)
            {
                maxChange = kvp.Value;
                encouragedStat = kvp.Key;
            }
        }

        if (encouragedStat != null)
        {
            Debug.Log($"[Encouragement] {gameObject.name} improved most in: {encouragedStat} (+{maxChange})");
        }

        _encouragedStat = encouragedStat;

        _encouragedStatTime = Random.Range(1, 5);
    }

    #endregion
    
    protected virtual void Update() { }

}