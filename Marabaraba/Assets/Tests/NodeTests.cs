using NUnit.Framework;
using UnityEngine;

public class NodeTests
{
    Node node;

    [SetUp]
    public void Setup()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        node = obj.AddComponent<Node>();
    }

    [Test]
    public void test_node_becomes_occupied_after_click()
    {
        // Arrange
        int player = 1;

        // Act
        node.OnClicked(player, Color.red);

        // Assert
        Assert.IsTrue(node.isOccupied);
    }

    [Test]
    public void test_node_owner_updates_after_click()
    {
        // Arrange
        int player = 2;

        // Act
        node.OnClicked(player, Color.blue);

        // Assert
        Assert.AreEqual(2, node.owner);
    }

    [Test]
    public void test_clear_node_resets_owner()
    {
        // Arrange
        node.OnClicked(1, Color.red);

        // Act
        node.ClearNode();

        // Assert
        Assert.AreEqual(0, node.owner);
    }

    [Test]
    public void test_clear_node_resets_occupation()
    {
        // Arrange
        node.OnClicked(1, Color.red);

        // Act
        node.ClearNode();

        // Assert
        Assert.IsFalse(node.isOccupied);
    }
}
