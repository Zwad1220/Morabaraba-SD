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

    private Material GetMaterial()
    {
        // If the game is actually playing, use .material so each node is unique
        if (Application.isPlaying)
        {
            return rend.material;
        }
        // In the Editor/Tests, use sharedMaterial to avoid the "Leak" error
        return rend.sharedMaterial;
    }

    /// <summary>
    /// Called when a piece is placed or moved onto this node
    /// </summary>
    public void OnClicked(int player, Color playerColor)
    {
        isOccupied = true;
        owner = player;

        if (rend != null)
        {
            GetMaterial().color = playerColor;
        }

        SetGlow(false);
    }

    public void ClearNode()
    {
        isOccupied = false;
        owner = 0;

        if (rend != null)
        {
            GetMaterial().color = Color.white;
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