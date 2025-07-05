using UnityEngine;

public class GiveNpcItem : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("npc")) return;

        // Get giver from this object
        IgiveAble giver = GetComponent<IgiveAble>();
        // Get receiver from the NPC
        IreciveAble receiver = other.GetComponent<IreciveAble>();

        Debug.Log("ggg");

        if (giver != null && receiver != null)
        {
            receiver.ReciveItem(giver.GetItemName(), giver.GetItemAmount());
        }
        else
        {
            Debug.LogWarning("GiveNpc: Missing IGiveable or IReceivable on involved objects.");
        }
    }
}
