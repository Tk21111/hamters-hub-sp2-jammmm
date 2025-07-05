using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    public Transform _target;
    public float _sightRadius = 10f;
    public LayerMask obstacleMask;

    public string[] targetTagsToLookFor = { "tree", "food", "food_runable", "water", "npc", "obstacle" };

    /// <summary>
    /// Normal vision check: tag match, seeAble, and not blocked by obstacle.
    /// </summary>
    public List<GameObject> GetGameObjectInSight()
    {
        return GetGameObjectInSightInternal(ignoreObstacles: false);
    }

    /// <summary>
    /// "Stupid" vision: ignores obstacles and visibility blocking.
    /// </summary>
    public List<GameObject> GetGameObjectInSightIgnoreObstacles()
    {
        return GetGameObjectInSightInternal(ignoreObstacles: true);
    }

   private List<GameObject> GetGameObjectInSightInternal(bool ignoreObstacles)
    {
        List<GameObject> visibleGameObjects = new List<GameObject>();
        Collider[] hits = Physics.OverlapSphere(transform.position, _sightRadius);

        foreach (Collider hit in hits)
        {
            GameObject hitObj = hit.gameObject;

            if (hitObj == gameObject) continue;

            ISeeAble seeAble = hit.GetComponent<ISeeAble>();

            if (seeAble == null || !seeAble.GetSeeAble())
            {
                continue;
            }

            // Match tag
            bool tagMatch = false;
            foreach (string tag in targetTagsToLookFor)
            {
                if (hitObj.CompareTag(tag))
                {
                    tagMatch = true;
                    break;
                }
            }
            if (!tagMatch) continue;

            // LOS check only if not ignoring obstacles
            if (!ignoreObstacles)
            {
                Vector3 dir = (hit.transform.position - transform.position).normalized;
                float dist = Vector3.Distance(transform.position, hit.transform.position);

                if (Physics.Raycast(transform.position, dir, dist, obstacleMask))
                {
                    continue;
                }
            }

            visibleGameObjects.Add(hitObj);
        }

        // Sort by distance
        visibleGameObjects.Sort((a, b) =>
        {
            float distA = Vector3.Distance(transform.position, a.transform.position);
            float distB = Vector3.Distance(transform.position, b.transform.position);
            return distA.CompareTo(distB);
        });

        return visibleGameObjects;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _sightRadius);
    }
}
