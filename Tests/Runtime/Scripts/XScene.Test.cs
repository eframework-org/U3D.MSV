// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using NUnit.Framework;
using EFramework.Modulize;
using UnityEngine.TestTools;
using System.Collections;

public class TestXScene
{
    [Test]
    public void Singleton()
    {
        // 验证单例模式
        var instance1 = MySingletonScene.Instance;
        var instance2 = MySingletonScene.Instance;

        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreSame(instance1, instance2);
    }

    [UnityTest]
    public IEnumerator Manager()
    {
        var onSwapCalled = false;
        XScene.OnSwap += () => onSwapCalled = true;
        var testScene1 = new XScene.Base();
        var testScene2 = new XScene.Base();
        XScene.Current = testScene1;

        // 切换到新场景
        XScene.Goto(testScene2, "test");
        Assert.IsNull(XScene.Last);
        Assert.AreSame(XScene.Current, testScene1);
        Assert.AreSame(XScene.Next, testScene2);
        Assert.AreEqual("test", XScene.Args[0]);

        // 等一帧Update调用
        yield return null;
        Assert.AreSame(XScene.Current, testScene2);
        Assert.AreSame(XScene.Last, testScene1);
        Assert.IsNull(XScene.Next);
        Assert.IsNull(XScene.Args);
        Assert.IsTrue(onSwapCalled);

        // 测试异常
        var ex = Assert.Throws<Exception>(() => XScene.Goto(new object()));
        Assert.AreEqual("OnProxy is null", ex.Message);

        XScene.OnProxy = (obj) => obj as XScene.IBase;
        Assert.DoesNotThrow(() => XScene.Goto(new object()));
    }

    private class MySingletonScene : XScene.Base<MySingletonScene> { }
}
#endif