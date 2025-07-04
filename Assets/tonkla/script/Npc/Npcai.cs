using UnityEngine;

public class controller : MonoBehaviour
{
    private Vision vision;
    private Stat stat;

    private Transform _target;

    public float moveSpeed = 5f;
    public float stopDistance = 0.1f;

    private void Awake()
    {
        vision = GetComponent<Vision>();
        stat = GetComponent<Stat>();
    }

    void Update()
    {
        if (stat._health < 50f)
        {
            foreach (GameObject obj in vision.GetGameObjectInSight())
            {
                if (obj.CompareTag("food") || (obj.CompareTag("food_runable") && stat._strength > 50f))
                {
                    _target = obj.transform;
                    break;  // take first valid target
                }
            }
        }
        else
        {
            _target = null;  // no target if healthy
        }

        if (_target != null)
        {
            float dist = Vector3.Distance(_target.position, transform.position);
            if (dist > stopDistance)
            {
                Vector3 direction = (_target.position - transform.position).normalized;
                transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            }
        }
    }
}
