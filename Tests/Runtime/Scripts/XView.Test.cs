// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

#if UNITY_INCLUDE_TESTS
using NUnit.Framework;
using EFramework.Modulize;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.TestTools;
using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;

public class TestXView
{
    private XView.Meta testMeta;
    private GameObject testPanel;
    private MyHandler myHandler;

    [SetUp]
    public void Setup()
    {
        testMeta = new XView.Meta("TestView");
        testPanel = new GameObject("TestPanel");
        myHandler = new MyHandler();
        XView.Initialize(myHandler);
    }

    [TearDown]
    public void Teardown()
    {
        if (testPanel != null) GameObject.Destroy(testPanel);
        XView.CloseAll();
    }

    [UnityTest]
    public IEnumerator Initialize()
    {
        // 创建三种不同缓存类型的视图
        var sceneCachedMeta = new XView.Meta("SceneCachedView", 0, XView.EventType.Dynamic, XView.CacheType.Scene);
        var sharedCachedMeta = new XView.Meta("SharedCachedView", 0, XView.EventType.Dynamic, XView.CacheType.Shared);
        var nonCachedMeta = new XView.Meta("NonCachedView", 0, XView.EventType.Dynamic, XView.CacheType.None);

        // 创建视图并添加到缓存列表
        var sceneCachedView = XView.Open(sceneCachedMeta);
        var sharedCachedView = XView.Open(sharedCachedMeta);
        var nonCachedView = XView.Open(nonCachedMeta);

        // 确保视图的GameObject存在
        Assert.IsNotNull(sceneCachedView.Panel);
        Assert.IsNotNull(sharedCachedView.Panel);
        Assert.IsNotNull(nonCachedView.Panel);
        // 添加到缓存视图列表
        XView.cachedView.Add(sceneCachedView);
        XView.cachedView.Add(sharedCachedView);
        XView.cachedView.Add(nonCachedView);

        // 创建一个测试场景并卸载，触发sceneUnloaded事件
        var scene = SceneManager.CreateScene("TestScene");
        SceneManager.SetActiveScene(scene);
        yield return SceneManager.UnloadSceneAsync(scene);

        // 验证Scene类型的缓存视图被移除，而Shared类型的视图保留
        Assert.AreEqual(2, XView.cachedView.Count);
        Assert.IsFalse(XView.cachedView.Contains(sceneCachedView));
        Assert.IsTrue(XView.cachedView.Contains(sharedCachedView));
        Assert.IsTrue(XView.cachedView.Contains(nonCachedView));
        Assert.IsTrue(sceneCachedView.Panel == null && !sceneCachedView.Panel, "SceneCached视图的GameObject应该被销毁");
        Assert.IsTrue(sharedCachedView.Panel != null && sharedCachedView.Panel, "SharedCached视图的GameObject应该仍然存在");
        Assert.IsTrue(nonCachedView.Panel != null && nonCachedView.Panel, "NonCached视图只在关闭时销毁");
    }

    [Test]
    public void OpenAndClose()
    {
        // 测试不同缓存类型
        var meta1 = new XView.Meta("View1", 0, XView.EventType.Dynamic, XView.CacheType.None);
        var meta2 = new XView.Meta("View2", 0, XView.EventType.Dynamic, XView.CacheType.Scene);
        var meta3 = new XView.Meta("View3", 0, XView.EventType.Dynamic, XView.CacheType.Shared);

        // 测试Open方法
        var view1 = XView.Open(meta1);
        var view2 = XView.Open(meta2);
        var view3 = XView.Open(meta3);
        Assert.IsNotNull(view1);
        Assert.IsNotNull(view2);
        Assert.IsNotNull(view3);
        Assert.IsTrue(view1.Panel.activeSelf);
        Assert.IsTrue(view2.Panel.activeSelf);
        Assert.IsTrue(view3.Panel.activeSelf);

        // 测试Close方法
        XView.Close(view1);
        XView.Close(view2);
        XView.Close(view3);
        Assert.IsTrue(view1.Panel == null);
        Assert.IsFalse(view2.Panel.activeSelf);
        Assert.IsFalse(view3.Panel.activeSelf);
    }

    [Test]
    public void CloseAll()
    {
        // 测试CloseAll方法
        var meta1 = new XView.Meta("View1");
        var meta2 = new XView.Meta("View2");
        var meta3 = new XView.Meta("View3");

        var view1 = XView.Open(meta1);
        var view2 = XView.Open(meta2);
        var view3 = XView.Open(meta3);

        Assert.IsNotNull(view1);
        Assert.IsNotNull(view2);
        Assert.IsNotNull(view3);

        XView.CloseAll(meta1); // 关闭除meta1外的所有界面

        Assert.IsTrue(view1.Panel.activeSelf);
        Assert.IsFalse(view2.Panel.activeSelf);
        Assert.IsFalse(view3.Panel.activeSelf);
    }

    [Test]
    public void Meta()
    {
        // 验证Meta基本属性
        var meta = new XView.Meta("TestView", 10, XView.EventType.Dynamic, XView.CacheType.None, true);
        Assert.AreEqual("TestView", meta.Path);
        Assert.AreEqual(10, meta.FixedRQ);
        Assert.AreEqual(XView.EventType.Dynamic, meta.Focus);
        Assert.AreEqual(XView.CacheType.None, meta.Cache);
        Assert.AreEqual(true, meta.Multiple);
    }

    [Test]
    public void EventTest()
    {
        var viewEvent = new XView.Event();
        var triggered = false;
        Action callback = () =>
        {
            triggered = true;
        };

        // 测试Reg和Notify
        viewEvent.Reg(1, callback);
        viewEvent.Notify(1);
        Assert.IsTrue(triggered);

        // 测试Unreg
        triggered = false;
        viewEvent.Unreg(1, callback);
        viewEvent.Notify(1);
        Assert.IsFalse(triggered);

        // 测试泛型Reg和Notify
        var stringValue = "";
        viewEvent.Reg<string>(2, (str) => stringValue = str);
        viewEvent.Notify(2, null, "test");
        Assert.AreEqual("test", stringValue);

        // 测试Clear
        viewEvent.Clear();
        stringValue = "";
        viewEvent.Notify(2, null, "after clear");
        Assert.AreEqual("", stringValue);
    }

    [Test]
    public void Load()
    {
        var parentTransform = new GameObject("Parent").transform;
        // 1. 测试加载普通视图
        var normalMeta = new XView.Meta("NormalView", 0, XView.EventType.Dynamic, XView.CacheType.None, false);
        var normalView = XView.Load(normalMeta, parentTransform, false);
        Assert.IsNotNull(normalView);
        Assert.AreEqual(normalMeta, normalView.Meta);
        Assert.IsNotNull(normalView.Panel);
        Assert.IsTrue(normalView.Panel.activeSelf);

        // 2. 测试加载多实例视图
        var multipleMeta = new XView.Meta("MultipleView", 0, XView.EventType.Dynamic, XView.CacheType.None, true);
        var multipleView1 = XView.Load(multipleMeta, parentTransform, false);
        var multipleView2 = XView.Load(multipleMeta, parentTransform, false);
        Assert.IsNotNull(multipleView1);
        Assert.IsNotNull(multipleView2);
        Assert.AreNotSame(multipleView1, multipleView2); // 应该是不同实例
        Assert.IsTrue(multipleView1.Panel.activeSelf);

        // 3. 测试加载已存在的非多实例视图 (closeIfOpened = false)
        // 先打开视图使其进入openedView列表
        var openedView = XView.Open(normalMeta);
        Assert.IsNotNull(openedView);
        Assert.IsTrue(openedView.Panel.activeSelf);
        // 尝试再次加载
        var existingView = XView.Load(normalMeta, parentTransform, false);
        Assert.IsNotNull(existingView);
        Assert.AreSame(openedView, existingView); // 应该返回已存在的视图

        // 4. 测试加载已存在的非多实例视图 (closeIfOpened = true)
        XView.Close(normalMeta); // 先关闭之前的视图
        var openedViewAgain = XView.Open(normalMeta);
        Assert.IsNotNull(openedViewAgain);
        // 尝试再次加载
        var newView = XView.Load(normalMeta, parentTransform, true);
        Assert.IsNotNull(newView);
        Assert.AreNotSame(openedViewAgain, newView); // 应该创建新实例
        Assert.IsTrue(openedViewAgain.Panel == null); // 原实例应该被关闭

        // 5. 测试从缓存加载视图
        var cachedMeta = new XView.Meta("CachedView", 0, XView.EventType.Dynamic, XView.CacheType.Scene, false);
        var cachedView = XView.Open(cachedMeta);
        Assert.IsNotNull(cachedView);
        // 关闭视图使其进入缓存
        XView.Close(cachedMeta);
        // 再次加载，应该从缓存获取
        var reloadedView = XView.Load(cachedMeta, parentTransform, false);
        Assert.IsNotNull(reloadedView);
        Assert.AreSame(cachedView, reloadedView); // 应该是相同实例

        if (parentTransform != null) GameObject.Destroy(parentTransform.gameObject);
    }

    [Test]
    public void Find()
    {
        var parentTransform = new GameObject("Parent").transform;
        // 通过Load加载，不会自动加入openedView列表
        XView.Load(testMeta, parentTransform, false);
        var foundView = XView.Find(testMeta);
        Assert.IsNull(foundView);

        // 通过Open加载，会自动加入openedView列表
        var openedView = XView.Open(testMeta);
        foundView = XView.Find(testMeta);
        Assert.IsNotNull(foundView);
        Assert.AreSame(openedView, foundView);

        if (parentTransform != null) GameObject.Destroy(parentTransform.gameObject);
    }

    [Test]
    public void Sort()
    {
        // 创建测试视图
        var meta1 = new XView.Meta("View1", 0, XView.EventType.Dynamic);
        var meta2 = new XView.Meta("View2", 0, XView.EventType.Dynamic);
        var meta3 = new XView.Meta("View3", 0, XView.EventType.Dynamic);
        var meta4 = new XView.Meta("View4", 0, XView.EventType.Dynamic);
        var view1 = XView.Open(meta1);
        var view2 = XView.Open(meta2);
        var view3 = XView.Open(meta3);
        XView.openedView.Clear();

        // 测试view被添加到below视图之前
        XView.Sort(view2, view1, null);
        XView.Sort(view1, null, null);
        Assert.AreEqual(1, XView.openedView.IndexOf(view1));
        Assert.AreEqual(0, XView.openedView.IndexOf(view2));

        // 测试view被添加到above视图之后
        XView.openedView.Clear();
        XView.Sort(view1, null, null);
        XView.Sort(view2, null, view1);
        Assert.AreEqual(0, XView.openedView.IndexOf(view1));
        Assert.AreEqual(1, XView.openedView.IndexOf(view2));

        // 测试below和above都为null时，view被添加到末尾
        XView.openedView.Clear();
        XView.Sort(view1, null, null);
        XView.Sort(view2, null, null);
        Assert.AreEqual(0, XView.openedView.IndexOf(view1));
        Assert.AreEqual(1, XView.openedView.IndexOf(view2));

        // 渲染顺序测试
        // 测试FixedRQ的渲染顺序
        XView.openedView.Clear();
        myHandler.lastSetOrderView = null;
        myHandler.lastSetOrderValue = 0;
        XView.Sort(view1, null, null);
        Assert.AreSame(view1, myHandler.lastSetOrderView);
        Assert.AreEqual(500, myHandler.lastSetOrderValue);

        // 焦点状态测试
        // 测试EventType.Slience类型视图
        var silenceMeta = new XView.Meta("SilenceView", 0, XView.EventType.Slience);
        var silenceView = XView.Open(silenceMeta);
        bool blurCalled = false;
        (silenceView as MyView).OnBlurCallback = () => blurCalled = true;

        // 确保视图先获得焦点
        XView.focusedView[silenceView] = true;
        myHandler.lastFocusedView = null;
        // Sort应该使Slience类型视图失去焦点
        XView.Sort(silenceView, null, null);
        Assert.IsNull(myHandler.lastFocusedView); // 不应该调用handler的SetFocus
        Assert.IsTrue(blurCalled); // 应该调用OnBlur
        Assert.IsFalse(XView.focusedView[silenceView]); // focusedView标记应为false

        // 测试EventType.Static和Dynamic类型视图
        XView.openedView.Clear();
        XView.focusedView.Clear();
        var staticMeta = new XView.Meta("StaticView", 0, XView.EventType.Static);
        var dynamicMeta = new XView.Meta("DynamicView", 0, XView.EventType.Dynamic);
        var staticView = XView.Open(staticMeta);
        var dynamicView = XView.Open(dynamicMeta);
        bool staticFocusCalled = false;
        bool dynamicFocusCalled = false;
        (staticView as MyView).OnFocusCallback = () => staticFocusCalled = true;
        (dynamicView as MyView).OnFocusCallback = () => dynamicFocusCalled = true;

        // 先清空焦点状态
        XView.openedView.Clear();
        XView.focusedView.Clear();

        // Sort后，Static类型视图应该获得焦点
        XView.Sort(staticView, null, null);
        Assert.IsTrue(staticFocusCalled);
        Assert.IsTrue(XView.focusedView[staticView]);

        // 添加Dynamic类型视图后，由于lastFocused已为true，Dynamic视图不应获得焦点
        XView.Sort(dynamicView, staticView, null);
        Assert.IsFalse(dynamicFocusCalled);
        Assert.IsFalse(XView.focusedView.ContainsKey(dynamicView) && XView.focusedView[dynamicView]); // focusedView标记应为false

        // 测试Panel为null的情况
        var testView = XView.Open(new XView.Meta("TestView"));
        GameObject.DestroyImmediate(testView.Panel);
        int initialCount = XView.openedView.Count;
        LogAssert.Expect(LogType.Error, new Regex(@".*XView.Sort: view TestView has already been destroyed\."));
        XView.Sort(null, null, null); // 调用Sort应该清理无效视图
        Assert.AreEqual(initialCount - 1, XView.openedView.Count);
    }

    [Test]
    public void Focus()
    {
        var view = XView.Open(testMeta);
        myHandler.lastFocusedView = null;
        XView.Focus(view);
        Assert.AreSame(view, myHandler.lastFocusedView);
    }

    [UnityTest]
    public IEnumerator AsyncOperations()
    {
        // 测试异步操作
        var asyncMeta = new XView.Meta("AsyncView");
        var callbackCalled = false;

        yield return XView.OpenAsync(asyncMeta, (view) =>
        {
            callbackCalled = true;
            Assert.IsNotNull(view);
            Assert.AreEqual(asyncMeta.Path, view.Meta.Path);
        });

        Assert.IsTrue(callbackCalled);
    }

    [Test]
    public void BaseMethods()
    {
        // 测试Base类的基本方法
        var obj = new GameObject();
        var myView = obj.AddComponent<MyView>();
        myView.Meta = testMeta;
        myView.Panel = obj;

        Assert.IsNotNull(myView.Event);
        Assert.IsNotNull(myView.Tags);

        // 测试生命周期方法
        bool openCalled = false;
        bool focusCalled = false;
        bool blurCalled = false;
        bool closeCalled = false;
        myView.OnOpenCallback = () => openCalled = true;
        myView.OnFocusCallback = () => focusCalled = true;
        myView.OnBlurCallback = () => blurCalled = true;
        myView.OnCloseCallback = () => closeCalled = true;

        myView.OnOpen();
        Assert.IsTrue(openCalled);
        myView.OnFocus();
        Assert.IsTrue(focusCalled);
        myView.OnBlur();
        Assert.IsTrue(blurCalled);

        bool closeDoneCalled = false;
        myView.OnClose(() => closeDoneCalled = true);
        Assert.IsTrue(closeCalled);
        Assert.IsTrue(closeDoneCalled);
    }

    private class MyHandler : XView.IHandler
    {
        public XView.IBase lastFocusedView;
        public List<XView.IBase> viewOrder = new List<XView.IBase>();
        public XView.IBase lastSetOrderView;
        public int lastSetOrderValue;

        public void Load(XView.IMeta meta, Transform parent, out XView.IBase view, out GameObject panel)
        {
            panel = new GameObject(meta.Path);
            if (parent != null)
                panel.transform.SetParent(parent, false);

            var myView = panel.AddComponent<MyView>();
            myView.Meta = meta;
            myView.Panel = panel;
            view = myView;

            viewOrder.Add(view);
        }

        public void LoadAsync(XView.IMeta meta, Transform parent, Action<XView.IBase, GameObject> callback)
        {
            var panel = new GameObject(meta.Path);
            if (parent != null)
                panel.transform.SetParent(parent, false);

            var mockView = panel.AddComponent<MyView>();
            mockView.Meta = meta;
            mockView.Panel = panel;

            viewOrder.Add(mockView);
            callback?.Invoke(mockView, panel);
        }

        public bool Loading(XView.IMeta meta)
        {
            return false;
        }

        public void SetOrder(XView.IBase view, int order)
        {
            if (!viewOrder.Contains(view)) viewOrder.Add(view);

            // 记录最后一次设置的顺序
            lastSetOrderView = view;
            lastSetOrderValue = order;
        }

        public void SetFocus(XView.IBase view, bool focus)
        {
            if (focus) lastFocusedView = view;
        }
    }

    private class MyView : XView.Base
    {
        public Action OnOpenCallback;
        public Action OnFocusCallback;
        public Action OnBlurCallback;
        public Action OnCloseCallback;

        public override void OnOpen(params object[] args)
        {
            base.OnOpen(args);
            OnOpenCallback?.Invoke();
        }

        public override void OnFocus()
        {
            base.OnFocus();
            OnFocusCallback?.Invoke();
        }

        public override void OnBlur()
        {
            base.OnBlur();
            OnBlurCallback?.Invoke();
        }

        public override void OnClose(Action done)
        {
            OnCloseCallback?.Invoke();
            done?.Invoke();
            base.OnClose(done);
        }
    }
}
#endif