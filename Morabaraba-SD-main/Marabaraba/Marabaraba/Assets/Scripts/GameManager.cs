using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// GameManager = Core controller ("brain") of the game.
/// Handles:
/// - Turns
/// - Phase switching (Placement → Movement)
/// - Mill detection
/// - Capture logic
/// - Piece counting & win conditions
/// - Flying rule
/// - Anti-repeat mill rule
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance so other scripts can easily access GameManager
    public static GameManager instance;

    [Header("Board Setup")]
    public Node[] allNodes; // All 24 nodes on the board

    [Header("UI References")]
    public TextMeshProUGUI captureText;
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI phaseText;

    [Header("Player Colors")]
    public Color p1BaseColor = Color.red;
    public Color p2BaseColor = Color.blue;

    // Highlight colors used when selecting pieces
    public Color p1GlowColor = Color.yellow;
    public Color p2GlowColor = Color.cyan;

    [Header("Game State")]
    public int currentPlayer = 1;   // Tracks whose turn it is
    private bool isCapturing = false; // True when player must capture a piece
    public int piecesPlaced = 0;   // Used to detect end of placement phase

    // Tracks remaining pieces (used for flying + win condition)
    public int p1PiecesLeft = 12;
    public int p2PiecesLeft = 12;

    // Prevents players from abusing the same mill repeatedly
    private List<int> lastMill = new List<int> { -1, -1, -1 };

    /// <summary>
    /// All valid mill combinations (3-in-a-row)
    /// Each entry refers to node IDs
    /// </summary>
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

        // Ensure nodes are ordered by ID (important for consistency)
        allNodes = allNodes.OrderBy(n => n.nodeID).ToArray();
    }

    void Start()
    {
        captureText.gameObject.SetActive(false);
        UpdateTurnUI();
        UpdatePhaseUI("Placement Phase");
    }

    /// <summary>
    /// Updates turn display and color
    /// </summary>
    public void UpdateTurnUI()
    {
        turnText.text = "Player " + currentPlayer + "'s Turn";
        turnText.color = (currentPlayer == 1) ? p1BaseColor : p2BaseColor;
    }

    /// <summary>
    /// Updates phase display (Placement / Movement)
    /// </summary>
    public void UpdatePhaseUI(string phase)
    {
        phaseText.text = "Current Phase: " + phase;
        phaseText.color = Color.white;
    }

    /// <summary>
    /// Called whenever a piece is placed during placement phase
    /// </summary>
    public void OnPiecePlaced(Node node)
    {
        piecesPlaced++;

        // Check if placement formed a mill
        CheckMillAndSwitchTurn(node);

        // Switch to movement phase after all pieces are placed
        if (piecesPlaced >= 24 && !isCapturing)
        {
            FindObjectOfType<PhaseState>().SwitchToMovementPhase();
        }
    }

    /// <summary>
    /// Returns the mill (list of 3 node IDs) if one is formed, otherwise null
    /// </summary>
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

    /// <summary>
    /// Checks if two mills are identical (used to prevent repetition exploit)
    /// </summary>
    bool IsSameMill(List<int> m1, List<int> m2)
    {
        if (m1 == null || m2 == null) return false;
        return m1.SequenceEqual(m2);
    }

    /// <summary>
    /// Activates capture mode if a valid (new) mill is formed
    /// </summary>
    void EnterCaptureMode(List<int> newMill)
    {
        //  Prevent infinite mill exploit
        if (IsSameMill(newMill, lastMill))
        {
            Debug.Log("Repeat mill - no capture allowed");
            SwitchTurn();
            return;
        }

        // Store this mill so it can't be reused immediately
        lastMill = new List<int>(newMill);

        isCapturing = true;

        captureText.gameObject.SetActive(true);
        captureText.text = "Player " + currentPlayer + ": Capture a piece!";
        captureText.color = (currentPlayer == 1) ? p1BaseColor : p2BaseColor;
    }

    /// <summary>
    /// Handles removing an opponent's piece
    /// </summary>
    public void TryCapture(Node node)
    {
        // Cannot capture own piece or empty node
        if (node.owner == currentPlayer || node.owner == 0) return;

        // Protection rule: cannot capture from a mill unless no alternatives exist
        if (IsPartOfMill(node) && HasPiecesOutsideMills(node.owner)) return;

        //  Store owner BEFORE clearing node (important fix)
        int capturedOwner = node.owner;

        node.ClearNode();

        // Update piece count correctly
        if (capturedOwner == 1) p1PiecesLeft--;
        else if (capturedOwner == 2) p2PiecesLeft--;

        Debug.Log($"P1: {p1PiecesLeft} | P2: {p2PiecesLeft}");

        // Win condition: fewer than 3 pieces = loss
        if (p1PiecesLeft <= 2) Debug.Log("Player 2 Wins!");
        if (p2PiecesLeft <= 2) Debug.Log("Player 1 Wins!");

        isCapturing = false;
        captureText.gameObject.SetActive(false);

        SwitchTurn();

        // Ensure correct phase if capture ends placement
        if (piecesPlaced >= 24)
            FindObjectOfType<PhaseState>().SwitchToMovementPhase();
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
        currentPlayer = (currentPlayer == 1) ? 2 : 1;
        UpdateTurnUI();
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
}