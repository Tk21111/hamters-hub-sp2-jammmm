using System.Collections.Generic;
using UnityEngine;

// Define the possible states for our AI
public enum AIState
{
    Idle,
    Roaming,
    HuntingForFood,
    CollectingWood,
    BuildingHouse
}

[RequireComponent(typeof(Vision), typeof(Stat))]
public class BaseAI : MonoBehaviour
{
    [Header("State Machine")]
    [SerializeField] private AIState currentState = AIState.Roaming;

    [Header("Behavior: Hunting")]
    public string[] seekableFoodTags;

    [Header("Behavior: Building")]
    public string woodSourceTag = "WoodSource";
    public int woodNeededForHouse = 10;
    public GameObject houseConstructionPrefab;
    public GameObject finishedHousePrefab;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float stopDistance = 1f;

    [Header("Behavior: Roaming")]
    public float roamRadius = 10f;
    public float roamDelay = 3f;
    public string[] roamInterestTags = { "tree", "water" , "npc" };

    private Vector3 roamTarget;
    private float nextRoamTime = 0f;
    private Vector3 roamAnchorPoint;
    private bool hasRoamTarget = false;

    // Internal State
    public BiomeType currentBiome = BiomeType.None;
    private int woodCollected = 0;
    private bool hasBuiltHouse = false;
    private Transform currentTarget;
    private Transform houseConstructionSite;

    // Components
    protected Vision vision;
    protected Stat stat;

    #region Unity Lifecycle
    protected virtual void Awake()
    {
        vision = GetComponent<Vision>();
        stat = GetComponent<Stat>();
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
        // HIGHEST PRIORITY: Find food if health is low.
        if (stat._health < 50f)
        {
            currentState = AIState.HuntingForFood;
            return; // Exit so we don't override this decision
        }

        // SECOND PRIORITY: If in a forest and haven't built a house yet...
        if (currentBiome == BiomeType.Forest && !hasBuiltHouse)
        {
            if (woodCollected < woodNeededForHouse)
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
        }
    }
    #endregion

    #region State Behaviors
    private void ExecuteHuntingState()
    {
        // Find the closest food source
        FindTarget(seekableFoodTags);
        MoveTowardsTarget();
    }

    private void ExecuteCollectingWoodState()
    {
        FindTarget(new string[] { woodSourceTag });
        MoveTowardsTarget();

        // If we've reached the wood source, "collect" it.
        if (currentTarget != null && Vector3.Distance(transform.position, currentTarget.position) < stopDistance)
        {
            woodCollected++;
            Destroy(currentTarget.gameObject); // Consume the wood source
            currentTarget = null; // Look for a new one
            Debug.Log("Collected wood! Now have: " + woodCollected);
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
            Debug.Log("Roaming towards: " + interestingTarget.name);
        }
        else
        {
            // Fallback: Pick a random point around the anchor
            Vector2 randomCircle = Random.insideUnitCircle * roamRadius;
            roamTarget = roamAnchorPoint + new Vector3(randomCircle.x, 0, randomCircle.y);
            hasRoamTarget = true;
            nextRoamTime = Time.time + roamDelay;
            Debug.Log("Roaming to random point");
        }
    }

    private Transform FindInterestingRoamTarget()
    {
        List<GameObject> visibleObjects = vision.GetGameObjectInSight();

        if (visibleObjects.Count == 0)
            return null;

        // Look for objects with interesting tags
        List<GameObject> matchTag = new List<GameObject>();
        foreach (GameObject obj in visibleObjects)
        {
            foreach (string tag in roamInterestTags)
            {
                if (obj.CompareTag(tag))
                {
                    // Check if it's within roam radius from anchor point
                    if (Vector3.Distance(roamAnchorPoint, obj.transform.position) <= roamRadius)
                    {
                        matchTag.Add(obj);
                    }
                }
            }
        }

        // If no interesting tagged objects, just pick a random visible object within range
        foreach (GameObject obj in visibleObjects)
        {
            if (Vector3.Distance(roamAnchorPoint, obj.transform.position) <= roamRadius)
            {
                return obj.transform;
            }
        }

        return null;
    }

    #endregion

    #region Helper Methods
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
}