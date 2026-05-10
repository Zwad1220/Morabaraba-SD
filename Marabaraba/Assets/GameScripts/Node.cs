using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    [Header("Node Data")]
    public int nodeID;
    public List<Node> neighbours = new List<Node>();

    [Header("State")]
    public bool isOccupied = false;
    public int owner = 0;

    private Renderer rend;
    private Color originalColor;
    void Awake()
    {
   
        if (rend == null)
        {
            rend = GetComponent<SpriteRenderer>();//finds sprite renderer on the node, if it exists. This is used for color changes and glow effects.
        }
    }

    private Material GetMaterial()
    {
        // checks if the game is playing, use .material so each node is unique
        if (Application.isPlaying)
        {
            return rend.material;
        }
        //  used sharedMaterial for tests to avoid the "Leak" error
        return rend.sharedMaterial;
    }

   
    // Called when a piece is placed or moved onto a node
    public void OnClicked(int player, Color playerColor)
    {
        isOccupied = true;//sets the node as occupied and assigns ownership to the current player
        owner = player;

        if (rend != null)
        {
            GetMaterial().color = playerColor;//sets the node's color to the current player's color
        }

        SetGlow(false);//turns off any glow effects when a piece is placed
    }

    public void ClearNode()//resets the node to an empty state, used for captures and undoing moves
    {
        isOccupied = false;//marks the node as unoccupied and resets ownership to 0 (no player)
        owner = 0;

        if (rend != null)
        {
            GetMaterial().color = Color.white;
        }

        SetGlow(false);
    }

   
    public void SetGlow(bool shouldHighlight, Color highlightColor = default)//turns on or off the glow effect by enabling/disabling the emission keyword and setting the emission color
    {
        if (rend == null) return;

        if (shouldHighlight)
        {
            rend.material.EnableKeyword("_EMISSION");//enables the emission keyword to turn on the glow effect
            rend.material.SetColor("_EmissionColor", highlightColor * 3.0f);//sets the emission color to the specified highlight color
        }
        else
        {
            rend.material.DisableKeyword("_EMISSION");//disables the emission keyword to turn off the glow effect
            rend.material.SetColor("_EmissionColor", Color.black);//resets the emission color to black
        }
    }
}