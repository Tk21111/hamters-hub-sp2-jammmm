using System.Collections.Generic;
using UnityEngine;

// Define the possible states for our AI
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

    // Internal State
    public BiomeType currentBiome = BiomeType.None;
    private bool hasBuiltHouse = false;
    private Transform currentTarget;
    private Transform houseConstructionSite;

    // Components
    protected Vision vision;

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        vision = GetComponent<Vision>();
        roamAnchorPoint = transform.position; // Set anchor point once at start
    }

    protected virtual void Update()
    {
        // The "brain" of the AI: decides when to change tasks.
        UpdateStateTransitions();

        // Executes the behavior for the current state.
        ExecuteCurrentState();
    }
    #endregion

    #region State Machine Logic
    /// <summary>
    /// This is the AI's decision-making center. It checks priorities and changes state.
    /// </summary>
    private void UpdateStateTransitions()
    {

         if (currentState == AIState.DoSomethingStupid)
        {
            return;
        }
        
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) > stopDistance)
        {
            return; // Still moving towards current target, don't change state
        }

        // HIGHEST PRIORITY: Find food if health is low.
        if (_health < 50f)
        {
            currentState = AIState.HuntingForFood;
            return; // Exit so we don't override this decision
        }

        // SECOND PRIORITY: If in a forest and haven't built a house yet...
        if (currentBiome == BiomeType.Forest && !hasBuiltHouse && currentState != AIState.DoSomethingStupid)
        {
            if (GetAmount("wood") < woodNeededForHouse)
            {
                currentState = AIState.CollectingWood;
            }
            else // We have enough wood
            {
                currentState = AIState.BuildingHouse;
            }
            return;
        }

        // DEFAULT STATE: If no other needs, just roam.
        currentState = AIState.Roaming;
    }


    /// <summary>
    /// A simple switch to run the logic for the current state.
    /// </summary>
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
    #endregion

    #region State Behaviors

   private void ExecuteDoStupid()
    {
        
        // If we don't have a target or we're stuck on current target
        if (currentTarget == null)
        {
            FindTargetStupidly(new string[] { "water", "obstacle" });
        }
        
        // Check if we're stuck (not moving towards target)
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) < stopDistance + 0.5f)
        {
            // We've reached the stupid target, transition back to roaming
            currentTarget = null;
            currentState = AIState.Roaming;
            hasRoamTarget = false;
            return;
        }
        
        MoveTowardsTarget();
    }
    private void ExecuteHuntingState()
    {
        // Find the closest food source
        FindTarget(seekableFoodTags);
        MoveTowardsTarget();
    }

    private void ExecuteCollectingWoodState()
    {
        // **MODIFICATION: Use the "stupid" targeting to find wood, ignoring obstacles.**
        // This will make the AI walk into water or other hazards if the wood is on the other side.
        if (currentTarget == null)
        {
            FindTargetStupidly(new string[] { woodSourceTag });
        }

        if (currentTarget == null)
        {
            // No wood found; fallback to roaming
            currentState = AIState.Roaming;
            hasRoamTarget = false;  // force pick new roam target
            return;
        }

        MoveTowardsTarget();

        // Only change state when we've reached the wood target
        if (Vector3.Distance(transform.position, currentTarget.position) <= stopDistance)
        {
            currentTarget = null; // Clear target
            currentState = AIState.DoSomethingStupid;
        }
    }

    private void ExecuteBuildingState()
    {
        // Step 1: If we haven't started building, create a construction site.
        if (houseConstructionSite == null)
        {
            houseConstructionSite = Instantiate(houseConstructionPrefab, transform.position + (transform.forward * 2), Quaternion.identity).transform;
            Debug.Log("Starting to build a house!");
        }

        currentTarget = houseConstructionSite;
        MoveTowardsTarget();

        // Step 2: If we are at the site, "build" it.
        if (Vector3.Distance(transform.position, houseConstructionSite.position) < stopDistance + 1f)
        {
            // In a real game, this would take time. We'll just finish it instantly.
            Instantiate(finishedHousePrefab, houseConstructionSite.position, houseConstructionSite.rotation);
            Destroy(houseConstructionSite.gameObject);
            hasBuiltHouse = true;
            Debug.Log("House finished!");
        }
    }

    #region Encourage
    private void ImplementEncourage()
    {
        if (string.IsNullOrEmpty(_encouragedStat) || _encouragedStatTime <= 0)
            return;

        switch (_encouragedStat.ToLower())
        {
            case "strength":
                Debug.Log($"{gameObject.name} feels strong and wants to BUILD something!");
                // Trigger build behavior or task
                break;

            case "resistanceiliness":
            case "resistanceenv":
                Debug.Log($"{gameObject.name} feels tough and wants to EXPLORE dangerous areas!");
                // Trigger explore behavior or movement to hazardous zones
                break;

            case "fly":
                Debug.Log($"{gameObject.name} feels like FLYING!");
                // Trigger flying logic or movement toward high points
                break;

            case "drive":
                Debug.Log($"{gameObject.name} feels like DRIVING through water or terrain!");
                // Trigger driving/movement toward water or vehicle
                break;

            case "intelligent":
                Debug.Log($"{gameObject.name} feels smarter and wants to LEARN more!");
                // Trigger interaction with books, NPCs, training, etc.
                break;

            default:
                Debug.Log($"{gameObject.name} has encouragement for unknown stat: {_encouragedStat}");
                break;
        }

        _encouragedStatTime--;

        // Clear encouragement when time runs out
        if (_encouragedStatTime <= 0)
        {
            Debug.Log($"{gameObject.name}'s encouraged state for {_encouragedStat} has ended.");
            _encouragedStat = null;
        }
    }

    #endregion

    private void ExecuteRoamingState()
    {
        // Check if we need a new roam target
        if (!hasRoamTarget || Time.time > nextRoamTime || Vector3.Distance(transform.position, roamTarget) < stopDistance)
        {
            SetNewRoamTarget();
        }

        // Move toward roamTarget
        if (hasRoamTarget && Vector3.Distance(transform.position, roamTarget) > stopDistance)
        {
            Vector3 direction = (roamTarget - transform.position).normalized;
            transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
        }


    }

    private void SetNewRoamTarget()
    {
        // First, try to find an interesting object to roam towards using vision
        Transform interestingTarget = FindInterestingRoamTarget();

        if (interestingTarget != null)
        {
            roamTarget = interestingTarget.position;
            hasRoamTarget = true;
            nextRoamTime = Time.time + roamDelay;
            // Debug.Log("Roaming towards: " + interestingTarget.name);
        }
        else
        {
            // Fallback: Pick a random point around the anchor
            Vector2 randomCircle = Random.insideUnitCircle * roamRadius;
            roamTarget = roamAnchorPoint + new Vector3(randomCircle.x, 0, randomCircle.y);
            hasRoamTarget = true;
            nextRoamTime = Time.time + roamDelay;
            // Debug.Log("Roaming to random point");
        }
    }

    private Transform FindInterestingRoamTarget()
    {
        List<GameObject> visibleObjects = vision.GetGameObjectInSight();

        if (visibleObjects.Count == 0)
            return null;

        List<GameObject> validTargets = new List<GameObject>();

        foreach (GameObject obj in visibleObjects)
        {
            if (obj.CompareTag("obstacle"))
                continue;

            if (Vector3.Distance(roamAnchorPoint, obj.transform.position) > roamRadius)
                continue;

            if (roamInterestTags.Length > 0 && !System.Array.Exists(roamInterestTags, tag => obj.CompareTag(tag)))
                continue;

            // Check for obstacles in the path using raycast
            Vector3 dir = (obj.transform.position - transform.position).normalized;
            float dist = Vector3.Distance(transform.position, obj.transform.position);

            if (!Physics.Raycast(transform.position, dir, dist, LayerMask.GetMask("obstacle")) && _intelligent > 60f)
            {
                validTargets.Add(obj);
            }
            else
            {
                Debug.Log($"Blocked path to {obj.name}");
            }
        }

        if (validTargets.Count > 0)
        {
            return validTargets[Random.Range(0, validTargets.Count)].transform;
        }

        return null;
    }
    #endregion

    #region Helper Methods
    
    /// <summary>
    /// **NEW METHOD:** Finds a target without checking for obstacles.
    /// This is the "stupid" version used to recklessly go after a goal.
    /// </summary>
   private void FindTargetStupidly(string[] tagsToFind)
    {
        currentTarget = null;
        float closestDist = float.MaxValue;
        GameObject closestTarget = null;

        // Use vision that ignores obstacles for "stupid" targeting
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
        {
            Debug.Log($"Found a {closestTarget.tag} to go to, ignoring all obstacles!");
            currentTarget = closestTarget.transform;
        }
    }


    
    /// <summary>
    /// Finds the first available target with a valid tag.
    /// </summary>
    private void FindTarget(string[] tagsToFind)
    {
        currentTarget = null; // Clear previous target
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
            if (Vector3.Distance(currentTarget.position, transform.position) > stopDistance)
            {
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
            }
        }
    }

    // Called by BiomeZone
    public void OnEnterBiome(BiomeType biome)
    {
        currentBiome = biome;
        Debug.Log("Entered " + biome);
    }

    // Called by BiomeZone
    public void OnExitBiome(BiomeType biome)
    {
        if (currentBiome == biome)
        {
            currentBiome = BiomeType.None;
            Debug.Log("Exited " + biome);
        }
    }
    #endregion

    #region Stat
    public void ModifyStat(StatDelta delta)
    {
        ApplyStatDelta(delta);

        Debug.Log($"{name} stat modified. New stats: " +
            $"Health: {_health}, Strength: {_strength}, Fly: {_fly}, Drive: {_drive}");
    }
    #endregion
}