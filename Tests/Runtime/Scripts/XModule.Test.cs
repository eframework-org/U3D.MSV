// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.Modulize;
using UnityEngine.TestTools;
using UnityEngine;
using System.Text.RegularExpressions;
using System;

public class TestXModule
{
    private XModule.Base testModule;

    [SetUp]
    public void Setup()
    {
        testModule = new XModule.Base();
    }

    [TearDown]
    public void Reset()
    {
        testModule = null;
    }

    [Test]
    public void Property()
    {
        // 验证基本属性
        Assert.AreEqual(testModule.Name, testModule.GetType().Name);
        testModule.name = "TestModule";
        Assert.AreEqual(testModule.Name, "TestModule");

        Assert.AreEqual(testModule.Enabled, false);
        Assert.IsNotNull(testModule.Event);
        Assert.IsNotNull(testModule.Tags);
        Assert.AreEqual(testModule.Tags.Get("Name"), "TestModule");
        Assert.IsNotNull(testModule.Tags.Get("Hash"));
    }

    [Test]
    public void Life()
    {
        // 验证基础生命周期方法
        LogAssert.Expect(LogType.Log, new Regex(@"Module has been awaked\."));
        LogAssert.Expect(LogType.Log, new Regex(@"Module has been started\."));
        LogAssert.Expect(LogType.Log, new Regex(@"Module has been reseted\."));
        LogAssert.Expect(LogType.Log, new Regex(@"Module has been stopped\."));

        testModule.Awake();
        testModule.Start();
        Assert.IsTrue(testModule.Enabled);
        testModule.Reset();
        testModule.Stop();
        Assert.IsFalse(testModule.Enabled);
    }

    [Test]
    public void Event()
    {
        var eventTriggered = false;
        Action callback = () => eventTriggered = true;

        // 验证注册事件
        testModule.Event.Reg(1, callback);
        testModule.Event.Notify(1);
        Assert.IsTrue(eventTriggered);

        // 验证注销事件
        testModule.Event.Unreg(1, callback);
        eventTriggered = false;
        testModule.Event.Notify(1);
        Assert.IsFalse(eventTriggered);
    }

    [Test]
    public void Singleton()
    {
        // 验证单例模式
        var instance1 = MySingletonModule.Instance;
        var instance2 = MySingletonModule.Instance;

        Assert.IsNotNull(instance1);
        Assert.IsNotNull(instance2);
        Assert.AreSame(instance1, instance2);
    }

    private class MySingletonModule : XModule.Base<MySingletonModule> { }
}
#endif