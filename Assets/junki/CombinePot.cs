using System.Collections.Generic;
using UnityEngine;
public class CombinePot : MonoBehaviour
{
    public List<PartEffect> currentParts = new List<PartEffect>();

    public void TryAddPart(PartEffect part)
    {
        if (!currentParts.Contains(part))
        {
            currentParts.Add(part);
            Debug.Log("เพิ่มชิ้นส่วน: " + part.name);
        }

        if (currentParts.Count >= 3)
        {
            CombineParts();
        }
    }

    private void CombineParts()
    {
        // รวมค่า Stat จากทุกชิ้น
        Stat.StatDelta combined = new Stat.StatDelta();

        foreach (var part in currentParts)
        {
            combined.health += part.deltaStat.health;
            combined.strength += part.deltaStat.strength;
            combined.weight += part.deltaStat.weight;
            combined.resistanceIliness += part.deltaStat.resistanceIliness;
            combined.resistanceEnv += part.deltaStat.resistanceEnv;
            combined.fly += part.deltaStat.fly;
            combined.drive += part.deltaStat.drive;
        }

        // ส่งค่า combined ไปยังตัวละครที่ต้องการ
        BaseAI target = FindObjectOfType<BaseAI>(); // หรือรับมาจากระบบอื่น
        target.ModifyStat(combined);

        Debug.Log("Apply Stat เรียบร้อยให้: " + target.name);

        // เคลียร์
        currentParts.Clear();
    }
}