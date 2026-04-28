using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single position (intersection) on the board.
/// Handles:
/// - Ownership (which player owns it)
/// - Occupation state
/// - Visual appearance (color + glow)
/// - Connections to neighbouring nodes
/// </summary>
public class Node : MonoBehaviour
{
    [Header("Node Data")]
    public int nodeID; // Unique ID (0–23)
    public List<Node> neighbours = new List<Node>(); // Adjacent nodes (used for movement)

    [Header("State")]
    public bool isOccupied = false; // Whether a piece is on this node
    public int owner = 0; // 0 = empty, 1 = Player 1, 2 = Player 2

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    /// <summary>
    /// Called when a piece is placed or moved onto this node
    /// Sets ownership + visual color
    /// </summary>
    public void OnClicked(int player, Color playerColor)
    {
        isOccupied = true;
        owner = player;

        // Set base color (team color)
        rend.material.color = playerColor;

        // Ensure glow is off initially
        SetGlow(false);
    }

    /// <summary>
    /// Clears the node (used when piece is captured or moved away)
    /// </summary>
    public void ClearNode()
    {
        isOccupied = false;
        owner = 0;

        // Reset to default color
        rend.material.color = Color.white;

        SetGlow(false);
    }

    /// <summary>
    /// Handles highlight glow using emission
    /// Used when selecting pieces
    /// </summary>
    public void SetGlow(bool shouldHighlight, Color highlightColor = default)
    {
        if (shouldHighlight)
        {
            // Enable emission (makes material "glow")
            rend.material.EnableKeyword("_EMISSION");

            // Bright highlight color
            rend.material.SetColor("_EmissionColor", highlightColor * 3.0f);
        }
        else
        {
            // Disable glow
            rend.material.DisableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", Color.black);
        }
    }
}