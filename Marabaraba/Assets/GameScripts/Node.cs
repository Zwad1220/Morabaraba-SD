using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a single position (intersection) on the board.
/// Handles:
/// - Ownership
/// - Occupation state
/// - Visual appearance
/// - Connections to neighbouring nodes
/// </summary>
public class Node : MonoBehaviour
{
    [Header("Node Data")]
    public int nodeID;
    public List<Node> neighbours = new List<Node>();

    [Header("State")]
    public bool isOccupied = false;
    public int owner = 0;

    private Renderer rend;

    void Awake()
    {
        rend = GetComponent<Renderer>();

        // Fallback for SpriteRenderer
        if (rend == null)
        {
            rend = GetComponent<SpriteRenderer>();
        }
    }

    /// <summary>
    /// Called when a piece is placed or moved onto this node
    /// </summary>
    public void OnClicked(int player, Color playerColor)
    {
        isOccupied = true;
        owner = player;

        // Only do visuals if renderer exists
        if (rend != null)
        {
            rend.material.color = playerColor;
        }

        SetGlow(false);
    }

    /// <summary>
    /// Clears the node
    /// </summary>
    public void ClearNode()
    {
        isOccupied = false;
        owner = 0;

        if (rend != null)
        {
            rend.material.color = Color.white;
        }

        SetGlow(false);
    }

    /// <summary>
    /// Glow highlight
    /// </summary>
    public void SetGlow(bool shouldHighlight, Color highlightColor = default)
    {
        if (rend == null) return;

        if (shouldHighlight)
        {
            rend.material.EnableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", highlightColor * 3.0f);
        }
        else
        {
            rend.material.DisableKeyword("_EMISSION");
            rend.material.SetColor("_EmissionColor", Color.black);
        }
    }
}