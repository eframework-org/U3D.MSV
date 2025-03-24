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
        Assert.AreEqual(testModule.Name, testModule.GetType().Name, "模块名称默认应当为类型名称");
        testModule.name = "TestModule";
        Assert.AreEqual(testModule.Name, "TestModule", "设置name后Name属性应当正确反映");

        Assert.AreEqual(testModule.Enabled, false, "模块初始状态应当为未启用");
        Assert.IsNotNull(testModule.Event, "模块的事件系统不应为空");
        Assert.IsNotNull(testModule.Tags, "模块的标签不应为空");
        Assert.AreEqual(testModule.Tags.Get("Name"), "TestModule", "模块标签中的Name应当与模块名称一致");
        Assert.IsNotNull(testModule.Tags.Get("Hash"), "模块标签中应当包含Hash值");
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
        Assert.IsTrue(testModule.Enabled, "调用Start后模块应当处于启用状态");
        testModule.Reset();
        testModule.Stop();
        Assert.IsFalse(testModule.Enabled, "调用Stop后模块应当处于未启用状态");
    }

    [Test]
    public void Event()
    {
        var eventTriggered = false;
        Action callback = () => eventTriggered = true;

        // 验证注册事件
        testModule.Event.Reg(1, callback);
        testModule.Event.Notify(1);
        Assert.IsTrue(eventTriggered, "注册事件后触发通知应当调用回调函数");

        // 验证注销事件
        testModule.Event.Unreg(1, callback);
        eventTriggered = false;
        testModule.Event.Notify(1);
        Assert.IsFalse(eventTriggered, "注销事件后触发通知不应当调用回调函数");
    }

    [Test]
    public void Singleton()
    {
        // 验证单例模式
        var instance1 = MySingletonModule.Instance;
        var instance2 = MySingletonModule.Instance;

        Assert.IsNotNull(instance1, "单例实例1不应为空");
        Assert.IsNotNull(instance2, "单例实例2不应为空");
        Assert.AreSame(instance1, instance2, "两次获取的单例实例应当是同一个对象");
    }

    private class MySingletonModule : XModule.Base<MySingletonModule> { }
}
#endif