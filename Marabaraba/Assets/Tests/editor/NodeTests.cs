using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class NodeTests
{
    private Node node;
    private GameObject nodeObj;

    [SetUp]
    public void Setup()
    {
        nodeObj = new GameObject();
        node = nodeObj.AddComponent<Node>();
    }

    [TearDown]
    public void Teardown() => Object.DestroyImmediate(nodeObj);

    // NORMAL CASE: Setting owner
    [Test]
    public void OnClicked_SetsCorrectOwnerAndOccupiedState()
    {
        node.OnClicked(1, Color.red);
        Assert.AreEqual(1, node.owner);
        Assert.IsTrue(node.isOccupied);
    }

    // EMPTY/SPECIAL CASE: Clearing a node
    [Test]
    public void ClearNode_ResetsOwnerToZero()
    {
        node.OnClicked(2, Color.blue);
        node.ClearNode();
        Assert.AreEqual(0, node.owner);
        Assert.IsFalse(node.isOccupied);
    }
}
