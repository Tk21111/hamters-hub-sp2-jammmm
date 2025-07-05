using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{
    //internal
    public Transform _target;
    public float _sightRadius = 10f;
    public LayerMask obstacleMask; 

    public string[] targetTagsToLookFor = { "tree" ,  "food" , "food_runable" , "water" , "npc"  };

    public List<GameObject> GetGameObjectInSight()
    {

        

        List<GameObject> visibleGameObject = new List<GameObject>();
        Collider[] hits = Physics.OverlapSphere(transform.position, _sightRadius);
        
        foreach (Collider hit in hits)
        {
            ISeeAble seeAble = hit.gameObject.GetComponent<ISeeAble>();

        
            if (seeAble == null  || hit.gameObject == gameObject || seeAble.GetSeeAble())
                continue;

            bool tagMatch = false;

            // Debug.Log(hit.gameObject.tag);
            foreach (string tag in targetTagsToLookFor)
            {

                if (hit.CompareTag(tag))
                {
                    tagMatch = true;
                    break;
                }
            }

            if (!tagMatch)
            {
                continue;
            }

            //check los

            Vector3 vecToTarget = (hit.transform.position - transform.position).normalized;
            float distToTarget = Vector3.Distance(transform.position, hit.transform.position);

            // Raycast to check line of sight
            if (!Physics.Raycast(transform.position, vecToTarget, distToTarget, obstacleMask))
            {
                visibleGameObject.Add(hit.gameObject);
            }
        }

        visibleGameObject.Sort((a, b) =>
        {
            float distA = Vector3.Distance(transform.position, a.transform.position);
            float distB = Vector3.Distance(transform.position, b.transform.position);

            return distA.CompareTo(distB);
        });
        
        return visibleGameObject;
    }
    
    private void OnDrawGizmosSelected()
    {
        // Visualize detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _sightRadius);
    }
}
