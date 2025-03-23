// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using UnityEngine;
using EFramework.Utility;
using UnityEngine.SceneManagement;

namespace EFramework.Modulize
{
    /// <summary>
    /// 视图管理模块，提供了视图的加载、显示、排序和事件管理等功能。该模块支持同步和异步加载，并提供了灵活的视图缓存和事件处理机制。
    /// </summary>
    public partial class XView
    {
        /// <summary>
        /// 定义了界面的事件类型。
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// 动态事件，会触发界面的焦点变化。
            /// </summary>
            Dynamic,

            /// <summary>
            /// 静态事件，不会触发界面的焦点变化。
            /// </summary>
            Static,

            /// <summary>
            /// 静默事件，不会触发界面的焦点变化和事件通知。
            /// </summary>
            Slience,
        }

        /// <summary>
        /// 定义了界面的缓存类型。
        /// </summary>
        public enum CacheType
        {
            /// <summary>
            /// 场景缓存，场景切换时会被清理。
            /// </summary>
            Scene,

            /// <summary>
            /// 共享缓存，场景切换时不会被清理。
            /// </summary>
            Shared,

            /// <summary>
            /// 不缓存，关闭时会被销毁。
            /// </summary>
            None,
        }

        /// <summary>
        /// 定义了界面的元数据接口，包含界面的基本配置信息。
        /// </summary>
        public interface IMeta
        {
            /// <summary>
            /// 获取界面的预制体路径。
            /// </summary>
            string Path { get; }

            /// <summary>
            /// 获取界面的固定渲染顺序。
            /// </summary>
            int FixedRQ { get; }

            /// <summary>
            /// 获取界面的事件类型。
            /// </summary>
            EventType Focus { get; }

            /// <summary>
            /// 获取界面的缓存类型。
            /// </summary>
            CacheType Cache { get; }

            /// <summary>
            /// 获取是否允许多个实例。
            /// </summary>
            bool Multiple { get; }
        }

        /// <summary>
        /// 提供了界面的元数据实现。
        /// </summary>
        public class Meta : IMeta
        {
            /// <summary>
            /// 获取或设置界面的预制体路径。
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// 获取或设置界面的固定渲染顺序。
            /// </summary>
            public int FixedRQ { get; set; }

            /// <summary>
            /// 获取或设置界面的事件类型。
            /// </summary>
            public EventType Focus { get; set; }

            /// <summary>
            /// 获取或设置界面的缓存类型。
            /// </summary>
            public CacheType Cache { get; set; }

            /// <summary>
            /// 获取或设置是否允许多个实例。
            /// </summary>
            public bool Multiple { get; set; }

            /// <summary>
            /// 初始化元数据。
            /// </summary>
            public Meta() { }

            /// <summary>
            /// 使用指定参数初始化元数据。
            /// </summary>
            /// <param name="path">预制体路径</param>
            /// <param name="fixedRQ">固定渲染顺序</param>
            /// <param name="focus">事件类型</param>
            /// <param name="cache">缓存类型</param>
            /// <param name="multiple">是否允许多个实例</param>
            public Meta(string path, int fixedRQ = 0, EventType focus = EventType.Dynamic, CacheType cache = CacheType.Scene, bool multiple = false)
            {
                Path = path;
                FixedRQ = fixedRQ;
                Cache = cache;
                Focus = focus;
                Multiple = multiple;
            }
        }

        /// <summary>
        /// 定义了界面的加载处理器接口。
        /// </summary>
        public interface IHandler
        {
            /// <summary>
            /// 同步加载界面。
            /// </summary>
            /// <param name="meta">界面元数据</param>
            /// <param name="parent">父级变换</param>
            /// <param name="view">界面实例</param>
            /// <param name="panel">界面面板</param>
            void Load(IMeta meta, Transform parent, out IBase view, out GameObject panel);

            /// <summary>
            /// 异步加载界面。
            /// </summary>
            /// <param name="meta">界面元数据</param>
            /// <param name="parent">父级变换</param>
            /// <param name="callback">加载完成回调</param>
            void LoadAsync(IMeta meta, Transform parent, Action<IBase, GameObject> callback);

            /// <summary>
            /// 检查界面是否正在加载。
            /// </summary>
            /// <param name="meta">界面元数据</param>
            /// <returns>是否正在加载</returns>
            bool Loading(IMeta meta);

            /// <summary>
            /// 设置界面的渲染顺序。
            /// </summary>
            /// <param name="view">界面实例</param>
            /// <param name="order">渲染顺序</param>
            void SetOrder(IBase view, int order);

            /// <summary>
            /// 设置界面的焦点状态。
            /// </summary>
            /// <param name="view">界面实例</param>
            /// <param name="focus">是否获得焦点</param>
            void SetFocus(IBase view, bool focus);
        }

        /// <summary>
        /// 提供了界面的事件系统实现，支持事件注册、注销和通知。
        /// </summary>
        public class Event : XEvent.Manager
        {
            /// <summary>
            /// 事件代理类，用于管理事件回调的上下文。
            /// </summary>
            protected class EvtProxy
            {
                public int ID;

                public XEvent.Manager Context;

                public XEvent.Callback Callback;
            }

            /// <summary>
            /// 事件上下文。
            /// </summary>
            protected readonly XEvent.Manager context;

            /// <summary>
            /// 事件代理容器。
            /// </summary>
            protected readonly Dictionary<int, List<EvtProxy>> proxies = new();

            public Event(XEvent.Manager context = null) { this.context = context ?? this; }

            /// <summary>
            /// 注册事件回调。
            /// </summary>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">是否只触发一次</param>
            /// <returns>是否注册成功</returns>
            public virtual bool Reg(int eid, XEvent.Callback callback, XEvent.Manager manager = null, bool once = false)
            {
                if (callback == null) return false;
                manager ??= context;

                var ret = manager.Reg(eid, callback, once);
                if (ret)
                {
                    if (!proxies.TryGetValue(eid, out var list))
                    {
                        list = new List<EvtProxy>();
                        proxies.Add(eid, list);
                    }

                    var proxy = new EvtProxy
                    {
                        ID = callback.GetHashCode(),
                        Context = manager,
                        Callback = callback
                    };
                    list.Add(proxy);
                }
                return ret;
            }

            /// <summary>
            /// 注册带一个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">是否只触发一次</param>
            /// <returns>是否注册成功</returns>
            public virtual bool Reg<T1>(int eid, Action<T1> callback, XEvent.Manager manager = null, bool once = false)
            {
                if (callback == null) return false;
                manager ??= context;

                var ncallback = new XEvent.Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    callback?.Invoke(arg1);
                });

                return Reg(eid, ncallback, manager, once);
            }

            /// <summary>
            /// 注册带一个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件枚举</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">是否只触发一次</param>
            /// <returns>是否注册成功</returns>
            public virtual bool Reg<T1>(Enum eid, Action<T1> callback, XEvent.Manager manager = null, bool once = false)
            {
                return Reg(eid.GetHashCode(), callback, manager, once);
            }

            /// <summary>
            /// 注册带两个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">是否只触发一次</param>
            /// <returns>是否注册成功</returns>
            public virtual bool Reg<T1, T2>(int eid, Action<T1, T2> callback, XEvent.Manager manager = null, bool once = false)
            {
                if (callback == null) return false;
                manager ??= context;

                var ncallback = new XEvent.Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    var arg2 = args != null && args.Length > 1 ? (T2)args[1] : default;
                    callback?.Invoke(arg1, arg2);
                });

                return Reg(eid, ncallback, manager, once);
            }

            /// <summary>
            /// 注册带两个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件枚举</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">是否只触发一次</param>
            /// <returns>是否注册成功</returns>
            public virtual bool Reg<T1, T2>(Enum eid, Action<T1, T2> callback, XEvent.Manager manager = null, bool once = false)
            {
                return Reg(eid.GetHashCode(), callback, manager, once);
            }

            /// <summary>
            /// 注册带三个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">是否只触发一次</param>
            /// <returns>是否注册成功</returns>
            public virtual bool Reg<T1, T2, T3>(int eid, Action<T1, T2, T3> callback, XEvent.Manager manager = null, bool once = false)
            {
                if (callback == null) return false;
                manager ??= context;

                var ncallback = new XEvent.Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    var arg2 = args != null && args.Length > 1 ? (T2)args[1] : default;
                    var arg3 = args != null && args.Length > 2 ? (T3)args[2] : default;
                    callback?.Invoke(arg1, arg2, arg3);
                });

                return Reg(eid, ncallback, manager, once);
            }

            /// <summary>
            /// 注册带三个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件枚举</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">是否只触发一次</param>
            /// <returns>是否注册成功</returns>
            public virtual bool Reg<T1, T2, T3>(Enum eid, Action<T1, T2, T3> callback, XEvent.Manager manager = null, bool once = false)
            {
                return Reg(eid.GetHashCode(), callback, manager, once);
            }

            /// <summary>
            /// 注销事件回调。
            /// </summary>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否注销成功</returns>
            public override bool Unreg(int eid, XEvent.Callback callback = null)
            {
                var ret = true;
                if (proxies.TryGetValue(eid, out var list))
                {
                    if (callback != null)
                    {
                        var hashCode = callback.GetHashCode();
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            var proxy = list[i];
                            if (proxy.ID == hashCode)
                            {
                                ret &= proxy.Context.Unreg(eid, proxy.Callback);
                                list.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        foreach (var proxy in list)
                        {
                            ret &= proxy.Context.Unreg(eid, proxy.Callback);
                        }
                        proxies.Remove(eid);
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销带一个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否注销成功</returns>
            public override bool Unreg<T1>(int eid, Action<T1> callback)
            {
                if (callback == null) return false;

                var ret = false;
                if (proxies.TryGetValue(eid, out var list))
                {
                    var hashCode = callback.GetHashCode();
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var proxy = list[i];
                        if (proxy.ID == hashCode)
                        {
                            ret |= proxy.Context.Unreg(eid, proxy.Callback);
                            list.RemoveAt(i);
                        }
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销带一个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件枚举</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否注销成功</returns>
            public override bool Unreg<T1>(Enum eid, Action<T1> callback)
            {
                return Unreg(eid.GetHashCode(), callback);
            }

            /// <summary>
            /// 注销带两个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否注销成功</returns>
            public override bool Unreg<T1, T2>(int eid, Action<T1, T2> callback)
            {
                if (callback == null) return false;

                var ret = false;
                if (proxies.TryGetValue(eid, out var list))
                {
                    var hashCode = callback.GetHashCode();
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var proxy = list[i];
                        if (proxy.ID == hashCode)
                        {
                            ret |= proxy.Context.Unreg(eid, proxy.Callback);
                            list.RemoveAt(i);
                        }
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销带两个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件枚举</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否注销成功</returns>
            public override bool Unreg<T1, T2>(Enum eid, Action<T1, T2> callback)
            {
                return Unreg(eid.GetHashCode(), callback);
            }

            /// <summary>
            /// 注销带三个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件ID</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否注销成功</returns>
            public override bool Unreg<T1, T2, T3>(int eid, Action<T1, T2, T3> callback)
            {
                if (callback == null) return false;

                var ret = false;
                if (proxies.TryGetValue(eid, out var list))
                {
                    var hashCode = callback.GetHashCode();
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var proxy = list[i];
                        if (proxy.ID == hashCode)
                        {
                            ret |= proxy.Context.Unreg(eid, proxy.Callback);
                            list.RemoveAt(i);
                        }
                    }
                }
                return ret;
            }

            /// <summary>
            /// 注销带三个参数的事件回调。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件枚举</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否注销成功</returns>
            public override bool Unreg<T1, T2, T3>(Enum eid, Action<T1, T2, T3> callback)
            {
                return Unreg(eid.GetHashCode(), callback);
            }

            /// <summary>
            /// 清理所有事件回调。
            /// </summary>
            public override void Clear()
            {
                foreach (var kvp in proxies)
                {
                    foreach (var proxy in kvp.Value)
                    {
                        proxy.Context.Unreg(kvp.Key, proxy.Callback);
                    }
                }
                proxies.Clear();
                base.Clear();
            }

            /// <summary>
            /// 通知事件。
            /// </summary>
            /// <param name="eid">事件枚举</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(Enum eid, XEvent.Manager manager = null, params object[] args)
            {
                manager ??= context;
                manager.Notify(eid.GetHashCode(), args);
            }

            /// <summary>
            /// 通知事件。
            /// </summary>
            /// <param name="eid">事件ID</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(int eid, XEvent.Manager manager = null, params object[] args)
            {
                manager ??= context;
                manager.Notify(eid, args);
            }
        }

        /// <summary>
        /// 定义了界面的基础接口。
        /// </summary>
        public interface IBase
        {
            /// <summary>
            /// 获取或设置界面的元数据。
            /// </summary>
            IMeta Meta { get; set; }

            /// <summary>
            /// 获取或设置界面的面板对象。
            /// </summary>
            GameObject Panel { get; set; }

            /// <summary>
            /// 界面打开时调用。
            /// </summary>
            /// <param name="args">打开参数</param>
            void OnOpen(params object[] args);

            /// <summary>
            /// 界面获得焦点时调用。
            /// </summary>
            void OnFocus();

            /// <summary>
            /// 界面失去焦点时调用。
            /// </summary>
            void OnBlur();

            /// <summary>
            /// 界面关闭时调用。
            /// </summary>
            /// <param name="done">关闭完成回调</param>
            void OnClose(Action done);
        }
    }

    public partial class XView
    {
        /// <summary>
        /// 提供了界面的基础实现。
        /// </summary>
        public class Base : MonoBehaviour, IBase
        {
            /// <summary>
            /// 获取或设置界面的元数据。
            /// </summary>
            public virtual IMeta Meta { get; set; }

            /// <summary>
            /// 获取或设置界面的面板对象。
            /// </summary>
            public virtual GameObject Panel { get; set; }

            internal XEvent.Manager @event;
            /// <summary>
            /// 获取界面的事件管理器。
            /// </summary>
            public virtual XEvent.Manager Event { get { @event ??= new Event(); return @event; } }

            internal XLog.LogTag tags;
            /// <summary>
            /// 获取或设置界面的日志标签。
            /// </summary>
            public virtual XLog.LogTag Tags
            {
                get
                {
                    if (tags == null)
                    {
                        tags = XLog.GetTag();
                        tags.Set("Name", this ? "null" : name);
                        tags.Set("Comp", GetType().FullName);
                        tags.Set("Hash", GetHashCode().ToString());
                    }
                    return tags;
                }
                set => tags = value;
            }

            /// <summary>
            /// 初始化界面。
            /// </summary>
            public Base() { }

            /// <summary>
            /// 使用代理初始化界面。
            /// </summary>
            /// <param name="proxy">代理对象</param>
            public Base(object proxy) { } // PuerTS 类型导出

            /// <summary>
            /// 界面初始化时调用。
            /// </summary>
            public virtual void Awake() { }

            /// <summary>
            /// 界面启用时调用。
            /// </summary>
            public virtual void OnEnable() { }

            /// <summary>
            /// 界面打开时调用。
            /// </summary>
            /// <param name="args">打开参数</param>
            public virtual void OnOpen(params object[] args) { }

            /// <summary>
            /// 界面获得焦点时调用。
            /// </summary>
            public virtual void OnFocus() { }

            /// <summary>
            /// 界面失去焦点时调用。
            /// </summary>
            public virtual void OnBlur() { }

            /// <summary>
            /// 界面禁用时调用。
            /// </summary>
            public virtual void OnDisable() { Event?.Clear(); }

            /// <summary>
            /// 界面关闭时调用。
            /// </summary>
            /// <param name="done">关闭完成回调</param>
            public virtual void OnClose(Action done) { done(); }

            /// <summary>
            /// 设置界面焦点。
            /// </summary>
            public virtual void Focus() { XView.Focus(this); }

            /// <summary>
            /// 关闭界面。
            /// </summary>
            /// <param name="resume">是否恢复焦点</param>
            public virtual void Close(bool resume = true) { XView.Close(this, resume); }
        }

        /// <summary>
        /// 提供了带模块的界面基础实现。
        /// </summary>
        /// <typeparam name="TModule">模块类型</typeparam>
        public class Base<TModule> : Base where TModule : XModule.IBase, new()
        {
            /// <summary>
            /// 获取界面的模块实例。
            /// </summary>
            public TModule Module { get => XModule.Base<TModule>.Instance; }

            /// <summary>
            /// 获取界面的事件管理器。
            /// </summary>
            public override XEvent.Manager Event { get { @event ??= new Event(Module.Event); return @event; } }

            /// <summary>
            /// 获取或设置界面的日志标签。
            /// </summary>
            public override XLog.LogTag Tags
            {
                get
                {
                    if (tags == null)
                    {
                        tags = XLog.GetTag();
                        tags.Set("Name", this ? "null" : name);
                        tags.Set("Comp", GetType().FullName);
                        tags.Set("Hash", GetHashCode().ToString());
                        tags.Set("Module", Module == null ? "null" : Module.Name);
                    }
                    return tags;
                }
                set => tags = value;
            }
        }
    }

    /// <summary>
    /// 视图管理类，提供界面的加载、显示、隐藏和焦点管理功能。
    /// </summary>
    public partial class XView
    {
        /// <summary>
        /// 共享的界面加载处理器。
        /// </summary>
        internal static IHandler sharedHandler;

        /// <summary>
        /// 缓存的界面列表。
        /// </summary>
        internal static List<IBase> cachedView = new();

        /// <summary>
        /// 打开的界面列表。
        /// </summary>
        internal static List<IBase> openedView = new();

        /// <summary>
        /// 获得焦点的界面字典。
        /// </summary>
        internal static readonly Dictionary<IBase, bool> focusedView = new();

        /// <summary>
        /// 初始化视图系统。
        /// </summary>
        /// <param name="handler">界面加载处理器</param>
        public static void Initialize(IHandler handler)
        {
            sharedHandler = handler ?? throw new ArgumentNullException("handler");
            SceneManager.sceneUnloaded += scene =>
            {
                if (!scene.isSubScene)
                {
                    for (var i = 0; i < cachedView.Count;)
                    {
                        var view = cachedView[i];
                        if (view.Meta.Cache == CacheType.Scene)
                        {
                            view.Panel.DestroyGO(true);
                            cachedView.RemoveAt(i);
                            XLog.Info("XView.Initialize: removed scene-cached view: {0}.", view.Meta.Path);
                        }
                        else { i++; }
                    }
                }
            };
        }

        /// <summary>
        /// 加载界面。
        /// </summary>
        /// <param name="meta">界面元数据</param>
        /// <param name="parent">父级变换</param>
        /// <param name="closeIfOpened">如果已打开则关闭</param>
        /// <returns>界面实例</returns>
        public static IBase Load(IMeta meta, Transform parent, bool closeIfOpened)
        {
            IBase view = null;
            if (!meta.Multiple)
            {
                view = Find(meta);
                if (closeIfOpened && view != null)
                {
                    Close(meta, false);
                    view = null;
                }
            }
            if (view == null)
            {
                for (var i = 0; i < cachedView.Count; i++)
                {
                    var temp = cachedView[i];
                    if (temp.Meta.Path == meta.Path)
                    {
                        view = temp;
                        if (view.Panel.activeSelf) view.Panel.SetActiveState(false);
                        cachedView.RemoveAt(i);
                        break;
                    }
                }
            }
            if (view == null)
            {
                sharedHandler.Load(meta, parent, out view, out var panel);
                if (view != null)
                {
                    view.Meta = meta;
                    view.Panel = panel;
                    sharedHandler.SetFocus(view, true);
                }
            }
            return view;
        }

        /// <summary>
        /// 查找界面。
        /// </summary>
        /// <param name="meta">界面元数据</param>
        /// <returns>界面实例</returns>
        public static IBase Find(IMeta meta)
        {
            if (meta != null)
            {
                for (var i = 0; i < openedView.Count; i++)
                {
                    var view = openedView[i];
                    if (view.Meta.Path == meta.Path)
                    {
                        return view;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="target">目标界面</param>
        /// <param name="args">打开参数</param>
        /// <returns>界面实例</returns>
        public static IBase Open(IMeta target, params object[] args)
        {
            // [TONOTE]: 兼容js拦截函数参数类型匹配错误问题
            if (args.Length > 0 && args[0] is Transform transform)
            {
                var nargs = new object[args.Length - 1];
                Array.Copy(args, 1, nargs, 0, args.Length - 1);
                return Open(target, transform, nargs);
            }
            else if (args.Length > 2 && args[0] is IMeta meta && args[1] is IMeta meta1 && args[2] is Transform transform1)
            {
                var nargs = new object[args.Length - 3];
                Array.Copy(args, 3, nargs, 0, args.Length - 3);
                return Open(target, meta, meta1, transform1, nargs);
            }
            else return Open(target, null, null, null, args);
        }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="target">目标界面</param>
        /// <param name="parent">父级变换</param>
        /// <param name="args">打开参数</param>
        /// <returns>界面实例</returns>
        public static IBase Open(IMeta target, Transform parent, params object[] args) { return Open(target, null, null, parent, args); }

        /// <summary>
        /// 打开界面。
        /// </summary>
        /// <param name="target">目标界面</param>
        /// <param name="below">下方界面</param>
        /// <param name="above">上方界面</param>
        /// <param name="parent">父级变换</param>
        /// <param name="args">打开参数</param>
        /// <returns>界面实例</returns>
        public static IBase Open(IMeta target, IMeta below, IMeta above, Transform parent, params object[] args)
        {
            var view = Load(target, parent, true);
            if (view == null) XLog.Error("XView.Open: error caused by nil IView obj, please check it {0}.", target.Path);
            else view.Panel.SetActiveState(true);
            var belowWin = Find(below);
            var aboveWin = Find(above);
            Sort(view, belowWin, aboveWin);
            view.OnOpen(args);
            return view;
        }

        public static bool OpenAsync(IMeta target, Action<IBase> callback = null, params object[] args) { return OpenAsync(target, null, null, null, callback, args); }

        public static bool OpenAsync(IMeta target, Transform parent, Action<IBase> callback = null, params object[] args) { return OpenAsync(target, null, null, parent, callback, args); }

        /// <summary>
        /// 异步打开界面。
        /// </summary>
        /// <param name="target">目标界面</param>
        /// <param name="below">下方界面</param>
        /// <param name="above">上方界面</param>
        /// <param name="parent">父级变换</param>
        /// <param name="callback">打开完成回调</param>
        /// <param name="args">打开参数</param>
        /// <returns>是否开始打开</returns>
        public static bool OpenAsync(IMeta target, IMeta below, IMeta above, Transform parent, Action<IBase> callback = null, params object[] args)
        {
            IBase view = null;
            if (!target.Multiple)
            {
                view = Find(target);
                if (view != null)
                {
                    Close(target, false);
                    view = null;
                }
            }
            if (view == null)
            {
                for (var i = 0; i < cachedView.Count; i++)
                {
                    var temp = cachedView[i];
                    if (temp.Meta.Path == target.Path)
                    {
                        view = temp;
                        if (view.Panel.activeSelf) view.Panel.SetActiveState(false);
                        cachedView.RemoveAt(i);
                        break;
                    }
                }
            }
            if (view == null)
            {
                if (!target.Multiple && sharedHandler.Loading(target))
                {
                    try { callback?.Invoke(null); }
                    catch (Exception e) { XLog.Panic(e); }
                    return false;
                }
                else
                {
                    sharedHandler.LoadAsync(target, parent, (view, panel) =>
                    {
                        if (view != null)
                        {
                            view.Meta = target;
                            view.Panel = panel;
                            sharedHandler.SetFocus(view, true);

                            panel.SetActiveState(true);
                            var belowWin = Find(below);
                            var aboveWin = Find(above);
                            Sort(view, belowWin, aboveWin);
                            view.OnOpen(args);
                        }
                        try { callback?.Invoke(view); }
                        catch (Exception e) { XLog.Panic(e); }
                    });
                }
            }
            else
            {
                view.Panel.SetActiveState(true);
                var belowWin = Find(below);
                var aboveWin = Find(above);
                Sort(view, belowWin, aboveWin);
                view.OnOpen(args);

                try { callback?.Invoke(view); }
                catch (Exception e) { XLog.Panic(e); }
            }
            return true;
        }

        /// <summary>
        /// 排序界面。
        /// </summary>
        /// <param name="view">目标界面</param>
        /// <param name="below">下方界面</param>
        /// <param name="above">上方界面</param>
        public static void Sort(IBase view, IBase below, IBase above)
        {
            if (view != null)
            {
                var inserted = false;
                if (below != null)
                {
                    for (var i = 0; i < openedView.Count; i++)
                    {
                        var temp = openedView[i];
                        if (temp == below)
                        {
                            openedView.Insert(i, view);
                            inserted = true;
                            break;
                        }
                    }
                }
                else if (above != null)
                {
                    for (var i = 0; i < openedView.Count; i++)
                    {
                        var temp = openedView[i];
                        if (temp == above)
                        {
                            openedView.Insert(i + 1, view);
                            inserted = true;
                            break;
                        }
                    }
                }
                if (!inserted)
                {
                    openedView.Add(view);
                }
            }
            var index = openedView.Count - 1;
            var rqIndex = index;
            var lastFocused = false;
            while (index >= 0)
            {
                var temp = openedView[index];
                if (temp.Panel == null)
                {
                    XLog.Error("XView.Sort: view {0} has already been destroyed.", temp.Meta.Path);
                    openedView.RemoveAt(index);
                    focusedView.Remove(temp);
                }
                else
                {
                    focusedView.TryGetValue(temp, out var focused);
                    if (temp.Meta.FixedRQ > 0)
                    {
                        sharedHandler.SetOrder(temp, temp.Meta.FixedRQ);
                    }
                    else
                    {
                        sharedHandler.SetOrder(temp, 1000 + (rqIndex - 1) * 500);
                        rqIndex -= 1;
                    }
                    if (temp.Meta.Focus == EventType.Slience)
                    {
                        if (focused)
                        {
                            focusedView[temp] = false;
                            sharedHandler.SetFocus(temp, false);
                            temp.OnBlur();
                        }
                    }
                    else if (lastFocused == false || temp.Meta.Focus == EventType.Static)
                    {
                        if (!focused)
                        {
                            focusedView[temp] = true;
                            sharedHandler.SetFocus(temp, true);
                            temp.OnFocus();
                        }
                        lastFocused = true;
                    }
                    else
                    {
                        if (focused)
                        {
                            focusedView[temp] = false;
                            sharedHandler.SetFocus(temp, false);
                            temp.OnBlur();
                        }
                    }
                }
                index--;
            }
        }

        /// <summary>
        /// 设置界面焦点。
        /// </summary>
        /// <param name="meta">界面元数据</param>
        public static void Focus(IMeta meta)
        {
            if (meta != null)
            {
                for (var i = 0; i < openedView.Count; i++)
                {
                    var temp = openedView[i];
                    if (temp.Meta.Path == meta.Path)
                    {
                        sharedHandler.SetFocus(temp, true);
                        temp.OnFocus();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 设置界面焦点。
        /// </summary>
        /// <param name="view">界面实例</param>
        public static void Focus(IBase view)
        {
            if (view != null)
            {
                for (var i = 0; i < openedView.Count; i++)
                {
                    var temp = openedView[i];
                    if (temp == view)
                    {
                        sharedHandler.SetFocus(temp, true);
                        temp.OnFocus();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 恢复界面焦点。
        /// </summary>
        public static void Resume() { Sort(null, null, null); }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="meta">界面元数据</param>
        /// <param name="resume">是否恢复焦点</param>
        public static void Close(IMeta meta, bool resume = true)
        {
            if (meta != null)
            {
                for (var i = openedView.Count - 1; i >= 0; i--)
                {
                    var temp = openedView[i];
                    if (temp.Meta.Path == meta.Path)
                    {
                        openedView.RemoveAt(i);
                        if (temp.Meta.Cache == CacheType.Shared || temp.Meta.Cache == CacheType.Scene)
                        {
                            cachedView.Add(temp);
                            if (temp.Meta.Cache == CacheType.Shared) UnityEngine.Object.DontDestroyOnLoad(temp.Panel);
                        }
                        var post = new Action(() =>
                        {
                            focusedView.Remove(temp);
                            temp.Panel.SetActiveState(false);
                            if (temp.Meta.Cache == CacheType.None)
                            {
                                temp.Panel.DestroyGO(true);
                            }
                        });
                        temp.OnClose(post);
                        break;
                    }
                }
                if (resume) Resume();
            }
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="view">界面实例</param>
        /// <param name="resume">是否恢复焦点</param>
        public static void Close(IBase view, bool resume = true)
        {
            if (view != null)
            {
                for (var i = openedView.Count - 1; i >= 0; i--)
                {
                    var temp = openedView[i];
                    if (temp == view)
                    {
                        openedView.RemoveAt(i);
                        if (temp.Meta.Cache == CacheType.Shared || temp.Meta.Cache == CacheType.Scene)
                        {
                            cachedView.Add(temp);
                            if (temp.Meta.Cache == CacheType.Shared) UnityEngine.Object.DontDestroyOnLoad(temp.Panel);
                        }
                        var post = new Action(() =>
                        {
                            focusedView.Remove(temp);
                            temp.Panel.SetActiveState(false);
                            if (temp.Meta.Cache == CacheType.None)
                            {
                                temp.Panel.DestroyGO(true);
                            }
                        });
                        temp.OnClose(post);
                        break;
                    }
                }
                if (resume) Resume();
            }
        }

        /// <summary>
        /// 关闭所有界面。
        /// </summary>
        /// <param name="exclude">排除的界面</param>
        public static void CloseAll(params IMeta[] exclude)
        {
            var index = 0;
            while (index < openedView.Count)
            {
                var view = openedView[index];
                var close = true;
                for (var i = 0; i < exclude.Length; i++)
                {
                    if (view.Meta.Path == exclude[i].Path)
                    {
                        close = false;
                        break;
                    }
                }
                if (close) Close(view.Meta, false);
                else index++;
            }
            Resume();
        }
    }
}
