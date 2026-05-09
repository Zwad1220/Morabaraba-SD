using UnityEngine;

public class BoardSetup : MonoBehaviour
{
    public Node[] nodes; // The 24 nodes on the board

    void Start()
    {
        // We use the millLines to determine which nodes are physically connected (uses index number of nodes)
        foreach (int[] line in GameManager.millLines)
        {
            // In a 3 node mill [A, B, C], A connects to B, and B connects to C
            Connect(line[0], line[1]);
            Connect(line[1], line[2]);
        }

        Debug.Log("Board Graph setup complete. All neighbours mapped.");// CHECK THEY WERE MAPPED CORRECTLY
    }

    // Creates a two way connection between two nodes
    void Connect(int aIdx, int bIdx)
    {
        Node a = nodes[aIdx];
        Node b = nodes[bIdx];

        // Checks if connection already exists and only adds if the connection doesn't already exist to avoid duplicates
        if (!a.neighbours.Contains(b)) a.neighbours.Add(b);
        if (!b.neighbours.Contains(a)) b.neighbours.Add(a);
    }
}