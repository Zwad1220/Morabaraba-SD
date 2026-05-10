using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class FlyingPhaseTests
{
    private GameManager gm;
    private FlyingPhase flying;
    private GameObject gameObj;

    [SetUp]
    public void Setup()
    {
        // Arrange: Create the test environment
        gameObj = new GameObject();
        gm = gameObj.AddComponent<GameManager>();
        flying = gameObj.AddComponent<FlyingPhase>();

        // CRITICAL: Manually set the static instances
        GameManager.instance = gm;
        FlyingPhase.instance = flying;

        // Setup mock nodes
        gm.allNodes = new Node[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject nObj = new GameObject();
            gm.allNodes[i] = nObj.AddComponent<Node>();
            gm.allNodes[i].nodeID = i;
        }

        gm.currentPlayer = 1;
    }
    [TearDown]
    public void Teardown() => Object.DestroyImmediate(gameObj);

    // NORMAL CASE: Moving to a neighbor when not flying
    [Test]
    public void CanMove_ToNeighbor_ReturnsTrue()
    {
        gm.piecesPlaced = 24; // Movement phase
        gm.p1PiecesLeft = 5;  // Not flying
        Node start = gm.allNodes[0];
        Node target = gm.allNodes[1];
        start.neighbours.Add(target);

        bool result = flying.CanMove(start, target);
        Assert.IsTrue(result);
    }

    // BOUNDARY CASE: Exactly 3 pieces allows flying (anywhere)
    [Test]
    public void IsFlyingActive_WithExactlyThreePieces_ReturnsTrue()
    {
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 3;

        Assert.IsTrue(gm.IsFlying(1), "Player with 3 pieces should be allowed to fly.");
    }

    // BOUNDARY CASE: Exactly 4 pieces prevents flying
    [Test]
    public void IsFlyingActive_WithFourPieces_ReturnsFalse()
    {
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 4;

        Assert.IsFalse(gm.IsFlying(1), "Player with 4 pieces should NOT be allowed to fly.");
    }

    // INVALID CASE: Moving to a node already occupied
    [Test]
    public void CanMove_ToOccupiedNode_ReturnsFalse()
    {
        Node start = gm.allNodes[0];
        Node target = gm.allNodes[1];
        target.isOccupied = true;

        Assert.IsFalse(flying.CanMove(start, target), "Cannot move to a node that is already occupied.");
    }

    [Test]
    public void CanMove_WhenFlyingIsActive_AllowsMovementToNonNeighbors()
    {
        // Arrange
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 3;

        Node start = gm.allNodes[0];
        Node target = gm.allNodes[10]; // Distant node

        // Act
        bool result = FlyingPhase.instance.CanMove(start, target);

        // Assert
        Assert.IsTrue(result, "Flying should allow movement to any empty node regardless of proximity.");
    }
}