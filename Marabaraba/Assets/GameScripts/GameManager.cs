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

    [Header("Draw UI")]
    public GameObject drawPanel;

    [Header("Placement Counters")]// Tracks how many pieces each player has left to place on the board  
    public int p1PiecesToPlace = 12;
    public int p2PiecesToPlace = 12;
    // Tracks remaining pieces (used for flying + win condition)
    public int p1PiecesLeft = 0;
    public int p2PiecesLeft = 0;

    [Header("Draw System")]
    public int movesWithoutCapture = 0;
    public int maxMovesWithoutCapture = 10;

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


    public void OnPiecePlaced(Node node)
    {
        // Assign the owner and update state
        node.owner = currentPlayer;
        node.isOccupied = true;

        if (currentPlayer == 1)
        {
            p1PiecesToPlace--;
            p1PiecesLeft++;
        }
        else
        {
            p2PiecesToPlace--;
            p2PiecesLeft++;
        }

        UpdatePieceUI();
        piecesPlaced++;

        // Check for mills BEFORE switching turns or phases
        CheckMillAndSwitchTurn(node);

        // Switch to movement phase after all pieces are placed 
        // and we aren't busy capturing a piece
        if (piecesPlaced >= 24 && !isCapturing)
        {
            var ps = FindObjectOfType<PhaseState>();
            if (ps != null) ps.SwitchToMovementPhase();
        }
    }

    List<int> GetMillFormed(Node node)
    {
        // Check if the node is empty; an empty node cannot form a mill
        if (node.owner == 0) return null;

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
      
        SetCaptureHighlights(true);
    }

   
    // Handles removing an opponent's piece
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
        movesWithoutCapture = 0;

        // Update piece counts
        if (capturedOwner == 1)
            p1PiecesLeft--;
        else if (capturedOwner == 2)
            p2PiecesLeft--;
        UpdatePieceUI();

        // Win conditions
        if (piecesPlaced >= 24)
        {
            if (p1PiecesLeft == 3)// Trigger flying phase when a player is down to 3 pieces
            {
                p1FlyingPhase = true;
            }
            if (p2PiecesLeft == 3)// Trigger flying phase when a player is down to 3 pieces
            {
                p2FlyingPhase = true;
            }
            if (p1PiecesLeft <= 2)
            {
                gameOver = true;
                winScreen.SetActive(true);
                winText.text = "Player 2 wins!";
                winText.color = p2BaseColor;// Set win text color to the winning player's base color
            }

            if (p2PiecesLeft <= 2)
            {
                gameOver = true;
                winScreen.SetActive(true);
                winText.text = "Player 1 wins!";
                winText.color = p1BaseColor;// Set win text color to the winning player's base color
            }
        }
        isCapturing = false;// Exit capture mode after a successful capture

        if (instructionText != null)
        {
            instructionText.text = "Place a piece on an empty slot.";
        }
        SetCaptureHighlights(false);
        SwitchTurn();

        // Return to movement phase if placement finished
        if (piecesPlaced >= 24)
        {
            FindObjectOfType<PhaseState>().SwitchToMovementPhase();// Ensure we switch to movement phase after capturing if we were still in placement phase
        }
    }

    
    // Checks if a node is part of a mill
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


    // Checks if player has any pieces NOT in mills
    bool HasPiecesOutsideMills(int player)
    {
        return allNodes.Any(n => n.owner == player && !IsPartOfMill(n));
    }


    // Switches turns between players
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
        if (piecesPlaced >= 24)
        {
            if (!PlayerHasMoves(currentPlayer))
            {
                int winner = (currentPlayer == 1) ? 2 : 1;

                gameOver = true;

                // Show win screen
                if (winScreen != null)
                    winScreen.SetActive(true);

                // Update winner text
                if (winText != null)
                    winText.text =
                        "Player " + winner + " Wins! Opponent cannot move.";

                // Optional instruction text
                if (instructionText != null)
                    instructionText.text =
                        "Player " + winner + " Wins! Opponent cannot move.";

                Debug.Log("Player " + winner + " Wins!");

                return;
            }
        }
    }
    public bool IsCapturing() => isCapturing;


    //allows players to move anywhere when they have ≤ 3 pieces
   
    public bool IsFlying(int player)
    {
        if (piecesPlaced < 24) return false;

        int pieces = (player == 1) ? p1PiecesLeft : p2PiecesLeft;// Get the number of pieces left for the specified player
        return pieces <= 3;
    }

    public void CheckMillAndSwitchTurn(Node node)// Checks if the recently placed piece formed a mill, and either enters capture mode or switches turn
    {
        List<int> mill = GetMillFormed(node);

        if (mill != null)
            EnterCaptureMode(mill);// If a mill was formed, enter capture mode instead of switching turn
        else
            if (mill != null)
        {
            EnterCaptureMode(mill);
        }
        else
        {
            //Only count during movement phase
            // ONLY apply draw rule in movement phase
            if (piecesPlaced >= 24)
            {
                // Draw rule ONLY when either player has 3 cows
                if (p1PiecesLeft == 3 || p2PiecesLeft == 3)
                {
                    movesWithoutCapture++;

                    Debug.Log("Moves without capture: " + movesWithoutCapture);

                    if (movesWithoutCapture >= maxMovesWithoutCapture)
                    {
                        EndDraw();
                        return;
                    }
                }
            }

            SwitchTurn();
        }
    }

    bool PlayerHasMoves(int player)
    {
        // Flying players can always move
        if (IsFlying(player))
            return true;

        var playerNodes = allNodes.Where(n => n.owner == player);

        foreach (Node node in playerNodes)
        {
            foreach (Node neighbour in node.neighbours)
            {
                if (!neighbour.isOccupied)
                {
                    return true;
                }
            }
        }

        return false;
    }


    // Highlights pieces that are legally allowed to be captured.
    public void SetCaptureHighlights(bool active)
    {
        int opponent = (currentPlayer == 1) ? 2 : 1;
        bool opponentHasPiecesOutside = HasPiecesOutsideMills(opponent);

        foreach (Node node in allNodes)
        {
            if (node.owner == 0) continue;// Skip empty nodes

            Renderer r = node.GetComponent<Renderer>();// Get the Renderer to change material color
            if (r == null) continue;

            // Use sharedMaterial ONLY if we are in a test/editor environment
            Material mat = Application.isPlaying ? r.material : r.sharedMaterial;

            if (active && node.owner == opponent)
            {
                bool isValidTarget = !IsPartOfMill(node) || !opponentHasPiecesOutside;// Valid capture if not in a mill, or if all opponent pieces are in mills

                if (isValidTarget)
                {
                    mat.color = validCaptureColor;
                    node.SetGlow(true, validCaptureColor);// Highlight valid capture targets with glow and color change
                }
            }
            else
            {
                Color teamColor = (node.owner == 1) ? p1BaseColor : p2BaseColor;// Revert to original team color and turn off glow when not in capture mode
                mat.color = teamColor;// Revert to original team color
                node.SetGlow(false);// Turn off glow
            }
        }
    }

    public void ResetGame()
    {
        // 1. Clear every node on the board
        foreach (Node node in allNodes)
        {
            node.ClearNode();
        }

        // 2. Reset all piece counters to project specifications
        p1PiecesToPlace = 12;
        p2PiecesToPlace = 12;
        p1PiecesLeft = 0;
        p2PiecesLeft = 0;
        piecesPlaced = 0;

        movesWithoutCapture = 0;

        // 3. Reset game state flags
        currentPlayer = 1;
        gameOver = false;
        p1FlyingPhase = false;
        p2FlyingPhase = false;
        isCapturing = false;

        // 4. Update the UI to reflect a fresh start
        if (winScreen != null)
            winScreen.SetActive(false);

        if (drawPanel != null)
            drawPanel.SetActive(false);
        if (instructionText != null) instructionText.text = "Place a piece on an empty slot.";

        UpdateTurnUI();
        UpdatePieceUI();
        UpdatePhaseUI("Placement Phase");

        // 5. Ensure the input scripts return to Placement Phase
        PhaseState ps = FindObjectOfType<PhaseState>();
        if (ps != null)
        {
            ps.placementPhase.enabled = true;
            ps.movementPhase.enabled = false;
        }
    }

    public void EndDraw()
    {
        gameOver = true;

        drawPanel.SetActive(true);

        Debug.Log("Game Ended In Draw");

        // Disable gameplay
        FindObjectOfType<Placement>().enabled = false;
        FindObjectOfType<Movement>().enabled = false;

        // Stop AI
        if (AIManager.instance != null)
        {
            AIManager.instance.StopAllCoroutines();
        }
    }
    public bool CheckForMill(Node node, int playerID)
    {
        // Reuses your existing GetMillFormed logic to satisfy the unit tests
        var mill = GetMillFormed(node);
        return mill != null && node.owner == playerID;
    }
}