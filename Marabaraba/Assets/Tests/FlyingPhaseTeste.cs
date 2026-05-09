using NUnit.Framework;
using UnityEngine;

public class FlyingPhaseTests
{
    GameManager gm;
    FlyingPhase fp;
    Node nodeA;
    Node nodeB;

    [SetUp]
    public void Setup()
    {
        GameObject gmObj = new GameObject();
        gm = gmObj.AddComponent<GameManager>();

        GameManager.instance = gm;

        GameObject fpObj = new GameObject();
        fp = fpObj.AddComponent<FlyingPhase>();

        GameObject a = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        nodeA = a.AddComponent<Node>();
        nodeB = b.AddComponent<Node>();
    }

    [Test]
    public void test_player_can_fly_with_three_pieces()
    {
        // Arrange
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 3;

        // Act
        bool result = fp.IsFlyingActive();

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void test_player_cannot_fly_with_four_pieces()
    {
        // Arrange
        gm.piecesPlaced = 24;
        gm.currentPlayer = 1;
        gm.p1PiecesLeft = 4;

        // Act
        bool result = fp.IsFlyingActive();

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void test_neighbour_node_allows_movement()
    {
        // Arrange
        nodeA.neighbours.Add(nodeB);

        // Act
        bool result = fp.CanMove(nodeA, nodeB);

        // Assert
        Assert.IsTrue(result);
    }

    // INVALID CASE

    [Test]
    public void test_cannot_move_to_occupied_node()
    {
        // Arrange
        nodeB.isOccupied = true;

        // Act
        bool result = fp.CanMove(nodeA, nodeB);

        // Assert
        Assert.IsFalse(result);
    }
}