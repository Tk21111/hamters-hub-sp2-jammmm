using UnityEngine;

public class DoDamage : MonoBehaviour
{

    public float dealDamage = 4f;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("npc")) return;

        // Get receiver from the NPC
        IDamageAble damage = other.GetComponent<IDamageAble>();

        if (damage != null)
        {
            damage.Damage(dealDamage);
        }
        else
        {
            Debug.Log("fail to damage");
        }

    }
}
