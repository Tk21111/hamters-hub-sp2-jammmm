using UnityEngine;

public class Stat : MonoBehaviour
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

    public virtual void HandleTrick()
    {
        _resistanceIliness -= 1f;
        _resistanceEnv -= 1f;
        _fly -= 1f;
        _drive -= 1f;
    }

    private void Update() {
        
    }

}
