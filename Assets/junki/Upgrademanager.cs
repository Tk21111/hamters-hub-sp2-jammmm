using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public BaseAI selectedNPC;
    public Stat.StatDelta pendingUpgrade;

    public void SelectNPC(BaseAI npc)
    {
        selectedNPC = npc;
        Debug.Log("เลือก NPC: " + npc.name);
    }

    public void SetUpgradeDelta(Stat.StatDelta delta)
    {
        pendingUpgrade = delta;
        Debug.Log("ตั้งค่าอัปเกรด: HP+" + delta.health + " STR+" + delta.strength);
    }

    public void ApplyUpgrade()
    {
        if (selectedNPC == null)
        {
            Debug.Log("ยังไม่ได้เลือก NPC");
            return;
        }

        selectedNPC.ModifyStat(pendingUpgrade);
        Debug.Log("อัปเกรดเรียบร้อยให้: " + selectedNPC.name);

        // ล้างค่าที่อัปเกรด
        pendingUpgrade = new Stat.StatDelta();
    }
}