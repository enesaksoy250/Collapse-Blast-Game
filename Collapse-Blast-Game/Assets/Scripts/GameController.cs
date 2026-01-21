using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class GameController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField]
    private GameConfig gameConfig;

    [Header("References")]
    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private InputHandler inputHandler;

    private BlastHandler blastHandler;
    private GravityHandler gravityHandler;
    private FillHandler fillHandler;
    private GroupVisualUpdater groupVisualUpdater;
    private DeadlockDetector deadlockDetector;
    private SmartShuffler smartShuffler;

    private GameState currentState;
    private bool isProcessing;

    public event Action OnGameStarted;
    public event Action OnMoveCompleted;
    public event Action OnShuffleTriggered;

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

    private void ValidateReferences()
    {
        if (gameConfig == null)
            Debug.LogError("GameController: GameConfig is not assigned!");
        if (gridManager == null)
            Debug.LogError("GameController: GridManager is not assigned!");
        if (inputHandler == null)
            Debug.LogError("GameController: InputHandler is not assigned!");
    }

    public void InitializeGame()
    {
        gridManager.Initialize();

        blastHandler = new BlastHandler(gridManager);
        gravityHandler = new GravityHandler(gridManager);
        fillHandler = new FillHandler(gridManager);
        groupVisualUpdater = new GroupVisualUpdater(gridManager, gameConfig);

        deadlockDetector = new DeadlockDetector(gridManager.GridData, gridManager.GroupFinder);
        smartShuffler = new SmartShuffler(
            gridManager.GridData,
            deadlockDetector
        );

        inputHandler.OnPositionClicked += OnBlockClicked;
        smartShuffler.OnShuffleCompleted += OnShuffleComplete;

        gridManager.CreateGrid();
        groupVisualUpdater.UpdateAllIcons();


        if (CheckDeadlock())
        {
            smartShuffler.Shuffle();
            RefreshGridVisuals();
            groupVisualUpdater.UpdateAllIcons();
        }

        currentState = GameState.Idle;
        OnGameStarted?.Invoke();
    }

    private void OnBlockClicked(Vector2Int position)
    {
        if (currentState != GameState.Idle || isProcessing)
            return;

        List<Vector2Int> group = gridManager.GroupFinder.FindGroup(position.y, position.x);

        if (group.Count < 2)
            return;

        StartCoroutine(ProcessMove(group));
    }

    private IEnumerator ProcessMove(List<Vector2Int> group)
    {
        isProcessing = true;
        currentState = GameState.Processing;
        inputHandler.SetInputEnabled(false);

        HashSet<int> affectedColumns = blastHandler.GetAffectedColumns(group);

        blastHandler.BlastGroup(group);

        yield return null;

        currentState = GameState.Settling;
        int maxFallDistance = gravityHandler.ApplyGravity(affectedColumns);

        float gravityWaitTime = CalculateFallTime(maxFallDistance);
        if (gravityWaitTime > 0)
        {
            yield return new WaitForSeconds(gravityWaitTime);
        }

        int maxSpawnDistance = fillHandler.FillColumns(affectedColumns);

        float fillWaitTime = CalculateFallTime(maxSpawnDistance + 1);
        if (fillWaitTime > 0)
        {
            yield return new WaitForSeconds(fillWaitTime);
        }

        groupVisualUpdater.UpdateAllIcons();

        bool isDeadlock = CheckDeadlock();

        if (isDeadlock)
        {
            yield return StartCoroutine(HandleDeadlock());
        }

        currentState = GameState.Idle;
        isProcessing = false;
        inputHandler.SetInputEnabled(true);
        OnMoveCompleted?.Invoke();
    }

    private float CalculateFallTime(int rowDistance)
    {
        if (rowDistance <= 0) return 0f;

        float distance = rowDistance * 1.2f;
        float gravity = gameConfig.gravity;
        float time = Mathf.Sqrt(2f * distance / gravity);

        return time + 0.05f;
    }

    private bool CheckDeadlock()
    {
        return deadlockDetector.QuickDeadlockCheck();
    }

    private IEnumerator HandleDeadlock()
    {
        currentState = GameState.Shuffling;
        OnShuffleTriggered?.Invoke();

        yield return new WaitForSeconds(0.25f);

        smartShuffler.Shuffle();
        RefreshGridVisuals();
        groupVisualUpdater.UpdateAllIcons();

        yield return null;
    }

    private void OnShuffleComplete()
    {
        Debug.Log("Shuffle completed - grid now has valid moves");
    }

    private void RefreshGridVisuals()
    {
        for (int row = 0; row < gridManager.RowCount; row++)
        {
            for (int col = 0; col < gridManager.ColumnCount; col++)
            {
                BlockData blockData = gridManager.GridData.GetBlock(row, col);
                Block blockView = gridManager.GetBlockView(row, col);

                if (blockView != null && !blockData.isEmpty)
                {
                    blockView.Initialize(
                        row,
                        col,
                        gridManager.ColorDataArray[blockData.colorIndex], gameConfig
                    );
                }
            }
        }
    }

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
