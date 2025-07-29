using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RuntimeLogging.Examples
{
    /// <summary>
    /// Example business code demonstrating ILogger usage patterns with LogDisplay
    /// This class shows how to integrate the logging system into typical game systems
    /// </summary>
    public class DemoBusinessLogic : MonoBehaviour
    {
        [Header("Demo Configuration")]
        [SerializeField] private float simulationSpeed = 1f;
        [SerializeField] private bool enablePlayerSimulation = true;
        [SerializeField] private bool enableNetworkSimulation = true;
        [SerializeField] private bool enableResourceSimulation = true;
        [SerializeField] private bool enableErrorSimulation = true;
        
        private ILogger logger;
        private PlayerSystem playerSystem;
        private NetworkSystem networkSystem;
        private ResourceSystem resourceSystem;
        private bool isRunning = false;
        
        private void Start()
        {
            // Get logger from LogManager (should be set up by DemoSceneController)
            logger = LogManager.GetLogger();
            
            if (logger == null)
            {
                Debug.LogError("No logger found in LogManager. Make sure DemoSceneController is initialized first.");
                return;
            }
            
            logger.Log("DemoBusinessLogic: Starting business logic simulation");
            
            // Initialize subsystems
            InitializeSubsystems();
            
            // Start simulation
            StartSimulation();
        }
        
        private void InitializeSubsystems()
        {
            logger.Log("DemoBusinessLogic: Initializing subsystems");
            
            // Initialize player system
            if (enablePlayerSimulation)
            {
                playerSystem = new PlayerSystem(logger);
                logger.Log("PlayerSystem initialized");
            }
            
            // Initialize network system
            if (enableNetworkSimulation)
            {
                networkSystem = new NetworkSystem(logger);
                logger.Log("NetworkSystem initialized");
            }
            
            // Initialize resource system
            if (enableResourceSimulation)
            {
                resourceSystem = new ResourceSystem(logger);
                logger.Log("ResourceSystem initialized");
            }
            
            logger.Log("DemoBusinessLogic: All subsystems initialized successfully");
        }
        
        private void StartSimulation()
        {
            logger.Log("DemoBusinessLogic: Starting simulation");
            isRunning = true;
            
            // Start coroutines for different systems
            if (enablePlayerSimulation && playerSystem != null)
            {
                StartCoroutine(PlayerSimulationCoroutine());
            }
            
            if (enableNetworkSimulation && networkSystem != null)
            {
                StartCoroutine(NetworkSimulationCoroutine());
            }
            
            if (enableResourceSimulation && resourceSystem != null)
            {
                StartCoroutine(ResourceSimulationCoroutine());
            }
            
            if (enableErrorSimulation)
            {
                StartCoroutine(ErrorSimulationCoroutine());
            }
        }
        
        private IEnumerator PlayerSimulationCoroutine()
        {
            while (isRunning)
            {
                yield return new WaitForSeconds(2f / simulationSpeed);
                playerSystem.SimulatePlayerAction();
                
                yield return new WaitForSeconds(5f / simulationSpeed);
                playerSystem.SimulateHealthChange();
                
                yield return new WaitForSeconds(8f / simulationSpeed);
                playerSystem.SimulatePlayerMovement();
            }
        }
        
        private IEnumerator NetworkSimulationCoroutine()
        {
            while (isRunning)
            {
                yield return new WaitForSeconds(3f / simulationSpeed);
                networkSystem.SimulateNetworkActivity();
                
                yield return new WaitForSeconds(7f / simulationSpeed);
                networkSystem.SimulateLatencyCheck();
                
                yield return new WaitForSeconds(15f / simulationSpeed);
                networkSystem.SimulateConnectionIssue();
            }
        }
        
        private IEnumerator ResourceSimulationCoroutine()
        {
            while (isRunning)
            {
                yield return new WaitForSeconds(4f / simulationSpeed);
                resourceSystem.SimulateResourceLoad();
                
                yield return new WaitForSeconds(6f / simulationSpeed);
                resourceSystem.SimulateMemoryCheck();
                
                yield return new WaitForSeconds(12f / simulationSpeed);
                resourceSystem.SimulateResourceCleanup();
            }
        }
        
        private IEnumerator ErrorSimulationCoroutine()
        {
            while (isRunning)
            {
                yield return new WaitForSeconds(10f / simulationSpeed);
                SimulateRandomError();
                
                yield return new WaitForSeconds(20f / simulationSpeed);
                SimulateWarningCondition();
            }
        }
        
        private void SimulateRandomError()
        {
            string[] errorMessages = {
                "Failed to save game state",
                "Audio system initialization failed",
                "Shader compilation error",
                "Network timeout occurred",
                "Memory allocation failed"
            };
            
            string error = errorMessages[Random.Range(0, errorMessages.Length)];
            logger.LogError($"DemoBusinessLogic: {error}");
        }
        
        private void SimulateWarningCondition()
        {
            string[] warningMessages = {
                "Frame rate dropped below 30 FPS",
                "Memory usage above 80%",
                "Network latency high",
                "Texture quality reduced for performance",
                "Audio buffer underrun detected"
            };
            
            string warning = warningMessages[Random.Range(0, warningMessages.Length)];
            logger.LogWarning($"DemoBusinessLogic: {warning}");
        }
        
        public void StopSimulation()
        {
            if (logger != null)
            {
                logger.Log("DemoBusinessLogic: Stopping simulation");
            }
            isRunning = false;
            StopAllCoroutines();
        }
        
        public void SetSimulationSpeed(float speed)
        {
            simulationSpeed = Mathf.Clamp(speed, 0.1f, 5f);
            logger.Log($"DemoBusinessLogic: Simulation speed set to {simulationSpeed:F1}x");
        }
        
        private void OnDestroy()
        {
            StopSimulation();
            if (logger != null)
            {
                logger.Log("DemoBusinessLogic: Component destroyed");
            }
        }
    }
    
    /// <summary>
    /// Example player system showing typical game logging patterns
    /// </summary>
    public class PlayerSystem
    {
        private ILogger logger;
        private int playerHealth = 100;
        private Vector3 playerPosition = Vector3.zero;
        private int playerLevel = 1;
        private int playerExperience = 0;
        
        public PlayerSystem(ILogger logger)
        {
            this.logger = logger;
            logger.Log("PlayerSystem: Player system initialized");
        }
        
        public void SimulatePlayerAction()
        {
            string[] actions = {
                "Player attacked enemy",
                "Player collected item",
                "Player opened chest",
                "Player completed quest",
                "Player used skill"
            };
            
            string action = actions[Random.Range(0, actions.Length)];
            logger.Log($"PlayerSystem: {action}");
            
            // Simulate experience gain
            int expGain = Random.Range(10, 50);
            playerExperience += expGain;
            logger.Log($"PlayerSystem: Gained {expGain} experience (Total: {playerExperience})");
            
            // Check for level up
            if (playerExperience >= playerLevel * 100)
            {
                playerLevel++;
                logger.Log($"PlayerSystem: Level up! Now level {playerLevel}");
            }
        }
        
        public void SimulateHealthChange()
        {
            int healthChange = Random.Range(-30, 20);
            int newHealth = Mathf.Clamp(playerHealth + healthChange, 0, 100);
            
            if (healthChange < 0)
            {
                logger.LogWarning($"PlayerSystem: Player took {-healthChange} damage (Health: {newHealth}/100)");
            }
            else if (healthChange > 0)
            {
                logger.Log($"PlayerSystem: Player healed {healthChange} HP (Health: {newHealth}/100)");
            }
            
            playerHealth = newHealth;
            
            if (playerHealth <= 0)
            {
                logger.LogError("PlayerSystem: Player died!");
                playerHealth = 100; // Respawn
                logger.Log("PlayerSystem: Player respawned");
            }
            else if (playerHealth < 25)
            {
                logger.LogWarning($"PlayerSystem: Player health critical: {playerHealth}%");
            }
        }
        
        public void SimulatePlayerMovement()
        {
            Vector3 movement = new Vector3(
                Random.Range(-10f, 10f),
                0,
                Random.Range(-10f, 10f)
            );
            
            playerPosition += movement;
            logger.Log($"PlayerSystem: Player moved to position {playerPosition:F1}");
            
            // Check for special areas
            if (playerPosition.magnitude > 50f)
            {
                logger.LogWarning("PlayerSystem: Player entered dangerous area");
            }
        }
    }
    
    /// <summary>
    /// Example network system showing network-related logging patterns
    /// </summary>
    public class NetworkSystem
    {
        private ILogger logger;
        private bool isConnected = true;
        private float currentLatency = 50f;
        private int packetsLost = 0;
        
        public NetworkSystem(ILogger logger)
        {
            this.logger = logger;
            logger.Log("NetworkSystem: Network system initialized");
        }
        
        public void SimulateNetworkActivity()
        {
            if (!isConnected)
            {
                // Try to reconnect
                if (Random.Range(0f, 1f) < 0.3f)
                {
                    isConnected = true;
                    logger.Log("NetworkSystem: Connection restored");
                }
                else
                {
                    logger.LogError("NetworkSystem: Still disconnected, retrying...");
                    return;
                }
            }
            
            string[] activities = {
                "Sent player position update",
                "Received game state sync",
                "Processed chat message",
                "Updated leaderboard",
                "Synchronized inventory"
            };
            
            string activity = activities[Random.Range(0, activities.Length)];
            logger.Log($"NetworkSystem: {activity}");
        }
        
        public void SimulateLatencyCheck()
        {
            currentLatency = Random.Range(20f, 200f);
            
            if (currentLatency > 150f)
            {
                logger.LogWarning($"NetworkSystem: High latency detected: {currentLatency:F0}ms");
            }
            else if (currentLatency > 100f)
            {
                logger.LogWarning($"NetworkSystem: Moderate latency: {currentLatency:F0}ms");
            }
            else
            {
                logger.Log($"NetworkSystem: Latency normal: {currentLatency:F0}ms");
            }
        }
        
        public void SimulateConnectionIssue()
        {
            if (Random.Range(0f, 1f) < 0.2f) // 20% chance of connection issue
            {
                isConnected = false;
                logger.LogError("NetworkSystem: Connection lost!");
                
                packetsLost += Random.Range(1, 5);
                logger.LogError($"NetworkSystem: {packetsLost} packets lost total");
            }
            else if (Random.Range(0f, 1f) < 0.3f) // 30% chance of packet loss
            {
                int lostPackets = Random.Range(1, 3);
                packetsLost += lostPackets;
                logger.LogWarning($"NetworkSystem: {lostPackets} packets lost (Total: {packetsLost})");
            }
        }
    }
    
    /// <summary>
    /// Example resource system showing resource management logging patterns
    /// </summary>
    public class ResourceSystem
    {
        private ILogger logger;
        private List<string> loadedResources = new List<string>();
        private float memoryUsage = 0f;
        private const float maxMemoryUsage = 100f;
        
        public ResourceSystem(ILogger logger)
        {
            this.logger = logger;
            logger.Log("ResourceSystem: Resource system initialized");
        }
        
        public void SimulateResourceLoad()
        {
            string[] resourceTypes = {
                "texture_player_idle.png",
                "audio_background_music.ogg",
                "model_environment_tree.fbx",
                "shader_water_surface.shader",
                "animation_player_walk.anim"
            };
            
            string resource = resourceTypes[Random.Range(0, resourceTypes.Length)];
            
            if (!loadedResources.Contains(resource))
            {
                loadedResources.Add(resource);
                float resourceSize = Random.Range(1f, 15f);
                memoryUsage += resourceSize;
                
                logger.Log($"ResourceSystem: Loaded {resource} ({resourceSize:F1}MB)");
                logger.Log($"ResourceSystem: Total memory usage: {memoryUsage:F1}MB / {maxMemoryUsage}MB");
                
                if (memoryUsage > maxMemoryUsage * 0.8f)
                {
                    logger.LogWarning($"ResourceSystem: Memory usage high: {memoryUsage:F1}MB ({(memoryUsage/maxMemoryUsage)*100:F0}%)");
                }
            }
            else
            {
                logger.Log($"ResourceSystem: {resource} already loaded (cached)");
            }
        }
        
        public void SimulateMemoryCheck()
        {
            float memoryPercentage = (memoryUsage / maxMemoryUsage) * 100f;
            
            if (memoryPercentage > 90f)
            {
                logger.LogError($"ResourceSystem: Memory usage critical: {memoryPercentage:F0}%");
            }
            else if (memoryPercentage > 75f)
            {
                logger.LogWarning($"ResourceSystem: Memory usage high: {memoryPercentage:F0}%");
            }
            else
            {
                logger.Log($"ResourceSystem: Memory usage normal: {memoryPercentage:F0}%");
            }
        }
        
        public void SimulateResourceCleanup()
        {
            if (loadedResources.Count > 0 && memoryUsage > maxMemoryUsage * 0.6f)
            {
                int resourcesToUnload = Random.Range(1, Mathf.Min(3, loadedResources.Count));
                float memoryFreed = 0f;
                
                for (int i = 0; i < resourcesToUnload; i++)
                {
                    string resource = loadedResources[0];
                    loadedResources.RemoveAt(0);
                    
                    float resourceSize = Random.Range(1f, 15f);
                    memoryFreed += resourceSize;
                    memoryUsage = Mathf.Max(0f, memoryUsage - resourceSize);
                    
                    logger.Log($"ResourceSystem: Unloaded {resource} ({resourceSize:F1}MB freed)");
                }
                
                logger.Log($"ResourceSystem: Cleanup complete. {memoryFreed:F1}MB freed. Current usage: {memoryUsage:F1}MB");
            }
        }
    }
}