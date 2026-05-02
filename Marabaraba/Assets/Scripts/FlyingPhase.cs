using UnityEngine;

/// <summary>
/// Handles the "Flying" rule in Morabaraba:
/// When a player has 3 or fewer pieces, they can move to ANY node,
/// ignoring neighbour connections.
/// </summary>
public class FlyingPhase : MonoBehaviour
{
    public static FlyingPhase instance;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Checks if the CURRENT player is in flying mode
    /// </summary>
    public bool IsFlyingActive()
    {
        int player = GameManager.instance.currentPlayer;
        return GameManager.instance.IsFlying(player);
    }

    /// <summary>
    /// Determines if a move is valid under flying rules
    /// </summary>
    public bool CanMove(Node fromNode, Node toNode)
    {
        // Cannot move to occupied node
        if (toNode.isOccupied) return false;

        // If flying → can go anywhere
        if (IsFlyingActive()) return true;

        // Otherwise → must be neighbour
        return fromNode.neighbours.Contains(toNode);
    }
}