using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class HintManager : MonoBehaviour
{
    public static HintManager instance;

    [Header("Hint Colors")]
    public Color suggestedPieceColor = Color.green;
    public Color suggestedMoveColor = Color.cyan;

    private Node highlightedFrom;
    private Node highlightedTo;

    void Awake()
    {
        instance = this;
    }

    // Called by Hint Button
    public void ShowHint()
    {
        if (GameManager.instance.gameOver)
            return;

        ClearHints();

        // Placement Phase
        if (GameManager.instance.piecesPlaced < 24)
        {
            ShowPlacementHint();
        }
        else
        {
            ShowMovementHint();
        }

        Debug.Log("Hint button pressed");
    }

    //Placement hints
    void ShowPlacementHint()
    {
        int player = GameManager.instance.currentPlayer;
        int opponent = (player == 1) ? 2 : 1;

        List<Node> emptyNodes =
            GameManager.instance.allNodes
            .Where(n => !n.isOccupied)
            .ToList();

        // 1. Try make a mill
        foreach (Node node in emptyNodes)
        {
            if (WouldFormMill(node, player))
            {
                HighlightPlacement(node);
                return;
            }
        }

        // 2. Try block opponent mill
        foreach (Node node in emptyNodes)
        {
            if (WouldFormMill(node, opponent))
            {
                HighlightPlacement(node);
                return;
            }
        }

        // 3. Otherwise choose strongest node
        Node best =
            emptyNodes
            .OrderByDescending(n => n.neighbours.Count)
            .FirstOrDefault();

        if (best != null)
        {
            HighlightPlacement(best);
        }
    }

    void HighlightPlacement(Node node)
    {
        highlightedTo = node;
        node.SetGlow(true, suggestedMoveColor);
    }

    //Movement hints
    void ShowMovementHint()
    {
        int player = GameManager.instance.currentPlayer;

        List<Move> moves = GetAvailableMoves(player);

        // 1. Look for mill-forming move
        foreach (Move move in moves)
        {
            if (WouldFormMill(move.to, player, move.from))
            {
                HighlightMove(move);
                return;
            }
        }

        // 2. Otherwise random valid move
        if (moves.Count > 0)
        {
            HighlightMove(moves[0]);
        }
    }

    void HighlightMove(Move move)
    {
        highlightedFrom = move.from;
        highlightedTo = move.to;

        move.from.SetGlow(true, suggestedPieceColor);
        move.to.SetGlow(true, suggestedMoveColor);
    }

    //Clear previous hints
    public void ClearHints()
    {
        if (highlightedFrom != null)
            highlightedFrom.SetGlow(false);

        if (highlightedTo != null)
            highlightedTo.SetGlow(false);

        highlightedFrom = null;
        highlightedTo = null;
    }

    //Helpers
    bool WouldFormMill(Node node, int player, Node ignoreNode = null)
    {
        foreach (int[] line in GameManager.millLines)
        {
            if (line.Contains(node.nodeID))
            {
                int count = 0;

                foreach (int id in line)
                {
                    if (id == node.nodeID)
                        continue;

                    Node n = GameManager.instance.allNodes[id];

                    if (n == ignoreNode)
                        continue;

                    if (n.owner == player)
                        count++;
                }

                if (count == 2)
                    return true;
            }
        }

        return false;
    }

    struct Move
    {
        public Node from;
        public Node to;
    }

    List<Move> GetAvailableMoves(int player)
    {
        List<Move> moves = new List<Move>();

        bool flying = GameManager.instance.IsFlying(player);

        var myNodes =
            GameManager.instance.allNodes
            .Where(n => n.owner == player);

        foreach (Node fromNode in myNodes)
        {
            foreach (Node toNode in GameManager.instance.allNodes.Where(n => !n.isOccupied))
            {
                if (flying || fromNode.neighbours.Contains(toNode))
                {
                    moves.Add(new Move
                    {
                        from = fromNode,
                        to = toNode
                    });
                }
            }
        }

        return moves;
    }
}
