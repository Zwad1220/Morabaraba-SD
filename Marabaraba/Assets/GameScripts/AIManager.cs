using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AIManager : MonoBehaviour
{
    public static AIManager instance;

    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty;
    public bool isAIActive = false;
    public int aiPlayerNumber = 2;

    void Awake() => instance = this;

    
    // Called by GameManager when it's the AI's turn.
    public void TriggerAITurn()
    {
        if (!isAIActive || GameManager.instance.currentPlayer != aiPlayerNumber || GameManager.instance.gameOver)// Ensure it's the AI's turn and the game isn't over
            return;

        StopAllCoroutines();
        StartCoroutine(AILogicCoroutine());// Start the AI logic as a Coroutine to allow for timed delays
    }

    IEnumerator AILogicCoroutine()
    {
        // Initial delay for natural feel
        yield return new WaitForSeconds(0.8f);
        if (GameManager.instance.gameOver) yield break;

        //  Determine Action- Placement or Movement
        //capture is checked first because if the player just formed a mill, the AI must capture before doing anything else
        if (GameManager.instance.IsCapturing())
        {
            ExecuteCapture();// If in capture mode, perform capture immediately (before placement/movement) to properly respond to player mills
        }
        else if (GameManager.instance.piecesPlaced < 24) 
        {
            ExecutePlacement();
        }
        else
        {
            yield return StartCoroutine(ExecuteMovementCoroutine());
        }

        //  Wait for the game state to update (important for mill detection)
        yield return new WaitForSeconds(0.2f);

        //  check if the action above created a mill, perform capture now
        if (GameManager.instance.IsCapturing() && !GameManager.instance.gameOver)
        {
            yield return new WaitForSeconds(0.6f); // Wait to show the mill was formed
            ExecuteCapture();
        }
    }

    //AI Strategies

    void ExecutePlacement()
    {
        List<Node> emptyNodes = GameManager.instance.allNodes.Where(n => !n.isOccupied).ToList();
        if (emptyNodes.Count == 0) return;

        Node target = null;

        if (currentDifficulty == Difficulty.Easy)
        {
            target = emptyNodes[Random.Range(0, emptyNodes.Count)];
        }
        else // Medium & Hard-Try to complete a mill or block player
        {
            target = FindBestPlacement(emptyNodes);// This method encapsulates the strategic logic for placement based on difficulty
        }

        if (target != null)
        {
            //UndoRedoManager.instance.SaveState();
            Color aiColor = (aiPlayerNumber == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;// Determine the AI's color based on its player number

            // Execute logic
            target.OnClicked(aiPlayerNumber, aiColor);
            GameManager.instance.OnPiecePlaced(target);
            GameManager.instance.movesWithoutCapture++;
        }
    }

    IEnumerator ExecuteMovementCoroutine()// Movement is more complex due to the need to select both a piece to move and a destination,  a Coroutine used to allow for visual highlights and delays
    {
        var validMoves = GetAvailableMoves();
        if (validMoves.Count == 0) yield break;

        Move choice;
        if (currentDifficulty == Difficulty.Easy)
        {
            choice = validMoves[Random.Range(0, validMoves.Count)];
        }
        else
        {
            var millMoves = validMoves.Where(m => WouldFormMill(m.to, aiPlayerNumber, m.from)).ToList();
            choice = (millMoves.Count > 0) ? millMoves[Random.Range(0, millMoves.Count)] : validMoves[Random.Range(0, validMoves.Count)];
        }
        //Visual highlight logic

        //Manually set the Material Color to Green 
        Renderer r = choice.from.GetComponent<Renderer>();

        if (r != null)
        {
            r.material.color = Color.green;
        }

        // Turn on the Glow using the AI's specific glow color from GameManager
        Color aiGlow = (aiPlayerNumber == 1) ? GameManager.instance.p1GlowColor : GameManager.instance.p2GlowColor;
        choice.from.SetGlow(true, aiGlow);

        // Wait for 1 second so the player sees the "Selection"
        yield return new WaitForSeconds(1.0f);

        //EXECUTE MOVE 
        //UndoRedoManager.instance.SaveState();

        // Clear the old node
        choice.from.ClearNode();

        Color aiBaseColor = (aiPlayerNumber == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;
        choice.to.OnClicked(aiPlayerNumber, aiBaseColor);

        GameManager.instance.CheckMillAndSwitchTurn(choice.to);
        GameManager.instance.movesWithoutCapture++;
    }

    void ExecuteCapture()
    {
        int opponent = (aiPlayerNumber == 1) ? 2 : 1;

        //Must take pieces NOT in a mill, unless ALL pieces are in mills
        var allOpponentPieces = GameManager.instance.allNodes.Where(n => n.owner == opponent).ToList();
        var validTargets = allOpponentPieces.Where(n => !IsPartOfMill(n)).ToList();

        // If all are in mills, any piece is a valid target
        if (validTargets.Count == 0)
            validTargets = allOpponentPieces;

        if (validTargets.Count > 0)
        {
            Node target = validTargets[Random.Range(0, validTargets.Count)];// Randomly select a target from the valid options to capture
            GameManager.instance.TryCapture(target);
        }
    }

    //Helpers

    Node FindBestPlacement(List<Node> options)
    {
        //check if any move can form a mill for the AI
        foreach (var node in options)
            if (WouldFormMill(node, aiPlayerNumber)) return node;

        //check if any move can block the opponent from forming a mill on their next turn
        int opponent = (aiPlayerNumber == 1) ? 2 : 1;
        foreach (var node in options)
            if (WouldFormMill(node, opponent)) return node;

        //rather than random, prefer placements that are part of more potential mills (more neighbors) to increase future mill opportunities
        return options.OrderByDescending(n => n.neighbours.Count).First();
    }

    // Overload for placement/flying
    bool WouldFormMill(Node node, int player, Node ignoreNode = null)
    {
        foreach (int[] line in GameManager.millLines)
        {
            if (line.Contains(node.nodeID))
            {
                int count = 0;
                foreach (int id in line)
                {
                    if (id == node.nodeID) continue;
                    Node n = GameManager.instance.allNodes[id];
                    // If we're checking a potential move, we want to ignore the piece being moved from its original position since it won't be there when we check for mills
                    if (n == ignoreNode) continue;
                    if (n.owner == player) count++;
                }
                if (count == 2) return true;
            }
        }
        return false;
    }

    struct Move { public Node from; public Node to; }// Simple struct to represent a potential move for the movement phase, containing both the starting node and the destination node

    List<Move> GetAvailableMoves()
    {
        List<Move> moves = new List<Move>();
        var myNodes = GameManager.instance.allNodes.Where(n => n.owner == aiPlayerNumber);// Get all nodes currently occupied by the AI's pieces

        foreach (var fromNode in myNodes)
        {
            bool aiFlying = GameManager.instance.IsFlying(aiPlayerNumber);

            foreach (var toNode in GameManager.instance.allNodes.Where(n => !n.isOccupied))// For each empty node, check if it's a valid move destination. If the AI is flying, it can move to any empty node. Otherwise, it can only move to neighboring nodes.
            {
                if (aiFlying || fromNode.neighbours.Contains(toNode))
                {
                    moves.Add(new Move { from = fromNode, to = toNode });// If the move is valid, add it to the list of potential moves for the movement phase
                }
            }
        }
        return moves;
    }

    bool IsPartOfMill(Node node)
    {
        if (node.owner == 0) return false;

        foreach (int[] line in GameManager.millLines)
        {
            if (line.Contains(node.nodeID))// Check if the node is part of this mill line
            {
                if (GameManager.instance.allNodes[line[0]].owner == node.owner &&// If all nodes in the line are owned by the same player, then this node is part of a mill
                    GameManager.instance.allNodes[line[1]].owner == node.owner &&
                    GameManager.instance.allNodes[line[2]].owner == node.owner)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // Strategic helper to find nodes that would complete a mill
    public Node GetMoveHint(int playerID)
    {
        List<Node> emptyNodes = GameManager.instance.allNodes.Where(n => !n.isOccupied).ToList();
        foreach (var node in emptyNodes)
        {
            if (WouldFormMill(node, playerID))
                return node;
        }
        return null;
    }
}