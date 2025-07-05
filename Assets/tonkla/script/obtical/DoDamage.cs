using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DoDamage : MonoBehaviour, ISeeAble
{
   public float dealDamage = 4f;
   public float damageInterval = 0.1f;
   public float cooldownDuration = 5f;
   
   private Dictionary<Collider, Coroutine> damageCoroutines = new Dictionary<Collider, Coroutine>();
   private Dictionary<Collider, float> damageCooldowns = new Dictionary<Collider, float>();

   private void OnTriggerEnter(Collider other)
   {
       if (!other.CompareTag("npc")) return;

       IDamageAble damage = other.GetComponent<IDamageAble>();
       if (damage != null)
       {
           // Start continuous damage
           Coroutine damageCoroutine = StartCoroutine(DealContinuousDamage(other, damage));
           damageCoroutines[other] = damageCoroutine;
       }
       else
       {
           Debug.Log("fail to damage");
       }
   }

   private void OnTriggerExit(Collider other)
   {
       if (!other.CompareTag("npc")) return;

       // Stop continuous damage when leaving trigger
       if (damageCoroutines.ContainsKey(other))
       {
           StopCoroutine(damageCoroutines[other]);
           damageCoroutines.Remove(other);
       }
   }

   private IEnumerator DealContinuousDamage(Collider target, IDamageAble damageComponent)
   {
       while (target != null && damageComponent != null)
       {
           // Check if target is on cooldown
           if (CanDamageTarget(target))
           {
               damageComponent.Damage(dealDamage);
               // Set cooldown for this target
               damageCooldowns[target] = Time.time + cooldownDuration;
           }
           
           yield return new WaitForSeconds(damageInterval);
       }
   }

   private bool CanDamageTarget(Collider target)
   {
       if (!damageCooldowns.ContainsKey(target))
           return true;
           
       return Time.time >= damageCooldowns[target];
   }

   public bool GetSeeAble()
   {
       return true; // Always visible, or add your own logic here
   }

   private void OnDestroy()
   {
       // Clean up all coroutines when object is destroyed
       foreach (var coroutine in damageCoroutines.Values)
       {
           if (coroutine != null)
               StopCoroutine(coroutine);
       }
       damageCoroutines.Clear();
       damageCooldowns.Clear();
   }
}