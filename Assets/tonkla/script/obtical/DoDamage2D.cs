using UnityEngine;

public class DoDamage2D : MonoBehaviour
{

    public float dealDamage = 4f;
    void OnTriggerEnter2D(Collider2D other)
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
