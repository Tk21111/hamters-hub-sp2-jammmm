using System.Collections.Generic;
using UnityEngine;

// สคริปต์นี้ทำหน้าที่รวบรวมชิ้นส่วน และเมื่อครบตามจำนวนที่กำหนด
// จะสร้างตัวละครใหม่จาก Prefab พร้อมกับค่าสถานะที่รวมจากทุกชิ้นส่วน
public class CombineAndCreate : MonoBehaviour
{
    // Singleton instance เพื่อให้สคริปต์อื่นเรียกใช้ได้ง่าย
    public static CombineAndCreate Instance;

    [Header("Configuration")]
    [Tooltip("จำนวนชิ้นส่วนที่ต้องการเพื่อเริ่มการผสม")]
    public int partsNeeded = 3;

    [Tooltip("Prefab ของตัวละครที่จะสร้างขึ้นหลังจากการผสม")]
    public GameObject resultPrefab;

    [Tooltip("ตำแหน่งที่จะสร้างตัวละครใหม่")]
    public Transform spawnPoint;

    // รายการสำหรับเก็บชิ้นส่วน UI ที่ถูกเพิ่มเข้ามา
    private List<PartEffect> currentParts = new List<PartEffect>();

    private void Awake()
    {
        // ตั้งค่า Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // เมธอดสำหรับลองเพิ่มชิ้นส่วนเข้ามาในรายการเพื่อรอการผสม
    public void TryAddPart(PartEffect part)
    {
        if (!currentParts.Contains(part))
        {
            currentParts.Add(part);
            Debug.Log("เพิ่มชิ้นส่วน: " + part.name);
        }

        // ถ้ามีชิ้นส่วนครบตามที่กำหนด ให้เริ่มการผสม
        if (currentParts.Count >= partsNeeded)
        {
            Combine();
        }
    }

    private void Combine()
    {
        if (resultPrefab == null || spawnPoint == null)
        {
            Debug.LogError("ยังไม่ได้กำหนดค่า Result Prefab หรือ Spawn Point ใน Inspector!");
            return;
        }

        // 1. รวมค่าสถานะจากทุกชิ้นส่วน
        Stat.StatDelta combinedStats = new Stat.StatDelta();
        foreach (var part in currentParts)
        {
            combinedStats.health += part.deltaStat.health;
            combinedStats.strength += part.deltaStat.strength;
            combinedStats.weight += part.deltaStat.weight;
            combinedStats.resistanceIliness += part.deltaStat.resistanceIliness;
            combinedStats.resistanceEnv += part.deltaStat.resistanceEnv;
            combinedStats.fly += part.deltaStat.fly;
            combinedStats.drive += part.deltaStat.drive;
        }

        // 2. สร้างตัวละครใหม่จาก Prefab
        GameObject newCharacterObject = Instantiate(resultPrefab, spawnPoint.position, spawnPoint.rotation);
        BaseAI newAI = newCharacterObject.GetComponent<BaseAI>();

        // 3. นำค่าสถานะที่รวมแล้วไปใช้กับตัวละครใหม่
        if (newAI != null)
        {
            newAI.ModifyStat(combinedStats);
            Debug.Log("สร้างตัวละครใหม่ '" + newCharacterObject.name + "' พร้อมค่าสถานะรวมแล้ว!");
        }
        else
        {
            Debug.LogError("Prefab ที่กำหนดไม่มีคอมโพเนนต์ BaseAI!");
        }

        // 4. ทำความสะอาดและล้างรายการ
        foreach (var part in currentParts)
        {
            // สมมติว่าชิ้นส่วนเหล่านี้เป็น GameObject ของ UI ที่ควรจะถูกทำลายทิ้ง
            if (part != null)
            {
                Destroy(part.gameObject);
            }
        }
        currentParts.Clear();
    }
}