// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using NUnit.Framework;
using EFramework.Modulize;

public class TestXScene
{
    private TestScene testScene;
    private TestSingletonScene testSingletonScene;

    [SetUp]
    public void Setup()
    {
        // 准备测试数据
        testScene = new TestScene();
        testSingletonScene = new TestSingletonScene();
    }

    [TearDown]
    public void Reset()
    {
        // 清理测试数据
        testScene = null;
        testSingletonScene = null;
    }

    [Test]
    public void Lifecycle()
    {
        // 验证基础生命周期方法
        bool awakeCalled = false;
        bool startCalled = false;
        bool updateCalled = false;
        bool resetCalled = false;
        bool stopCalled = false;

        var sceneImpl = new TestSceneImpl();
        sceneImpl.AwakeAction = () => awakeCalled = true;
        sceneImpl.StartAction = (args) => startCalled = true;
        sceneImpl.UpdateAction = () => updateCalled = true;
        sceneImpl.ResetAction = () => resetCalled = true;
        sceneImpl.StopAction = () => stopCalled = true;

        sceneImpl.Awake();
        Assert.That(awakeCalled, Is.True);

        sceneImpl.Start("test");
        Assert.That(startCalled, Is.True);

        sceneImpl.Update();
        Assert.That(updateCalled, Is.True);

        sceneImpl.Reset();
        Assert.That(resetCalled, Is.True);

        sceneImpl.Stop();
        Assert.That(stopCalled, Is.True);
    }

    [Test]
    public void Singleton()
    {
        // 验证单例模式
        var instance1 = TestSingletonScene.Instance;
        var instance2 = TestSingletonScene.Instance;

        Assert.That(instance1, Is.Not.Null);
        Assert.That(instance2, Is.Not.Null);
        Assert.That(instance1, Is.SameAs(instance2));
    }

    [Test]
    public void Manage()
    {
        // 验证场景管理功能
        bool onSwapCalled = false;
        XScene.OnSwap += () => onSwapCalled = true;

        // 设置场景代理
        XScene.OnProxy = (obj) => obj as XScene.IBase;

        // 切换到新场景
        XScene.Goto(testScene, "test");
        Assert.That(XScene.Next, Is.SameAs(testScene));
        Assert.That(XScene.Args, Is.EqualTo(new object[] { "test" }));

        // 模拟场景更新
        XScene.Update();
        Assert.That(XScene.Current, Is.SameAs(testScene));
        Assert.That(XScene.Last, Is.Null);
        Assert.That(XScene.Next, Is.Null);
        Assert.That(XScene.Args, Is.Null);
        Assert.That(onSwapCalled, Is.True);
    }

    [Test]
    public void Proxy()
    {
        // 验证场景代理未设置时的异常
        XScene.OnProxy = null;
        Assert.Throws<Exception>(() => XScene.Goto(new object()));
    }

    private class TestSceneImpl : XScene.Base
    {
        public Action AwakeAction { get; set; }
        public Action<object[]> StartAction { get; set; }
        public Action UpdateAction { get; set; }
        public Action ResetAction { get; set; }
        public Action StopAction { get; set; }

        public override void Awake()
        {
            AwakeAction?.Invoke();
        }

        public override void Start(params object[] args)
        {
            StartAction?.Invoke(args);
        }

        public override void Update()
        {
            UpdateAction?.Invoke();
        }

        public override void Reset()
        {
            ResetAction?.Invoke();
        }

        public override void Stop()
        {
            StopAction?.Invoke();
        }
    }

    private class TestScene : XScene.Base
    {
        public override string Name => "TestScene";
    }

    private class TestSingletonScene : XScene.Base<TestSingletonScene>
    {
        public override string Name => "TestSingletonScene";
    }
}
#endif