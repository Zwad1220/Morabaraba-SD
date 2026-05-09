using UnityEngine;

public class FlyingPhase : MonoBehaviour
{
    public static FlyingPhase instance;// Singleton instance for easy access across scripts
    public GameManager gm;// Reference to the GameManager for accessing game state
    void Awake()
    {
        instance = this;// Set the singleton instance to this script
    }

    
    // Checks if the CURRENT player is in flying mode
    public bool IsFlyingActive()
    {
        int player = GameManager.instance.currentPlayer;
        return GameManager.instance.IsFlying(player);// Delegates to GameManager's IsFlying method which checks pieces left
    }

  
    // Determines if a move is valid under flying rules
    public bool CanMove(Node fromNode, Node toNode)
    {
        // Cannot move to occupied node
        if (toNode.isOccupied) return false;

        // If flying- player can go anywhere
        if (IsFlyingActive())
        {
            return true;
        }

        // Otherwise- player must move to a neighbouring node
        return fromNode.neighbours.Contains(toNode);
    }
}