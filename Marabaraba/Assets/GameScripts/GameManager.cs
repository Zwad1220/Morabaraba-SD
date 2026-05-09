using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    // Singleton instance so other scripts can easily access GameManager
    public static GameManager instance;

    [Header("Board Setup")]
    public Node[] allNodes; // All 24 nodes on the board

    [Header("UI References")]// References to UI text elements for dynamic updates
    public TextMeshProUGUI instructionText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI phaseText;
    public TextMeshProUGUI winText;
    public GameObject winScreen;

    [Header("Piece Counter UI")]// Displays pieces left to place during placement phase, and pieces left on board during movement phase
    public TextMeshProUGUI p1PiecesText;
    public TextMeshProUGUI p2PiecesText;

    [Header("Placement Counters")]// Tracks how many pieces each player has left to place on the board  
    public int p1PiecesToPlace = 12;
    public int p2PiecesToPlace = 12;
    // Tracks remaining pieces (used for flying + win condition)
    public int p1PiecesLeft = 0;
    public int p2PiecesLeft = 0;

    [Header("Player Colors")]// Base colors for each player's pieces
    public Color p1BaseColor = Color.red;
    public Color p2BaseColor = Color.blue;

    // Highlight colors used when selecting pieces
    public Color p1GlowColor = Color.yellow;
    public Color p2GlowColor = Color.cyan;

    [Header("Game State")]
    public int currentPlayer = 1;   // Tracks whose turn it is
    private bool isCapturing = false; // allows player to capture a piece when valid
    public int piecesPlaced = 0;   // Used to detect end of placement phase
    public bool gameOver = false;
    public bool p1FlyingPhase = false;// Tracks if player 1 is in flying phase (3 or fewer pieces left)
    public bool p2FlyingPhase = false;// Tracks if player 2 is in flying phase (3 or fewer pieces left)

    public Color validCaptureColor = Color.magenta;// colour to change pieces to when they are valid capture targets (used in capture mode)

    // All valid mill combinations (3-in-a-row)
    public static readonly int[][] millLines = new int[][]
    {
        new[] {0,1,2}, new[] {3,4,5}, new[] {6,7,8},
        new[] {9,10,11}, new[] {12,13,14},
        new[] {15,16,17}, new[] {18,19,20}, new[] {21,22,23},

        new[] {0,9,21}, new[] {3,10,18}, new[] {6,11,15},
        new[] {1,4,7}, new[] {16,19,22},
        new[] {8,12,17}, new[] {5,13,20}, new[] {2,14,23},

        new[] {0,3,6}, new[] {2,5,8},
        new[] {21,18,15}, new[] {23,20,17}
    };

    void Awake()
    {
        instance = this;
        // Ensure nodes are ordered by ID 
        allNodes = allNodes.OrderBy(n => n.nodeID).ToArray();
    }

    void Start()
    {
        // Reset all nodes to their "Empty" state (White) at the start
        foreach (Node node in allNodes)
        {
            node.ClearNode();
        }

        if (instructionText != null)
        {
            instructionText.text = "Place a piece on an empty slot.";
        }
        UpdateTurnUI();
        UpdatePhaseUI("Placement Phase");
        UpdatePieceUI();
    }

 
    // Updates turn display and color
    public void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = "Current Player: " + currentPlayer;
            turnText.color = (currentPlayer == 1) ? p1BaseColor : p2BaseColor;
        }

        if (currentPlayer == 1 && p1FlyingPhase)
        {
            UpdatePhaseUI("Flying Phase");

            if (instructionText != null)
            {
                instructionText.text =
                    "Player " + currentPlayer +
                    ": Move a piece to any empty slot.";
            }
        }

        if (currentPlayer == 2 && p2FlyingPhase)
        {
            UpdatePhaseUI("Flying Phase");

            if (instructionText != null)
            {
                instructionText.text =
                    "Player " + currentPlayer +
                    ": Move a piece to any empty slot.";
            }
        }
    }

    public void UpdatePieceUI()
    {
        if (p1PiecesText == null || p2PiecesText == null)
            return;

        // Placement phase
        if (piecesPlaced < 24)
        {
            p1PiecesText.text =
                "Player 1 Pieces To Place: " + p1PiecesToPlace;

            p2PiecesText.text =
                "Player 2 Pieces To Place: " + p2PiecesToPlace;
        }
        else
        {
            p1PiecesText.text =
                "Player 1 Pieces Left: " + p1PiecesLeft;

            p2PiecesText.text =
                "Player 2 Pieces Left: " + p2PiecesLeft;
        }

        p1PiecesText.color = p1BaseColor;
        p2PiecesText.color = p2BaseColor;
    }

  
    // Updates phase display (Placement / Movement)
    public void UpdatePhaseUI(string phase)
    {
        if (phaseText == null) return;

        phaseText.text = "Current Phase: " + phase;
        phaseText.color = Color.white;
    }

 
    // Called whenever a piece is placed during placement phase
    public void OnPiecePlaced(Node node)
    {
        if (currentPlayer == 1)
        {
            p1PiecesToPlace--;// Decrease pieces left to place
            p1PiecesLeft++;// Increase pieces on board
        }
        else
        {
            p2PiecesToPlace--;// Decrease pieces left to place
            p2PiecesLeft++;// Increase pieces on board
        }

        UpdatePieceUI();

        piecesPlaced++;

        // Check if placement formed a mill
        CheckMillAndSwitchTurn(node);

        // Switch to movement phase after all pieces are placed
        if (piecesPlaced >= 24 && !isCapturing)
        {
            FindObjectOfType<PhaseState>().SwitchToMovementPhase();
        }
    }
   
    // Returns the mill (list of 3 node IDs) if one is formed, otherwise null
    List<int> GetMillFormed(Node node)
    {
        foreach (int[] line in millLines)
        {
            if (line.Contains(node.nodeID))
            {
                if (allNodes[line[0]].owner == node.owner &&
                    allNodes[line[1]].owner == node.owner &&
                    allNodes[line[2]].owner == node.owner)
                {
                    return line.ToList();
                }
            }
        }
        return null;
    }

    // Activates capture mode if a valid mill is formed

    void EnterCaptureMode(List<int> newMill)
    {

        isCapturing = true;

        instructionText.text = "Player " + currentPlayer + ": Capture a piece!";
        //instructionText.color = (currentPlayer == 1) ? p1BaseColor : p2BaseColor;
        SetCaptureHighlights(true);
    }

    /// <summary>
    /// Handles removing an opponent's piece
    /// </summary>
    public void TryCapture(Node node)
    {

        //Invalid capture checks
        if (node.owner == currentPlayer || node.owner == 0)
            return;

        //Cannot capture mill pieces if alternatives exist
        if (IsPartOfMill(node) && HasPiecesOutsideMills(node.owner))
            return;

        //Save Valid captures
        if (gameOver) return;
        UndoRedoManager.instance.SaveState();

        int capturedOwner = node.owner;

        node.ClearNode();

        // Update piece counts
        if (capturedOwner == 1)
            p1PiecesLeft--;
        else if (capturedOwner == 2)
            p2PiecesLeft--;
        UpdatePieceUI();

        Debug.Log($"P1: {p1PiecesLeft} | P2: {p2PiecesLeft}");

        

        // Win conditions
        if (piecesPlaced >= 24)
        {
            if (p1PiecesLeft == 3)
            {
                p1FlyingPhase = true;
            }
            if (p2PiecesLeft == 3)
            {
                p2FlyingPhase = true;
            }
            if (p1PiecesLeft <= 2)
            {
                gameOver = true;
                winScreen.SetActive(true);
                winText.text = "Player 2 wins!";
                winText.color = p2BaseColor;
            }

            if (p2PiecesLeft <= 2)
            {
                gameOver = true;
                winScreen.SetActive(true);
                winText.text = "Player 1 wins!";
                winText.color = p1BaseColor;
            }
        }
        isCapturing = false;

        if (instructionText != null)
        {
            instructionText.text = "Place a piece on an empty slot.";
        }
        SetCaptureHighlights(false);
        SwitchTurn();

        // Return to movement phase if placement finished
        if (piecesPlaced >= 24)
        {
            FindObjectOfType<PhaseState>().SwitchToMovementPhase();
        }
    }

    /// <summary>
    /// Checks if a node is part of a mill
    /// </summary>
    bool IsPartOfMill(Node node)
    {
        foreach (int[] line in millLines)
        {
            if (line.Contains(node.nodeID))
            {
                if (allNodes[line[0]].owner == node.owner &&
                    allNodes[line[1]].owner == node.owner &&
                    allNodes[line[2]].owner == node.owner)
                    return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if player has any pieces NOT in mills
    /// </summary>
    bool HasPiecesOutsideMills(int player)
    {
        return allNodes.Any(n => n.owner == player && !IsPartOfMill(n));
    }

    /// <summary>
    /// Switches turns between players
    /// </summary>
    public void SwitchTurn()
    {
        if (gameOver) return;
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateTurnUI();
        if (AIManager.instance.isAIActive && currentPlayer == AIManager.instance.aiPlayerNumber)
        {
            AIManager.instance.TriggerAITurn();
        }
        if (piecesPlaced >= 24)
        {
            FindObjectOfType<PhaseState>().SwitchToMovementPhase();
        }
    }

    public bool IsCapturing() => isCapturing;

    /// <summary>
    /// Flying rule: player can move anywhere when they have ≤ 3 pieces
    /// Only active in movement phase
    /// </summary>
    public bool IsFlying(int player)
    {
        if (piecesPlaced < 24) return false;

        int pieces = (player == 1) ? p1PiecesLeft : p2PiecesLeft;
        return pieces <= 3;
    }

    /// <summary>
    /// Main decision after move/placement:
    /// - If mill → capture mode
    /// - Else → switch turn
    /// </summary>
    public void CheckMillAndSwitchTurn(Node node)
    {
        List<int> mill = GetMillFormed(node);

        if (mill != null)
            EnterCaptureMode(mill);
        else
            SwitchTurn();
    }

    /// <summary>
    /// Highlights pieces that are legally allowed to be captured.
    /// </summary>
    public void SetCaptureHighlights(bool active)
    {
        int opponent = (currentPlayer == 1) ? 2 : 1;
        bool opponentHasPiecesOutside = HasPiecesOutsideMills(opponent);

        foreach (Node node in allNodes)
        {
            if (node.owner == 0) continue;

            Renderer r = node.GetComponent<Renderer>();
            if (r == null) continue;

            // Use sharedMaterial ONLY if we are in a test/editor environment
            Material mat = Application.isPlaying ? r.material : r.sharedMaterial;

            if (active && node.owner == opponent)
            {
                bool isValidTarget = !IsPartOfMill(node) || !opponentHasPiecesOutside;

                if (isValidTarget)
                {
                    mat.color = validCaptureColor;
                    node.SetGlow(true, validCaptureColor);
                }
            }
            else
            {
                Color teamColor = (node.owner == 1) ? p1BaseColor : p2BaseColor;
                mat.color = teamColor;
                node.SetGlow(false);
            }
        }
    }
}