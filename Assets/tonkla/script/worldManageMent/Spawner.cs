// using UnityEngine;
// using System.Collections.Generic; // Required for using Lists

// [System.Serializable]
// public class CreatureToSpawn
// {
//     public string name; // For easy identification in the Inspector
//     public GameObject creaturePrefab;
//     public Stat baseStat;
// }

// public class Spawner : MonoBehaviour
// {
//     public List<CreatureToSpawn> creatureTypes = new List<CreatureToSpawn>();

//     /// <summary>
//     /// Spawns a specific type of creature by its index in the list.
//     /// </summary>
//     /// <param name="creatureIndex">The index of the creature type to spawn.</param>
//     public void SpawnCreature(int creatureIndex)
//     {
//         if (creatureIndex < 0 || creatureIndex >= creatureTypes.Count)
//         {
//             Debug.LogError("Invalid creature index provided.");
//             return;
//         }

//         CreatureToSpawn selectedCreature = creatureTypes[creatureIndex];
//         GameObject newCreature = Instantiate(selectedCreature.creaturePrefab);
//         Stat stat = newCreature.GetComponent<Stat>();

//         if (stat != null && selectedCreature.baseStat != null)
//         {
//             // Clone stat values
//             stat.FromData(selectedCreature.baseStat.ToData());
//         }
//     }

//     /// <summary>
//     /// Spawns a random creature from the list.
//     /// </summary>
//     public void SpawnRandomCreature()
//     {
//         if (creatureTypes.Count == 0)
//         {
//             Debug.LogError("No creature types have been assigned to the spawner.");
//             return;
//         }

//         int randomIndex = Random.Range(0, creatureTypes.Count);
//         SpawnCreature(randomIndex);
//     }
// }