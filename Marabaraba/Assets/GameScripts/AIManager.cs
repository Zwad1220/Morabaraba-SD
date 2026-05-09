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

    /// <summary>
    /// Called by GameManager when it's the AI's turn.
    /// </summary>
    public void TriggerAITurn()
    {
        if (!isAIActive || GameManager.instance.currentPlayer != aiPlayerNumber || GameManager.instance.gameOver)
            return;

        StopAllCoroutines();
        StartCoroutine(AILogicCoroutine());
    }

    IEnumerator AILogicCoroutine()
    {
        // 1. Initial delay for natural feel
        yield return new WaitForSeconds(0.8f);
        if (GameManager.instance.gameOver) yield break;

        // 2. Determine Action: Placement or Movement
        // Note: We check Capture FIRST in case the game loaded into a capture state
        if (GameManager.instance.IsCapturing())
        {
            ExecuteCapture();
        }
        else if (GameManager.instance.piecesPlaced < 24) 
        {
            ExecutePlacement();
        }
        else
        {
            yield return StartCoroutine(ExecuteMovementCoroutine());
        }

        // 3. WAIT for the game state to update (important for mill detection)
        yield return new WaitForSeconds(0.2f);

        // 4. CAPTURE CHECK: If the action above created a mill, perform capture now
        if (GameManager.instance.IsCapturing() && !GameManager.instance.gameOver)
        {
            yield return new WaitForSeconds(0.6f); // Wait to show the mill was formed
            ExecuteCapture();
        }
    }

    // --- AI STRATEGIES ---

    void ExecutePlacement()
    {
        List<Node> emptyNodes = GameManager.instance.allNodes.Where(n => !n.isOccupied).ToList();
        if (emptyNodes.Count == 0) return;

        Node target = null;

        if (currentDifficulty == Difficulty.Easy)
        {
            target = emptyNodes[Random.Range(0, emptyNodes.Count)];
        }
        else // Medium & Hard: Try to complete a mill or block player
        {
            target = FindBestPlacement(emptyNodes);
        }

        if (target != null)
        {
            UndoRedoManager.instance.SaveState();
            Color aiColor = (aiPlayerNumber == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;

            // Execute logic
            target.OnClicked(aiPlayerNumber, aiColor);
            GameManager.instance.OnPiecePlaced(target);
        }
    }

    IEnumerator ExecuteMovementCoroutine()
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
        // --- VISUAL HIGHLIGHT LOGIC ---

        // 1. Manually set the Material Color to Green (to match human movement)
        Renderer r = choice.from.GetComponent<Renderer>();

        if (r != null)
        {
            r.material.color = Color.green;
        }

        // 2. Turn on the Glow using the AI's specific glow color from GameManager
        Color aiGlow = (aiPlayerNumber == 1) ? GameManager.instance.p1GlowColor : GameManager.instance.p2GlowColor;
        choice.from.SetGlow(true, aiGlow);

        // 3. Wait for 1 second so the player sees the "Selection"
        yield return new WaitForSeconds(1.0f);

        // --- EXECUTE MOVE ---
        UndoRedoManager.instance.SaveState();

        // Clear the old node (this also resets its color/glow internally)
        choice.from.ClearNode();

        Color aiBaseColor = (aiPlayerNumber == 1) ? GameManager.instance.p1BaseColor : GameManager.instance.p2BaseColor;
        choice.to.OnClicked(aiPlayerNumber, aiBaseColor);

        GameManager.instance.CheckMillAndSwitchTurn(choice.to);
    }

    void ExecuteCapture()
    {
        int opponent = (aiPlayerNumber == 1) ? 2 : 1;

        // Rules: Must take pieces NOT in a mill, unless ALL pieces are in mills
        var allOpponentPieces = GameManager.instance.allNodes.Where(n => n.owner == opponent).ToList();
        var validTargets = allOpponentPieces.Where(n => !IsPartOfMill(n)).ToList();

        // If all are in mills, any piece is a valid target
        if (validTargets.Count == 0)
            validTargets = allOpponentPieces;

        if (validTargets.Count > 0)
        {
            Node target = validTargets[Random.Range(0, validTargets.Count)];
            GameManager.instance.TryCapture(target);
        }
    }

    // --- HELPERS ---

    Node FindBestPlacement(List<Node> options)
    {
        // 1. Can AI finish a mill?
        foreach (var node in options)
            if (WouldFormMill(node, aiPlayerNumber)) return node;

        // 2. Can AI block a Player mill?
        int opponent = (aiPlayerNumber == 1) ? 2 : 1;
        foreach (var node in options)
            if (WouldFormMill(node, opponent)) return node;

        // 3. Prefer high-connectivity nodes (center squares)
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
                    // During movement, the "from" node must be ignored as it's about to be empty
                    if (n == ignoreNode) continue;
                    if (n.owner == player) count++;
                }
                if (count == 2) return true;
            }
        }
        return false;
    }

    struct Move { public Node from; public Node to; }

    List<Move> GetAvailableMoves()
    {
        List<Move> moves = new List<Move>();
        var myNodes = GameManager.instance.allNodes.Where(n => n.owner == aiPlayerNumber);

        foreach (var fromNode in myNodes)
        {
            bool aiFlying = GameManager.instance.IsFlying(aiPlayerNumber);

            foreach (var toNode in GameManager.instance.allNodes.Where(n => !n.isOccupied))
            {
                if (aiFlying || fromNode.neighbours.Contains(toNode))
                {
                    moves.Add(new Move { from = fromNode, to = toNode });
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
            if (line.Contains(node.nodeID))
            {
                if (GameManager.instance.allNodes[line[0]].owner == node.owner &&
                    GameManager.instance.allNodes[line[1]].owner == node.owner &&
                    GameManager.instance.allNodes[line[2]].owner == node.owner)
                {
                    return true;
                }
            }
        }
        return false;
    }
}