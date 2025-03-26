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
        Assert.IsNotNull(sceneCachedView.Panel, "SceneCached视图的GameObject应当存在");
        Assert.IsNotNull(sharedCachedView.Panel, "SharedCached视图的GameObject应当存在");
        Assert.IsNotNull(nonCachedView.Panel, "NonCached视图的GameObject应当存在");
        // 添加到缓存视图列表
        XView.cachedView.Add(sceneCachedView);
        XView.cachedView.Add(sharedCachedView);
        XView.cachedView.Add(nonCachedView);

        // 创建一个测试场景并卸载，触发sceneUnloaded事件
        var scene = SceneManager.CreateScene("MSVTestScene");
        SceneManager.SetActiveScene(scene);
        yield return SceneManager.UnloadSceneAsync(scene);

        // 验证Scene类型的缓存视图被移除，而Shared类型的视图保留
        Assert.AreEqual(2, XView.cachedView.Count, "缓存视图数量应当为2");
        Assert.IsFalse(XView.cachedView.Contains(sceneCachedView), "SceneCached视图应当被移除");
        Assert.IsTrue(XView.cachedView.Contains(sharedCachedView), "SharedCached视图应当保留");
        Assert.IsTrue(XView.cachedView.Contains(nonCachedView), "NonCached视图只在关闭时销毁");
        Assert.IsTrue(sceneCachedView.Panel == null && !sceneCachedView.Panel, "SceneCached视图的GameObject应当被销毁");
        Assert.IsTrue(sharedCachedView.Panel != null && sharedCachedView.Panel, "SharedCached视图的GameObject应当仍然存在");
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
        Assert.IsNotNull(view1, "视图1应当成功创建且不为空");
        Assert.IsNotNull(view2, "视图2应当成功创建且不为空");
        Assert.IsNotNull(view3, "视图3应当成功创建且不为空");
        Assert.IsTrue(view1.Panel.activeSelf, "视图1的面板应当处于激活状态");
        Assert.IsTrue(view2.Panel.activeSelf, "视图2的面板应当处于激活状态");
        Assert.IsTrue(view3.Panel.activeSelf, "视图3的面板应当处于激活状态");

        // 测试Close方法
        XView.Close(view1);
        XView.Close(view2);
        XView.Close(view3);
        Assert.IsTrue(view1.Panel == null, "CacheType.None类型的视图关闭后Panel应当被销毁");
        Assert.IsFalse(view2.Panel.activeSelf, "CacheType.Scene类型的视图关闭后Panel应当设为非活动状态");
        Assert.IsFalse(view3.Panel.activeSelf, "CacheType.Shared类型的视图关闭后Panel应当设为非活动状态");
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

        Assert.IsTrue(view1.Panel.activeSelf, "排除的视图1应当保持激活状态");
        Assert.IsFalse(view2.Panel.activeSelf, "视图2应当被关闭且处于非激活状态");
        Assert.IsFalse(view3.Panel.activeSelf, "视图3应当被关闭且处于非激活状态");
    }

    [Test]
    public void Meta()
    {
        // 验证Meta基本属性
        var meta = new XView.Meta("TestView", 10, XView.EventType.Dynamic, XView.CacheType.None, true);
        Assert.AreEqual("TestView", meta.Path, "Meta的Path属性应当正确设置为指定值");
        Assert.AreEqual(10, meta.FixedRQ, "Meta的FixedRQ属性应当正确设置为指定值");
        Assert.AreEqual(XView.EventType.Dynamic, meta.Focus, "Meta的Focus属性应当正确设置为指定值");
        Assert.AreEqual(XView.CacheType.None, meta.Cache, "Meta的Cache属性应当正确设置为指定值");
        Assert.AreEqual(true, meta.Multiple, "Meta的Multiple属性应当正确设置为指定值");
    }

    [Test]
    public void EventTest()
    {
        var obj1 = new GameObject("EventTestView1");
        var obj2 = new GameObject("EventTestView2");
        var eventView1 = obj1.AddComponent<MyEventView1>();
        var eventView2 = obj2.AddComponent<MyEventView2>();
        var isCalled = false;
        eventView1.Callback = () =>
        {
            isCalled = true;
        };

        // 测试Reg和Notify
        eventView1.Event.Reg(TestEvent.Test1, eventView1.Callback);
        eventView1.Event.Notify(TestEvent.Test1);
        Assert.IsTrue(isCalled, "注册事件后触发通知应当调用回调函数");

        // 测试Unreg
        isCalled = false;
        eventView1.Event.Unreg(TestEvent.Test1, eventView1.Callback);
        eventView1.Event.Notify(TestEvent.Test1);
        Assert.IsFalse(isCalled, "注销事件后触发通知不应当调用回调函数");

        // 测试跨实例事件
        isCalled = false;
        eventView2.Callback = () => isCalled = true;
        eventView2.View1 = eventView1;
        eventView1.Event.Notify(TestEvent.Test2);
        Assert.IsTrue(isCalled);
        isCalled = false;
        eventView1.Event.Unreg(TestEvent.Test2, eventView2.Callback);
        eventView1.Event.Notify(TestEvent.Test2);
        Assert.IsFalse(isCalled);

        // 测试泛型Reg和Notify
        var stringValue = "";
        eventView1.Event.Reg<string>(TestEvent.Test3, (str) => stringValue = str);
        eventView1.Event.Notify(TestEvent.Test3, "test");
        Assert.AreEqual("test", stringValue, "泛型事件应当正确传递字符串参数");

        // 测试双参数泛型
        int intValue = 0;
        bool boolValue = false;
        Action<int, bool> intBoolCallback = (i, b) =>
        {
            intValue = i;
            boolValue = b;
        };
        eventView1.Event.Reg<int, bool>(4, intBoolCallback);
        eventView1.Event.Notify(4, 42, true);
        Assert.AreEqual(42, intValue, "双参数泛型事件应当正确传递int参数");
        Assert.IsTrue(boolValue, "双参数泛型事件应当正确传递bool参数");

        // 测试三参数泛型
        float floatValue = 0f;
        char charValue = ' ';
        Action<int, float, char> tripleCallback = (i, f, c) =>
        {
            intValue = i;
            floatValue = f;
            charValue = c;
        };
        eventView1.Event.Reg<int, float, char>(5, tripleCallback);
        eventView1.Event.Notify(5, 100, 3.14f, 'A');
        Assert.AreEqual(100, intValue, "三参数泛型事件应当正确传递int参数");
        Assert.AreEqual(3.14f, floatValue, "三参数泛型事件应当正确传递float参数");
        Assert.AreEqual('A', charValue, "三参数泛型事件应当正确传递char参数");

        // 测试多泛型的Unreg
        eventView1.Event.Unreg<int, bool>(6, intBoolCallback);
        intValue = 0;
        boolValue = false;
        eventView1.Event.Notify(6, 99, true);
        Assert.AreEqual(0, intValue, "注销后的事件不应改变int参数值");
        Assert.IsFalse(boolValue, "注销后的事件不应改变bool参数值");

        // 测试Clear
        eventView1.Event.Clear();
        stringValue = "";
        eventView1.Event.Notify(2, null, "after clear");
        Assert.AreEqual("", stringValue, "清除事件后不应触发任何回调");
    }

    [Test]
    public void Load()
    {
        var parentTransform = new GameObject("Parent").transform;
        // 1. 测试加载普通视图
        var normalMeta = new XView.Meta("NormalView", 0, XView.EventType.Dynamic, XView.CacheType.None, false);
        var normalView = XView.Load(normalMeta, parentTransform, false);
        Assert.IsNotNull(normalView, "加载的视图不应为空");
        Assert.AreEqual(normalMeta, normalView.Meta, "加载的视图Meta属性应当与传入的Meta一致");
        Assert.IsNotNull(normalView.Panel, "加载的视图Panel不应为空");
        Assert.IsTrue(normalView.Panel.activeSelf, "加载的视图Panel应当处于激活状态");

        // 2. 测试加载多实例视图
        var multipleMeta = new XView.Meta("MultipleView", 0, XView.EventType.Dynamic, XView.CacheType.None, true);
        var multipleView1 = XView.Load(multipleMeta, parentTransform, false);
        var multipleView2 = XView.Load(multipleMeta, parentTransform, false);
        Assert.IsNotNull(multipleView1, "第一个多实例视图不应为空");
        Assert.IsNotNull(multipleView2, "第二个多实例视图不应为空");
        Assert.AreNotSame(multipleView1, multipleView2, "多实例视图应当创建不同的实例");
        Assert.IsTrue(multipleView1.Panel.activeSelf);

        // 3. 测试加载已存在的非多实例视图 (closeIfOpened = false)
        // 先打开视图使其进入openedView列表
        var openedView = XView.Open(normalMeta);
        Assert.IsNotNull(openedView, "打开的视图不应为空");
        Assert.IsTrue(openedView.Panel.activeSelf, "打开的视图Panel应当处于激活状态");
        // 尝试再次加载
        var existingView = XView.Load(normalMeta, parentTransform, false);
        Assert.IsNotNull(existingView, "加载已存在视图不应为空");
        Assert.AreSame(openedView, existingView, "closeIfOpened为false时应当返回已存在的视图实例");

        // 4. 测试加载已存在的非多实例视图 (closeIfOpened = true)
        XView.Close(normalMeta); // 先关闭之前的视图
        var openedViewAgain = XView.Open(normalMeta);
        Assert.IsNotNull(openedViewAgain, "重新打开的视图不应为空");
        // 尝试再次加载
        var newView = XView.Load(normalMeta, parentTransform, true);
        Assert.IsNotNull(newView, "新加载的视图不应为空");
        Assert.AreNotSame(openedViewAgain, newView, "closeIfOpened为true时应当创建新的视图实例");
        Assert.IsTrue(openedViewAgain.Panel == null, "原视图实例应当被关闭并销毁");

        // 5. 测试从缓存加载视图
        var cachedMeta = new XView.Meta("CachedView", 0, XView.EventType.Dynamic, XView.CacheType.Scene, false);
        var cachedView = XView.Open(cachedMeta);
        Assert.IsNotNull(cachedView, "缓存类型视图应当成功创建");
        // 关闭视图使其进入缓存
        XView.Close(cachedMeta);
        // 再次加载，应该从缓存获取
        var reloadedView = XView.Load(cachedMeta, parentTransform, false);
        Assert.IsNotNull(reloadedView, "从缓存加载的视图不应为空");
        Assert.AreSame(cachedView, reloadedView, "应当从缓存中获取到相同的视图实例");

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
        Assert.AreEqual(1, XView.openedView.IndexOf(view1), "view1应当位于索引1的位置");
        Assert.AreEqual(0, XView.openedView.IndexOf(view2), "view2应当位于索引0的位置");

        // 测试view被添加到above视图之后
        XView.openedView.Clear();
        XView.Sort(view1, null, null);
        XView.Sort(view2, null, view1);
        Assert.AreEqual(0, XView.openedView.IndexOf(view1), "view1应当位于索引0的位置");
        Assert.AreEqual(1, XView.openedView.IndexOf(view2), "view2应当位于索引1的位置");

        // 测试below和above都为null时，view被添加到末尾
        XView.openedView.Clear();
        XView.Sort(view1, null, null);
        XView.Sort(view2, null, null);
        Assert.AreEqual(0, XView.openedView.IndexOf(view1), "view1应当位于索引0的位置");
        Assert.AreEqual(1, XView.openedView.IndexOf(view2), "view2应当位于索引1的位置");

        // 渲染顺序测试
        // 测试FixedRQ的渲染顺序
        XView.openedView.Clear();
        myHandler.lastSetOrderView = null;
        myHandler.lastSetOrderValue = 0;
        XView.Sort(view1, null, null);
        Assert.AreSame(view1, myHandler.lastSetOrderView, "SetOrder方法应当使用正确的视图参数");
        Assert.AreEqual(500, myHandler.lastSetOrderValue, "普通视图的渲染顺序应当按公式计算");

        // 焦点状态测试
        // 测试EventType.Slience类型视图
        var silenceMeta = new XView.Meta("SilenceView", 0, XView.EventType.Slience);
        var silenceView = XView.Open(silenceMeta);
        var blurCalled = false;
        (silenceView as MyView).OnBlurCallback = () => blurCalled = true;

        // 确保视图先获得焦点
        XView.focusedView[silenceView] = true;
        myHandler.lastFocusedView = null;
        // Sort应该使Slience类型视图失去焦点
        XView.Sort(silenceView, null, null);
        Assert.IsNull(myHandler.lastFocusedView, "Slience类型视图不应调用SetFocus方法");
        Assert.IsTrue(blurCalled, "Slience类型视图应当调用OnBlur方法");
        Assert.IsFalse(XView.focusedView[silenceView], "Slience类型视图在focusedView中的标记应为false");

        // 测试EventType.Static和Dynamic类型视图
        XView.openedView.Clear();
        XView.focusedView.Clear();
        var staticMeta = new XView.Meta("StaticView", 0, XView.EventType.Static);
        var dynamicMeta = new XView.Meta("DynamicView", 0, XView.EventType.Dynamic);
        var staticView = XView.Open(staticMeta);
        var dynamicView = XView.Open(dynamicMeta);
        var staticFocusCalled = false;
        var dynamicFocusCalled = false;
        (staticView as MyView).OnFocusCallback = () => staticFocusCalled = true;
        (dynamicView as MyView).OnFocusCallback = () => dynamicFocusCalled = true;

        // 先清空焦点状态
        XView.openedView.Clear();
        XView.focusedView.Clear();

        // Sort后，Static类型视图应该获得焦点
        XView.Sort(staticView, null, null);
        Assert.IsTrue(staticFocusCalled, "Static类型视图应当调用OnFocus方法");
        Assert.IsTrue(XView.focusedView[staticView], "Static类型视图在focusedView中的标记应为true");

        // 添加Dynamic类型视图后，由于lastFocused已为true，Dynamic视图不应获得焦点
        XView.Sort(dynamicView, staticView, null);
        Assert.IsFalse(dynamicFocusCalled, "当lastFocused为true时，Dynamic类型视图不应调用OnFocus方法");
        Assert.IsFalse(XView.focusedView.ContainsKey(dynamicView) && XView.focusedView[dynamicView], "Dynamic类型视图在focusedView中的标记应为false");

        // 测试Panel为null的情况
        var testView = XView.Open(new XView.Meta("TestView"));
        GameObject.DestroyImmediate(testView.Panel);
        int initialCount = XView.openedView.Count;
        LogAssert.Expect(LogType.Error, new Regex("XView.Sort: view .* has already been destroyed."));
        XView.Sort(null, null, null); // 调用Sort应该清理无效视图
        Assert.AreEqual(initialCount - 1, XView.openedView.Count, "Panel为null的视图应当从openedView列表中移除");
    }

    [Test]
    public void Focus()
    {
        var view = XView.Open(testMeta);
        myHandler.lastFocusedView = null;
        XView.Focus(view);
        Assert.AreSame(view, myHandler.lastFocusedView, "Focus方法应当正确设置视图的焦点状态");
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
            Assert.IsNotNull(view, "异步加载的视图不应为空");
            Assert.AreEqual(asyncMeta.Path, view.Meta.Path, "异步加载的视图Meta路径应当与传入的Meta一致");
        });

        Assert.IsTrue(callbackCalled, "异步加载完成后应当调用回调函数");
    }

    [Test]
    public void BaseMethods()
    {
        // 测试Base类的基本方法
        var obj = new GameObject();
        var myView = obj.AddComponent<MyView>();
        myView.Meta = testMeta;
        myView.Panel = obj;

        Assert.IsNotNull(myView.Event, "视图的Event属性不应为空");
        Assert.IsNotNull(myView.Tags, "视图的Tags属性不应为空");

        // 测试生命周期方法
        var openCalled = false;
        var focusCalled = false;
        var blurCalled = false;
        var closeCalled = false;
        myView.OnOpenCallback = () => openCalled = true;
        myView.OnFocusCallback = () => focusCalled = true;
        myView.OnBlurCallback = () => blurCalled = true;
        myView.OnCloseCallback = () => closeCalled = true;

        myView.OnOpen();
        Assert.IsTrue(openCalled, "OnOpen方法应当调用OnOpenCallback");
        myView.OnFocus();
        Assert.IsTrue(focusCalled, "OnFocus方法应当调用OnFocusCallback");
        myView.OnBlur();
        Assert.IsTrue(blurCalled, "OnBlur方法应当调用OnBlurCallback");

        var closeDoneCalled = false;
        myView.OnClose(() => closeDoneCalled = true);
        Assert.IsTrue(closeCalled, "OnClose方法应当调用OnCloseCallback");
        Assert.IsTrue(closeDoneCalled, "OnClose方法应当执行传入的done回调");
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

    private class MyEventView1 : XView.Base
    {
        public Action Callback = null;
    }

    private class MyEventView2 : XView.Base
    {
        public Action Callback = null;
        private MyEventView1 view1;
        public MyEventView1 View1
        {
            get
            {
                return view1;
            }
            set
            {
                view1 = value;
                view1.Event.Reg(TestEvent.Test2, Callback);
            }
        }
    }
    enum TestEvent
    {
        Test1,
        Test2,
        Test3,
    }
}
#endif