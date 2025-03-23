// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using System;
using NUnit.Framework;
using UnityEngine;
using EFramework.Modulize;

public class TestXView
{
    private XView.Meta testMeta;
    private GameObject testPanel;
    private XView.Base testView;

    [SetUp]
    public void Setup()
    {
        // 准备测试数据
        testMeta = new XView.Meta(
            path: "TestView",
            fixedRQ: 100,
            focus: XView.EventType.Dynamic,
            cache: XView.CacheType.Scene,
            multiple: false
        );

        testPanel = new GameObject("TestPanel");
        testView = testPanel.AddComponent<XView.Base>();
        testView.Meta = testMeta;
        testView.Panel = testPanel;
    }

    [TearDown]
    public void TearDown()
    {
        // 清理测试数据
        if (testPanel != null)
        {
            UnityEngine.Object.DestroyImmediate(testPanel);
        }
    }

    [Test]
    public void Meta_Initialization()
    {
        // 验证 Meta 类的初始化
        Assert.That(testMeta.Path, Is.EqualTo("TestView"));
        Assert.That(testMeta.FixedRQ, Is.EqualTo(100));
        Assert.That(testMeta.Focus, Is.EqualTo(XView.EventType.Dynamic));
        Assert.That(testMeta.Cache, Is.EqualTo(XView.CacheType.Scene));
        Assert.That(testMeta.Multiple, Is.False);
    }

    [Test]
    public void Base_EventSystem()
    {
        // 验证事件系统的初始化和清理
        Assert.That(testView.Event, Is.Not.Null);

        bool eventTriggered = false;
        testView.Event.Reg(1, () => eventTriggered = true);
        testView.Event.Notify(1);
        Assert.That(eventTriggered, Is.True);

        testView.OnDisable();
        eventTriggered = false;
        testView.Event.Notify(1);
        Assert.That(eventTriggered, Is.False);
    }

    [Test]
    public void Base_Lifecycle()
    {
        // 验证基础生命周期方法
        bool onOpenCalled = false;
        bool onFocusCalled = false;
        bool onBlurCalled = false;
        bool onCloseCalled = false;

        var testViewImpl = new TestViewImpl();
        testViewImpl.OnOpenAction = () => onOpenCalled = true;
        testViewImpl.OnFocusAction = () => onFocusCalled = true;
        testViewImpl.OnBlurAction = () => onBlurCalled = true;
        testViewImpl.OnCloseAction = (done) => { onCloseCalled = true; done(); };

        testViewImpl.OnOpen();
        Assert.That(onOpenCalled, Is.True);

        testViewImpl.OnFocus();
        Assert.That(onFocusCalled, Is.True);

        testViewImpl.OnBlur();
        Assert.That(onBlurCalled, Is.True);

        testViewImpl.OnClose(() => { });
        Assert.That(onCloseCalled, Is.True);
    }

    private class TestViewImpl : XView.Base
    {
        public Action OnOpenAction { get; set; }
        public Action OnFocusAction { get; set; }
        public Action OnBlurAction { get; set; }
        public Action<Action> OnCloseAction { get; set; }

        public override void OnOpen(params object[] args)
        {
            OnOpenAction?.Invoke();
        }

        public override void OnFocus()
        {
            OnFocusAction?.Invoke();
        }

        public override void OnBlur()
        {
            OnBlurAction?.Invoke();
        }

        public override void OnClose(Action done)
        {
            OnCloseAction?.Invoke(done);
        }
    }
}
#endif