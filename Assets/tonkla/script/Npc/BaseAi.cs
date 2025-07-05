using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Idle,
    Roaming,
    HuntingForFood,
    CollectingWood,
    BuildingHouse,
    DoSomethingStupid
}

[RequireComponent(typeof(Vision), typeof(Stat))]
public class BaseAI : Stat
{
    [Header("State Machine")]
    [SerializeField] private AIState currentState = AIState.Roaming;

    [Header("Behavior: Hunting")]
    public string[] seekableFoodTags;

    [Header("Behavior: Building")]
    public string woodSourceTag = "wood";
    public int woodNeededForHouse = 10;
    public GameObject houseConstructionPrefab;
    public GameObject finishedHousePrefab;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float stopDistance = 1f;

    [Header("Behavior: Roaming")]
    public float roamRadius = 10f;
    public float roamDelay = 3f;
    public string[] roamInterestTags = { "tree", "water", "npc" };

    private Vector3 roamTarget;
    private float nextRoamTime = 0f;
    private Vector3 roamAnchorPoint;
    private bool hasRoamTarget = false;

    public BiomeType currentBiome = BiomeType.None;
    private bool hasBuiltHouse = false;
    private Transform currentTarget;
    private Transform houseConstructionSite;

    protected Vision vision;

    protected virtual void Awake()
    {
        vision = GetComponent<Vision>();
        roamAnchorPoint = transform.position;
    }

    protected override void Update()
    {
        UpdateStateTransitions();
        ExecuteCurrentState();
    }

    private void UpdateStateTransitions()
    {
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > stopDistance)
            return;

        if (_health < 50f)
        {
            SetState(AIState.HuntingForFood);
            return;
        }

        if (currentBiome == BiomeType.Forest && !hasBuiltHouse)
        {
            if (GetAmount("wood") < woodNeededForHouse)
                SetState(AIState.CollectingWood);
            else
                SetState(AIState.BuildingHouse);

            return;
        }

        SetState(AIState.Roaming);
    }

    private void SetState(AIState newState)
    {
        if (currentState != newState)
        {
            Debug.Log($"Transitioning to state: {newState}");
            currentState = newState;
        }
    }

    private void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.HuntingForFood:
                ExecuteHuntingState();
                break;
            case AIState.CollectingWood:
                ExecuteCollectingWoodState();
                break;
            case AIState.BuildingHouse:
                ExecuteBuildingState();
                break;
            case AIState.Roaming:
                ExecuteRoamingState();
                break;
            case AIState.DoSomethingStupid:
                ExecuteDoStupid();
                break;
        }
    }

    private void ExecuteRoamingState()
    {
        if (Time.time > nextRoamTime || Vector3.Distance(transform.position, roamTarget) < stopDistance)
        {
            SetNewRoamTarget();
        }

        if (hasRoamTarget && Vector3.Distance(transform.position, roamTarget) > stopDistance)
        {
            Vector3 direction = (roamTarget - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    private void SetNewRoamTarget()
    {
        Transform interestingTarget = FindInterestingRoamTarget();

        if (interestingTarget != null)
        {
            roamTarget = interestingTarget.position;
        }
        else
        {
            Vector2 randomCircle = Random.insideUnitCircle * roamRadius;
            roamTarget = roamAnchorPoint + new Vector3(randomCircle.x, 0, randomCircle.y);
        }

        hasRoamTarget = true;
        nextRoamTime = Time.time + roamDelay;
    }

    private Transform FindInterestingRoamTarget()
    {
        List<GameObject> visibleObjects = vision.GetGameObjectInSight();

        foreach (GameObject obj in visibleObjects)
        {
            if (Vector3.Distance(roamAnchorPoint, obj.transform.position) > roamRadius)
                continue;

            if (!System.Array.Exists(roamInterestTags, tag => obj.CompareTag(tag)))
                continue;

            Vector3 dir = (obj.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, obj.transform.position);

            if (!Physics.Raycast(transform.position, dir, dist, LayerMask.GetMask("obstacle")))
            {
                return obj.transform;
            }
        }

        return null;
    }

    private void ExecuteHuntingState()
    {
        FindTarget(seekableFoodTags);
        MoveTowardsTarget();
    }

    private void ExecuteCollectingWoodState()
    {
        if (currentTarget == null)
            FindTargetStupidly(new string[] { woodSourceTag });

        if (currentTarget == null)
        {
            SetState(AIState.Roaming);
            return;
        }

        MoveTowardsTarget();

        if (Vector3.Distance(transform.position, currentTarget.position) <= stopDistance)
        {
            currentTarget = null;
            SetState(AIState.DoSomethingStupid);
        }
    }

    private void ExecuteBuildingState()
    {
        if (houseConstructionSite == null)
        {
            houseConstructionSite = Instantiate(houseConstructionPrefab, transform.position + transform.forward * 2, Quaternion.identity).transform;
        }

        currentTarget = houseConstructionSite;
        MoveTowardsTarget();

        if (Vector3.Distance(transform.position, houseConstructionSite.position) < stopDistance + 1f)
        {
            Instantiate(finishedHousePrefab, houseConstructionSite.position, houseConstructionSite.rotation);
            Destroy(houseConstructionSite.gameObject);
            hasBuiltHouse = true;
        }
    }

    private void ExecuteDoStupid()
    {
        if (currentTarget == null)
            FindTargetStupidly(new string[] { "water", "obstacle" });

        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) < stopDistance + 0.5f)
        {
            currentTarget = null;
            SetState(AIState.Roaming);
            return;
        }

        MoveTowardsTarget();
    }

    private void FindTargetStupidly(string[] tagsToFind)
    {
        float closestDist = float.MaxValue;
        GameObject closestTarget = null;

        foreach (GameObject obj in vision.GetGameObjectInSightIgnoreObstacles())
        {
            foreach (string tag in tagsToFind)
            {
                if (obj.CompareTag(tag))
                {
                    float distance = Vector3.Distance(transform.position, obj.transform.position);
                    if (distance < closestDist)
                    {
                        closestDist = distance;
                        closestTarget = obj;
                    }
                }
            }
        }

        if (closestTarget != null)
            currentTarget = closestTarget.transform;
    }

    private void FindTarget(string[] tagsToFind)
    {
        foreach (GameObject obj in vision.GetGameObjectInSight())
        {
            foreach (string tag in tagsToFind)
            {
                if (obj.CompareTag(tag))
                {
                    currentTarget = obj.transform;
                    return;
                }
            }
        }
    }

    private void MoveTowardsTarget()
    {
        if (currentTarget != null)
        {
            Vector3 direction = (currentTarget.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }
    }

    public void OnEnterBiome(BiomeType biome) => currentBiome = biome;
    public void OnExitBiome(BiomeType biome) => currentBiome = currentBiome == biome ? BiomeType.None : currentBiome;
}
