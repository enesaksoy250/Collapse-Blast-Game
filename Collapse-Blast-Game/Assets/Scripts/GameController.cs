using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CollapseBlast.Config;
using CollapseBlast.Core;
using CollapseBlast.Data;
using CollapseBlast.Managers;
using CollapseBlast.Handlers;
using CollapseBlast.Input;

namespace CollapseBlast
{
    /// <summary>
    /// Main game controller that orchestrates the game flow.
    /// Manages game state and coordinates between all systems.
    /// </summary>
    public class GameController : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField]
        private GameConfig gameConfig;

        [Header("References")]
        [SerializeField]
        private GridManager gridManager;

        [SerializeField]
        private InputHandler inputHandler;

        // Handlers
        private BlastHandler blastHandler;
        private GravityHandler gravityHandler;
        private FillHandler fillHandler;
        private GroupVisualUpdater groupVisualUpdater;
        private DeadlockDetector deadlockDetector;
        private SmartShuffler smartShuffler;

        // Game state
        private GameState currentState;
        private bool isProcessing;

        // Animation timing
        private float settleDelay = 0.35f;

        // Events
        public event System.Action OnGameStarted;
        public event System.Action OnMoveCompleted;
        public event System.Action OnShuffleTriggered;

        private enum GameState
        {
            Idle,
            Processing,
            Settling,
            Shuffling
        }

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            InitializeGame();
        }

        /// <summary>
        /// Validates required references.
        /// </summary>
        private void ValidateReferences()
        {
            if (gameConfig == null)
                Debug.LogError("GameController: GameConfig is not assigned!");
            if (gridManager == null)
                Debug.LogError("GameController: GridManager is not assigned!");
            if (inputHandler == null)
                Debug.LogError("GameController: InputHandler is not assigned!");
        }

        /// <summary>
        /// Initializes the game.
        /// </summary>
        public void InitializeGame()
        {
            // Initialize grid manager
            gridManager.Initialize();

            // Create handlers
            blastHandler = new BlastHandler(gridManager);
            gravityHandler = new GravityHandler(gridManager, gameConfig);
            fillHandler = new FillHandler(gridManager, gameConfig);
            groupVisualUpdater = new GroupVisualUpdater(gridManager, gameConfig);

            // Create deadlock detection and shuffle
            deadlockDetector = new DeadlockDetector(gridManager.GridData, gridManager.GroupFinder);
            smartShuffler = new SmartShuffler(
                gridManager.GridData,
                gridManager.GroupFinder,
                deadlockDetector,
                gameConfig.colorCount
            );

            // Subscribe to events
            inputHandler.OnPositionClicked += OnBlockClicked;
            smartShuffler.OnShuffleCompleted += OnShuffleComplete;

            // Create initial grid
            gridManager.CreateGrid();

            // Update visual states
            groupVisualUpdater.UpdateAllIcons();

            // Check for initial deadlock
            CheckDeadlock();

            currentState = GameState.Idle;
            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// Handles block click events.
        /// </summary>
        private void OnBlockClicked(Vector2Int position)
        {
            if (currentState != GameState.Idle || isProcessing)
                return;

            // Find the group at clicked position
            List<Vector2Int> group = gridManager.GroupFinder.FindGroup(position.y, position.x);

            // Check if valid group (at least 2 blocks)
            if (group.Count < 2)
                return;

            // Start processing
            StartCoroutine(ProcessMove(group));
        }

        /// <summary>
        /// Processes a move (blast, gravity, fill, check deadlock).
        /// </summary>
        private IEnumerator ProcessMove(List<Vector2Int> group)
        {
            isProcessing = true;
            currentState = GameState.Processing;
            inputHandler.SetInputEnabled(false);

            // Get affected columns before blasting
            HashSet<int> affectedColumns = blastHandler.GetAffectedColumns(group);

            // Blast the group
            blastHandler.BlastGroup(group);

            // Wait a frame for blocks to be removed
            yield return null;

            // Apply gravity
            currentState = GameState.Settling;
            gravityHandler.ApplyGravity(affectedColumns);

            // Wait for gravity animation
            yield return new WaitForSeconds(settleDelay);

            // Fill empty cells
            fillHandler.FillColumns(affectedColumns);

            // Wait for fill animation
            yield return new WaitForSeconds(settleDelay);

            // Update icon states
            groupVisualUpdater.UpdateAllIcons();

            // Check for deadlock
            bool isDeadlock = CheckDeadlock();

            if (isDeadlock)
            {
                // Handle deadlock
                yield return StartCoroutine(HandleDeadlock());
            }

            // Done processing
            currentState = GameState.Idle;
            isProcessing = false;
            inputHandler.SetInputEnabled(true);
            OnMoveCompleted?.Invoke();
        }

        /// <summary>
        /// Checks for deadlock condition.
        /// </summary>
        private bool CheckDeadlock()
        {
            return deadlockDetector.QuickDeadlockCheck();
        }

        /// <summary>
        /// Handles deadlock by shuffling the grid.
        /// </summary>
        private IEnumerator HandleDeadlock()
        {
            currentState = GameState.Shuffling;
            OnShuffleTriggered?.Invoke();

            // Small delay before shuffle for visual feedback
            yield return new WaitForSeconds(0.5f);

            // Perform smart shuffle
            smartShuffler.Shuffle();

            // Refresh the visual representation
            RefreshGridVisuals();

            // Update icons after shuffle
            groupVisualUpdater.UpdateAllIcons();

            yield return new WaitForSeconds(0.3f);
        }

        /// <summary>
        /// Called when shuffle is complete.
        /// </summary>
        private void OnShuffleComplete()
        {
            Debug.Log("Shuffle completed - grid now has valid moves");
        }

        /// <summary>
        /// Refreshes the visual representation of the grid after shuffle.
        /// </summary>
        private void RefreshGridVisuals()
        {
            // Update each block view to match the shuffled data
            for (int row = 0; row < gridManager.RowCount; row++)
            {
                for (int col = 0; col < gridManager.ColumnCount; col++)
                {
                    BlockData blockData = gridManager.GridData.GetBlock(row, col);
                    Block.BlockView blockView = gridManager.GetBlockView(row, col);

                    if (blockView != null && !blockData.isEmpty)
                    {
                        // Re-initialize with new color from shuffled data
                        blockView.Initialize(
                            blockData.colorIndex,
                            row,
                            col,
                            gridManager.ColorDataArray[blockData.colorIndex]
                        );
                    }
                }
            }
        }

        /// <summary>
        /// Restarts the game.
        /// </summary>
        public void RestartGame()
        {
            StopAllCoroutines();
            gridManager.ClearGrid();
            InitializeGame();
        }

        private void OnDestroy()
        {
            if (inputHandler != null)
            {
                inputHandler.OnPositionClicked -= OnBlockClicked;
            }

            if (smartShuffler != null)
            {
                smartShuffler.OnShuffleCompleted -= OnShuffleComplete;
            }
        }
    }
}
