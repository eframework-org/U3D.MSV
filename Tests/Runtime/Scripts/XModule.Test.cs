// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using NUnit.Framework;
using EFramework.Modulize;

public class TestXModule
{
    private TestModule testModule;
    private TestSingletonModule testSingletonModule;

    [SetUp]
    public void Setup()
    {
        // 准备测试数据
        testModule = new TestModule();
        testSingletonModule = new TestSingletonModule();
    }

    [TearDown]
    public void Reset()
    {
        // 清理测试数据
        testModule = null;
        testSingletonModule = null;
    }

    [Test]
    public void Property()
    {
        // 验证基本属性
        Assert.That(testModule.Name, Is.EqualTo("TestModule"));
        Assert.That(testModule.Enabled, Is.False);
        Assert.That(testModule.Event, Is.Not.Null);
        Assert.That(testModule.Tags, Is.Not.Null);
        Assert.That(testModule.Tags.Get("Name"), Is.EqualTo("TestModule"));
    }

    [Test]
    public void Lifecycle()
    {
        // 验证基础生命周期方法
        bool awakeCalled = false;
        bool startCalled = false;
        bool resetCalled = false;
        bool stopCalled = false;

        var moduleImpl = new TestModuleImpl();
        moduleImpl.AwakeAction = () => awakeCalled = true;
        moduleImpl.StartAction = (args) => startCalled = true;
        moduleImpl.ResetAction = () => resetCalled = true;
        moduleImpl.StopAction = () => stopCalled = true;

        moduleImpl.Awake();
        Assert.That(awakeCalled, Is.True);

        moduleImpl.Start("test");
        Assert.That(startCalled, Is.True);
        Assert.That(moduleImpl.Enabled, Is.True);

        moduleImpl.Reset();
        Assert.That(resetCalled, Is.True);

        moduleImpl.Stop();
        Assert.That(stopCalled, Is.True);
        Assert.That(moduleImpl.Enabled, Is.False);
        Assert.That(moduleImpl.Event, Is.Not.Null);
    }

    [Test]
    public void Event()
    {
        // 验证事件系统
        bool eventTriggered = false;
        testModule.Event.Reg(1, () => eventTriggered = true);
        testModule.Event.Notify(1);
        Assert.That(eventTriggered, Is.True);

        testModule.Stop();
        eventTriggered = false;
        testModule.Event.Notify(1);
        Assert.That(eventTriggered, Is.False);
    }

    [Test]
    public void Singleton()
    {
        // 验证单例模式
        var instance1 = TestSingletonModule.Instance;
        var instance2 = TestSingletonModule.Instance;

        Assert.That(instance1, Is.Not.Null);
        Assert.That(instance2, Is.Not.Null);
        Assert.That(instance1, Is.SameAs(instance2));
    }

    private class TestModuleImpl : XModule.Base
    {
        public Action AwakeAction { get; set; }
        public Action<object[]> StartAction { get; set; }
        public Action ResetAction { get; set; }
        public Action StopAction { get; set; }

        public override void Awake()
        {
            AwakeAction?.Invoke();
            base.Awake();
        }

        public override void Start(params object[] args)
        {
            StartAction?.Invoke(args);
            base.Start(args);
        }

        public override void Reset()
        {
            ResetAction?.Invoke();
            base.Reset();
        }

        public override void Stop()
        {
            StopAction?.Invoke();
            base.Stop();
        }
    }

    private class TestModule : XModule.Base
    {
        public override string Name => "TestModule";
    }

    private class TestSingletonModule : XModule.Base<TestSingletonModule>
    {
        public override string Name => "TestSingletonModule";
    }
}
#endif