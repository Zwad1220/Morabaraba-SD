using NUnit.Framework;
using UnityEngine;

public class AIManagerTests
{
    AIManager ai;

    [SetUp]
    public void Setup()
    {
        GameObject obj = new GameObject();
        ai = obj.AddComponent<AIManager>();
    }

    [Test]
    public void test_ai_starts_disabled()
    {
        // Assert
        Assert.IsFalse(ai.isAIActive);
    }

    [Test]
    public void test_ai_can_be_enabled()
    {
        // Act
        ai.isAIActive = true;

        // Assert
        Assert.IsTrue(ai.isAIActive);
    }
}