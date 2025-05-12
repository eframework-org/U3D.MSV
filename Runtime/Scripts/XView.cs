// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using EFramework.Utility;
using UnityEngine.SceneManagement;

namespace EFramework.Modulize
{
    /// <summary>
/// XView 提供了业务开发的基础视图，通过适配器模式实现了视图的管理功能。
/// </summary>
/// <remarks>
/// <code>
/// 功能特性
/// - 业务适配器：通过适配器实例控制视图，实现了加载、绑定、显示、排序等功能
/// - 高可用拓展：提供了视图元素特性标记，支持 Unity UI、FairyGUI 等 UI 插件
/// 
/// 使用手册
/// 1. 定义视图
/// 
/// 1.1 基础视图
/// 
///     // 基础视图
///     public class MyView : XView.Base { }
/// 
///     // 模块视图
///     public class MyModulizedView : XView.Base&lt;MyModulizedView&gt; { }
/// 
/// 1.2 视图描述
/// 
///     // 创建描述
///     var meta = new XView.Meta(
///         path: "Prefabs/MyView",           // 预制体路径
///         fixedRQ: 0,                       // 固定渲染顺序
///         focus: XView.EventType.Dynamic,   // 焦点类型
///         cache: XView.CacheType.Scene,     // 缓存类型
///         multiple: false                   // 是否支持多实例
///     );
/// 
/// 1.3 事件系统
/// 
/// 基础示例：
/// 
///     // 注册事件
///     Event.Reg(eid, callback);
///     Event.Reg&lt;T1&gt;(eid, callback);
///     Event.Reg&lt;T1, T2&gt;(eid, callback);
///     Event.Reg&lt;T1, T2, T3&gt;(eid, callback);
/// 
///     // 注销事件
///     Event.Unreg(eid, callback);
///     Event.Unreg&lt;T1&gt;(eid, callback);
///     Event.Unreg&lt;T1, T2&gt;(eid, callback);
///     Event.Unreg&lt;T1, T2, T3&gt;(eid, callback);
/// 
///     // 通知事件
///     Event.Notify(eid, manager, args);
/// 
/// 标记示例：
/// 
/// - 事件标记会在视图 `OnEnable` 时自动注册，在视图 `OnDisable` 时自动注销
/// - 若未指定模块类型，则使用当前视图关联的模块
/// 
///     // 定义模块
///     public class MyModule : XModule.Base&lt;MyModule&gt; { }
/// 
///     // 定义事件
///     public enum MyEvent
///     {
///         Event1,
///         Event2,
///         Event3,
///     }
/// 
///     // 事件标记
///     public class MyView : XView.Base&lt;MyModule&gt; {
///         // 模块事件注册（指定模块类型）
///         [XModule.Event(MyEvent.Event1, typeof(MyModule))]
///         private void OnEvent1() { }
/// 
///         // 模块事件注册（单次触发）
///         [XModule.Event(MyEvent.Event2, typeof(MyModule), true)]
///         private void OnEvent2(params object[] args) { }
/// 
///         // 模块事件注册（当前视图关联的模块）
///         [XModule.Event(MyEvent.Event3)]
///         private void OnEvent3(int paramA, int paramB) { }
///     }
/// 
/// 1.4 视图特性
/// 
/// - 可用于标记视图元素的元数据，实现视图绑定和事件绑定
/// - 子类会继承父类的所有特性标记，可应用于类、字段和方法
/// - 视图特性会在视图 `Awake` 时回调 `Handler.SetBinding` 方法
/// - 特性标记只维护视图的元数据，业务层需要自行处理视图的绑定行为
/// 
/// 使用示例：
/// 
///     // 类特性标记
///     [XView.Element("类特性名称")]
///     [XView.Element("类特性名称带参数", "参数值")]
///     public class MyView : XView.Base
///     {
///         // 字段特性标记
///         [XView.Element("字段特性名称")]
///         private Button button;
/// 
///         // 字段特性标记带参数
///         [XView.Element("字段特性名称带参数", "参数值")]
///         private Text text;
/// 
///         // 方法特性标记
///         [XView.Element("方法特性名称")]
///         private void OnButtonClick() { }
/// 
///         // 方法特性标记带参数
///         [XView.Element("方法特性名称带参数", "参数值")]
///         private void OnTextChanged() { }
///     }
/// 
/// 2. 视图管理
/// 
/// 2.1 初始化
/// 
/// 使用示例：
/// 
///     // 创建自定义Handler
///     public class MyHandler : XView.IHandler 
///     {
///         public void Load(XView.IMeta meta, Transform parent, out XView.IBase view, out GameObject panel)
///         {
///             // 实现视图加载逻辑
///         }
/// 
///         public void LoadAsync(XView.IMeta meta, Transform parent, Action&lt;XView.IBase, GameObject&gt; callback)
///         {
///             // 实现异步加载逻辑
///         }
/// 
///         public bool Loading(XView.IMeta meta) { 
///             // 是否正在加载视图
///         }
///
///         public void SetBinding(GameObject go, object target, XView.Element[] elements)
///         {
///             // 实现视图绑定逻辑
///         }
/// 
///         public void SetOrder(XView.IBase view, int order)
///         {
///             // 实现视图排序逻辑
///         }
/// 
///         public void SetFocus(XView.IBase view, bool focus)
///         {
///             // 实现焦点设置逻辑
///         }
///     }
/// 
///     // 初始化视图系统
///     var handler = new MyHandler();
///     XView.Initialize(handler);
/// 
/// 2.2 打开视图
/// 
/// 使用示例：
/// 
///     // 同步打开视图
///     var view = XView.Open(meta, args);
/// 
///     // 异步打开视图
///     XView.OpenAsync(meta, callback, args);
/// 
/// 2.3 关闭视图
/// 
/// 使用示例：
/// 
///     // 关闭指定视图
///     XView.Close(meta, resume);
/// 
///     // 关闭所有视图
///     XView.CloseAll(exclude);
/// 
/// 2.4 视图排序
/// 
/// 使用示例：
/// 
///     // 设置视图顺序
///     XView.Sort(view, below, above);
/// 
///     // 恢复默认顺序
///     XView.Resume();
///
/// 常见问题
/// 1. 视图的 EventType（焦点类型）有什么区别？
/// 
///     1. Dynamic（动态）：当新视图打开时，之前的视图会自动失去焦点，适合需要独占用户输入的视图，如对话框。
///     2. Static（静态）：打开新视图不会影响此视图的焦点状态，适合需要保持交互的持久性视图，如操作面板。
///     3. Silent（静默）：视图不参与焦点系统，适合纯显示性质的视图，如提示信息或背景元素。
/// 
/// 2. 视图的 CacheType（缓存类型）有什么区别？
/// 
///     1. Scene（场景级）：视图实例在场景切换时销毁，适合与当前场景关联的UI。
///     2. Shared（共享级）：视图实例在应用程序生命周期内保持，适合全局共享的UI，如主菜单。
///     3. None（无缓存）：视图在关闭时立即销毁，适合临时性强、内存占用大的UI。
/// </code>
/// 更多信息请参考视图文档。
/// </remarks>

    #region 基础视图

    public partial class XView
    {
        /// <summary>
        /// 定义了视图的事件类型。
        /// </summary>
        public enum EventType
        {
            /// <summary>
            /// 动态事件，会触发视图的焦点变化。
            /// </summary>
            Dynamic,

            /// <summary>
            /// 静态事件，不会触发视图的焦点变化。
            /// </summary>
            Static,

            /// <summary>
            /// 静默事件，不会触发视图的焦点变化和事件通知。
            /// </summary>
            Slience,
        }

        /// <summary>
        /// 定义了视图的缓存类型。
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
        /// 定义了视图的描述接口，包含视图的基本配置信息。
        /// </summary>
        public interface IMeta
        {
            /// <summary>
            /// 获取视图的预制体路径。
            /// </summary>
            string Path { get; }

            /// <summary>
            /// 获取视图的固定渲染顺序。
            /// </summary>
            int FixedRQ { get; }

            /// <summary>
            /// 获取视图的事件类型。
            /// </summary>
            EventType Focus { get; }

            /// <summary>
            /// 获取视图的缓存类型。
            /// </summary>
            CacheType Cache { get; }

            /// <summary>
            /// 获取是否允许多个实例。
            /// </summary>
            bool Multiple { get; }
        }

        /// <summary>
        /// 提供了视图的描述实现。
        /// </summary>
        public class Meta : IMeta
        {
            /// <summary>
            /// 获取或设置视图的预制体路径。
            /// </summary>
            public string Path { get; set; }

            /// <summary>
            /// 获取或设置视图的固定渲染顺序。
            /// </summary>
            public int FixedRQ { get; set; }

            /// <summary>
            /// 获取或设置视图的事件类型。
            /// </summary>
            public EventType Focus { get; set; }

            /// <summary>
            /// 获取或设置视图的缓存类型。
            /// </summary>
            public CacheType Cache { get; set; }

            /// <summary>
            /// 获取或设置是否允许多个实例。
            /// </summary>
            public bool Multiple { get; set; }

            /// <summary>
            /// 初始化描述。
            /// </summary>
            public Meta() { }

            /// <summary>
            /// 使用指定参数初始化描述。
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
        /// 定义了视图的加载处理器接口。
        /// </summary>
        public interface IHandler
        {
            /// <summary>
            /// 同步加载视图。
            /// </summary>
            /// <param name="meta">视图描述</param>
            /// <param name="parent">父级变换</param>
            /// <param name="view">视图实例</param>
            /// <param name="panel">视图面板</param>
            void Load(IMeta meta, Transform parent, out IBase view, out GameObject panel);

            /// <summary>
            /// 异步加载视图。
            /// </summary>
            /// <param name="meta">视图描述</param>
            /// <param name="parent">父级变换</param>
            /// <param name="callback">加载完成回调</param>
            void LoadAsync(IMeta meta, Transform parent, Action<IBase, GameObject> callback);

            /// <summary>
            /// 检查视图是否正在加载。
            /// </summary>
            /// <param name="meta">视图描述</param>
            /// <returns>是否正在加载</returns>
            bool Loading(IMeta meta);

            /// <summary>
            /// 设置视图的元素绑定。
            /// </summary>
            /// <param name="go">游戏对象</param>
            /// <param name="target">组件实例</param>
            /// <param name="elements">元素列表</param>
            void SetBinding(GameObject go, object target, Element[] elements);

            /// <summary>
            /// 设置视图的渲染顺序。
            /// </summary>
            /// <param name="view">视图实例</param>
            /// <param name="order">渲染顺序</param>
            void SetOrder(IBase view, int order);

            /// <summary>
            /// 设置视图的焦点状态。
            /// </summary>
            /// <param name="view">视图实例</param>
            /// <param name="focus">是否获得焦点</param>
            void SetFocus(IBase view, bool focus);
        }

        /// <summary>
        /// 提供了视图的事件系统实现，支持事件注册、注销和通知。
        /// </summary>
        public class Event
        {
            /// <summary>
            /// 事件代理类，用于管理事件回调的上下文。
            /// </summary>
            internal class Proxy
            {
                public int ID;

                public XEvent.Manager Context;

                public XEvent.Callback Callback;
            }

            /// <summary>
            /// 事件上下文。
            /// </summary>
            internal readonly XEvent.Manager context;

            /// <summary>
            /// 事件代理容器。
            /// </summary>
            internal readonly Dictionary<int, List<Proxy>> proxies = new();

            public Event(XEvent.Manager context = null) { this.context = context ?? new XEvent.Manager(); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(int eid, XEvent.Callback callback, XEvent.Manager manager = null, bool once = false)
            {
                if (callback == null) return false;
                manager ??= context;

                var ret = manager.Reg(eid, callback, once);
                if (ret)
                {
                    if (!proxies.TryGetValue(eid, out var list))
                    {
                        list = new List<Proxy>();
                        proxies.Add(eid, list);
                    }

                    var proxy = new Proxy { ID = callback.GetHashCode(), Context = manager, Callback = callback };
                    list.Add(proxy);
                }

                return ret;
            }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(int eid, XEvent.Callback callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(Enum eid, XEvent.Callback callback, XEvent.Manager manager = null, bool once = false) { return Reg(eid.GetHashCode(), callback, manager, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg(Enum eid, XEvent.Callback callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1>(int eid, Action<T1> callback, XEvent.Manager manager = null, bool once = false)
            {
                if (callback == null) return false;
                manager ??= context;

                var ncallback = new XEvent.Callback(args =>
                {
                    var arg1 = args != null && args.Length > 0 ? (T1)args[0] : default;
                    callback?.Invoke(arg1);
                });

                var ret = manager.Reg(eid, ncallback, once);
                if (ret)
                {
                    if (!proxies.TryGetValue(eid, out var list))
                    {
                        list = new List<Proxy>();
                        proxies.Add(eid, list);
                    }

                    var proxy = new Proxy { ID = callback.GetHashCode(), Context = manager, Callback = ncallback };
                    list.Add(proxy);
                }

                return ret;
            }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1>(int eid, Action<T1> callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1>(Enum eid, Action<T1> callback, XEvent.Manager manager = null, bool once = false) { return Reg(eid.GetHashCode(), callback, manager, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1>(Enum eid, Action<T1> callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
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

                var ret = manager.Reg(eid, ncallback, once);
                if (ret)
                {
                    if (!proxies.TryGetValue(eid, out var list))
                    {
                        list = new List<Proxy>();
                        proxies.Add(eid, list);
                    }

                    var proxy = new Proxy { ID = callback.GetHashCode(), Context = manager, Callback = ncallback };
                    list.Add(proxy);
                }

                return ret;
            }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2>(int eid, Action<T1, T2> callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2>(Enum eid, Action<T1, T2> callback, XEvent.Manager manager = null, bool once = false) { return Reg(eid.GetHashCode(), callback, manager, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2>(Enum eid, Action<T1, T2> callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
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

                var ret = manager.Reg(eid, ncallback, once);
                if (ret)
                {
                    if (!proxies.TryGetValue(eid, out var list))
                    {
                        list = new List<Proxy>();
                        proxies.Add(eid, list);
                    }

                    var proxy = new Proxy { ID = callback.GetHashCode(), Context = manager, Callback = ncallback };
                    list.Add(proxy);
                }

                return ret;
            }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2, T3>(int eid, Action<T1, T2, T3> callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="manager">事件管理器</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2, T3>(Enum eid, Action<T1, T2, T3> callback, XEvent.Manager manager = null, bool once = false) { return Reg(eid.GetHashCode(), callback, manager, once); }

            /// <summary>
            /// 注册事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <param name="once">回调一次</param>
            /// <returns>是否成功</returns>
            public virtual bool Reg<T1, T2, T3>(Enum eid, Action<T1, T2, T3> callback, bool once = false) { return Reg(eid, callback, null, once); }

            /// <summary>
            /// 注销事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns> 
            public virtual bool Unreg(int eid, XEvent.Callback callback = null)
            {
                var ret = false;
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
                                ret |= proxy.Context.Unreg(eid, proxy.Callback);
                                list.RemoveAt(i);
                            }
                        }
                    }
                    else
                    {
                        foreach (var proxy in list)
                        {
                            ret |= proxy.Context.Unreg(eid, proxy.Callback);
                        }

                        proxies.Remove(eid);
                    }
                }

                return ret;
            }

            /// <summary>
            /// 注销事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns> 
            public virtual bool Unreg(Enum eid, XEvent.Callback callback = null) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 注销事件。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1>(int eid, Action<T1> callback)
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
            /// 注销事件。
            /// </summary>
            /// <typeparam name="T1">参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1>(Enum eid, Action<T1> callback) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 注销事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2>(int eid, Action<T1, T2> callback)
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
            /// 注销事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2>(Enum eid, Action<T1, T2> callback)
            {
                return Unreg(eid.GetHashCode(), callback);
            }

            /// <summary>
            /// 注销事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2, T3>(int eid, Action<T1, T2, T3> callback)
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
            /// 注销事件。
            /// </summary>
            /// <typeparam name="T1">第一个参数类型</typeparam>
            /// <typeparam name="T2">第二个参数类型</typeparam>
            /// <typeparam name="T3">第三个参数类型</typeparam>
            /// <param name="eid">事件标识</param>
            /// <param name="callback">事件回调</param>
            /// <returns>是否成功</returns>
            public virtual bool Unreg<T1, T2, T3>(Enum eid, Action<T1, T2, T3> callback) { return Unreg(eid.GetHashCode(), callback); }

            /// <summary>
            /// 清除事件注册。
            /// </summary>
            public virtual void Clear()
            {
                foreach (var kvp in proxies)
                {
                    foreach (var proxy in kvp.Value)
                    {
                        proxy.Context.Unreg(kvp.Key, proxy.Callback);
                    }
                }
                proxies.Clear();
            }

            /// <summary>
            /// 通知事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(int eid, params object[] args) { context.Notify(eid, args); }

            /// <summary>
            /// 通知事件。
            /// </summary>
            /// <param name="eid">事件标识</param>
            /// <param name="args">事件参数</param>
            public virtual void Notify(Enum eid, XEvent.Manager manager = null, params object[] args) { Notify(eid.GetHashCode(), args); }
        }

        /// <summary>
        /// 定义了视图的基础接口。
        /// </summary>
        public interface IBase
        {
            /// <summary>
            /// 获取或设置视图的描述。
            /// </summary>
            IMeta Meta { get; set; }

            /// <summary>
            /// 获取或设置视图的面板对象。
            /// </summary>
            GameObject Panel { get; set; }

            /// <summary>
            /// 视图打开时调用。
            /// </summary>
            /// <param name="args">打开参数</param>
            void OnOpen(params object[] args);

            /// <summary>
            /// 视图获得焦点时调用。
            /// </summary>
            void OnFocus();

            /// <summary>
            /// 视图失去焦点时调用。
            /// </summary>
            void OnBlur();

            /// <summary>
            /// 视图关闭时调用。
            /// </summary>
            /// <param name="done">关闭完成回调</param>
            void OnClose(Action done);
        }

        /// <summary>
        /// 视图元素特性，支持自定义描述。
        /// </summary>
        [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
        public sealed class Element : Attribute
        {
            /// <summary>
            /// 元素名称。
            /// </summary>
            public string Name { get; internal set; }

            /// <summary>
            /// 自定义描述。
            /// </summary>
            public string Extras { get; internal set; }

            /// <summary>
            /// 反射信息。
            /// </summary>
            public object Reflect { get; internal set; }

            public Element(string name = null, string extras = null, object reflect = null)
            {
                Name = name;
                Extras = extras;
                Reflect = reflect;
            }

            /// <summary>
            /// 视图元素特性的全局缓存。
            /// </summary>
            internal static readonly Dictionary<Type, Element[]> cached = new();

            /// <summary>
            /// 根据类型获取视图元素标记的特性。
            /// </summary>
            /// <param name="type">目标类型</param>
            /// <returns>标记的特性列表</returns>
            public static Element[] Get(Type type)
            {
                if (type == null) return null;

                if (!cached.TryGetValue(type, out var elements))
                {
                    var telements = new List<Element>();

                    var classAttrs = type.GetCustomAttributes<Element>();
                    foreach (var attr in classAttrs)
                    {
                        attr.Reflect = type;
                        telements.Add(attr);
                    }

                    var members = type.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var member in members)
                    {
                        var memberAttrs = member.GetCustomAttributes<Element>();
                        foreach (var attr in memberAttrs)
                        {
                            attr.Reflect = member;
                            telements.Add(attr);
                        }
                    }

                    foreach (var element in telements)
                    {
                        if (string.IsNullOrEmpty(element.Name) && element.Reflect != null)
                        {
                            element.Name = (element.Reflect as MemberInfo).Name;
                        }
                    }

                    elements = telements.ToArray();
                    cached.Add(type, elements);
                }

                return elements;
            }
        }
    }

    public partial class XView
    {
        /// <summary>
        /// 提供了视图的基础实现。
        /// </summary>
        public class Base : MonoBehaviour, IBase
        {
            /// <summary>
            /// 获取或设置视图的描述。
            /// </summary>
            public virtual IMeta Meta { get; set; }

            /// <summary>
            /// 获取或设置视图的面板对象。
            /// </summary>
            public virtual GameObject Panel { get; set; }

            internal Event @event;

            /// <summary>
            /// 获取视图的事件管理器。
            /// </summary>
            public virtual Event Event
            {
                get
                {
                    @event ??= new Event();
                    return @event;
                }
            }

            internal XLog.LogTag tags;

            /// <summary>
            /// 获取或设置视图的日志标签。
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
            /// 初始化视图。
            /// </summary>
            public Base() { }

            /// <summary>
            /// 使用代理初始化视图。
            /// </summary>
            /// <param name="proxy">代理对象</param>
            public Base(object proxy) { } // PuerTS 类型导出

            /// <summary>
            /// 视图初始化时调用。
            /// </summary>
            public virtual void Awake()
            {
                var elements = Element.Get(GetType());
                Handler.SetBinding(gameObject, this, elements);
            }

            /// <summary>
            /// 视图启用时调用。
            /// </summary>
            public virtual void OnEnable()
            {
                var events = XModule.Event.Get(GetType());
                if (events != null && events.Count > 0)
                {
                    foreach (var @event in events)
                    {
                        if (@event.Target != null)
                        {
                            Event.Reg(@event.ID, args =>
                            {
                                if (@event.Callback.IsStatic) @event.Callback.Invoke(null, args);
                                else @event.Callback.Invoke(this, args);
                            }, @event.Target.Event, @event.Once);
                        }
                        else
                        {
                            Event.Reg(@event.ID, args =>
                            {
                                if (@event.Callback.IsStatic) @event.Callback.Invoke(null, args);
                                else @event.Callback.Invoke(this, args);
                            }, @event.Once);
                        }
                    }
                }
            }

            /// <summary>
            /// 视图打开时调用。
            /// </summary>
            /// <param name="args">打开参数</param>
            public virtual void OnOpen(params object[] args) { }

            /// <summary>
            /// 视图获得焦点时调用。
            /// </summary>
            public virtual void OnFocus() { }

            /// <summary>
            /// 视图失去焦点时调用。
            /// </summary>
            public virtual void OnBlur() { }

            /// <summary>
            /// 视图禁用时调用。
            /// </summary>
            public virtual void OnDisable() { Event?.Clear(); }

            /// <summary>
            /// 视图关闭时调用。
            /// </summary>
            /// <param name="done">关闭完成回调</param>
            public virtual void OnClose(Action done) { done(); }

            /// <summary>
            /// 设置视图焦点。
            /// </summary>
            public virtual void Focus() { XView.Focus(this); }

            /// <summary>
            /// 关闭视图。
            /// </summary>
            /// <param name="resume">是否恢复焦点</param>
            public virtual void Close(bool resume = true) { XView.Close(this, resume); }
        }

        /// <summary>
        /// 提供了带模块的视图基础实现。
        /// </summary>
        /// <typeparam name="TModule">模块类型</typeparam>
        public class Base<TModule> : Base where TModule : XModule.IBase, new()
        {
            /// <summary>
            /// 获取视图的模块实例。
            /// </summary>
            public TModule Module { get => XModule.Base<TModule>.Instance; }

            /// <summary>
            /// 获取视图的事件管理器。
            /// </summary>
            public override Event Event
            {
                get
                {
                    @event ??= new Event(Module.Event);
                    return @event;
                }
            }

            /// <summary>
            /// 获取或设置视图的日志标签。
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

    #endregion

    #region 视图管理

    public partial class XView
    {
        /// <summary>
        /// 共享的视图加载处理器。
        /// </summary>
        public static IHandler Handler { get; internal set; }

        /// <summary>
        /// 缓存的视图列表。
        /// </summary>
        internal static List<IBase> cachedView = new();

        /// <summary>
        /// 打开的视图列表。
        /// </summary>
        internal static List<IBase> openedView = new();

        /// <summary>
        /// 获得焦点的视图字典。
        /// </summary>
        internal static readonly Dictionary<IBase, bool> focusedView = new();

        /// <summary>
        /// 初始化视图系统。
        /// </summary>
        /// <param name="handler">视图加载处理器</param>
        public static void Initialize(IHandler handler)
        {
            Handler = handler ?? throw new ArgumentNullException("handler");
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
        /// 加载视图。
        /// </summary>
        /// <param name="meta">视图描述</param>
        /// <param name="parent">父级变换</param>
        /// <param name="closeIfOpened">如果已打开则关闭</param>
        /// <returns>视图实例</returns>
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
                Handler.Load(meta, parent, out view, out var panel);
                if (view != null)
                {
                    view.Meta = meta;
                    view.Panel = panel;
                    Handler.SetFocus(view, true);
                }
            }

            return view;
        }

        /// <summary>
        /// 查找视图。
        /// </summary>
        /// <param name="meta">视图描述</param>
        /// <returns>视图实例</returns>
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
        /// 打开视图。
        /// </summary>
        /// <param name="target">目标视图</param>
        /// <param name="args">打开参数</param>
        /// <returns>视图实例</returns>
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
        /// 打开视图。
        /// </summary>
        /// <param name="target">目标视图</param>
        /// <param name="parent">父级变换</param>
        /// <param name="args">打开参数</param>
        /// <returns>视图实例</returns>
        public static IBase Open(IMeta target, Transform parent, params object[] args) { return Open(target, null, null, parent, args); }

        /// <summary>
        /// 打开视图。
        /// </summary>
        /// <param name="target">目标视图</param>
        /// <param name="below">下方视图</param>
        /// <param name="above">上方视图</param>
        /// <param name="parent">父级变换</param>
        /// <param name="args">打开参数</param>
        /// <returns>视图实例</returns>
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

        public static bool OpenAsync(IMeta target, Transform parent, Action<IBase> callback = null, params object[] args)
        {
            return OpenAsync(target, null, null, parent, callback, args);
        }

        /// <summary>
        /// 异步打开视图。
        /// </summary>
        /// <param name="target">目标视图</param>
        /// <param name="below">下方视图</param>
        /// <param name="above">上方视图</param>
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
                if (!target.Multiple && Handler.Loading(target))
                {
                    try { callback?.Invoke(null); }
                    catch (Exception e) { XLog.Panic(e); }

                    return false;
                }
                else
                {
                    Handler.LoadAsync(target, parent, (view, panel) =>
                    {
                        if (view != null)
                        {
                            view.Meta = target;
                            view.Panel = panel;
                            Handler.SetFocus(view, true);

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
        /// 排序视图。
        /// </summary>
        /// <param name="view">目标视图</param>
        /// <param name="below">下方视图</param>
        /// <param name="above">上方视图</param>
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
                        Handler.SetOrder(temp, temp.Meta.FixedRQ);
                    }
                    else
                    {
                        Handler.SetOrder(temp, 1000 + (rqIndex - 1) * 500);
                        rqIndex -= 1;
                    }

                    if (temp.Meta.Focus == EventType.Slience)
                    {
                        if (focused)
                        {
                            focusedView[temp] = false;
                            Handler.SetFocus(temp, false);
                            temp.OnBlur();
                        }
                    }
                    else if (lastFocused == false || temp.Meta.Focus == EventType.Static)
                    {
                        if (!focused)
                        {
                            focusedView[temp] = true;
                            Handler.SetFocus(temp, true);
                            temp.OnFocus();
                        }

                        lastFocused = true;
                    }
                    else
                    {
                        if (focused)
                        {
                            focusedView[temp] = false;
                            Handler.SetFocus(temp, false);
                            temp.OnBlur();
                        }
                    }
                }

                index--;
            }
        }

        /// <summary>
        /// 设置视图焦点。
        /// </summary>
        /// <param name="meta">视图描述</param>
        public static void Focus(IMeta meta)
        {
            if (meta != null)
            {
                for (var i = 0; i < openedView.Count; i++)
                {
                    var temp = openedView[i];
                    if (temp.Meta.Path == meta.Path)
                    {
                        Handler.SetFocus(temp, true);
                        temp.OnFocus();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 设置视图焦点。
        /// </summary>
        /// <param name="view">视图实例</param>
        public static void Focus(IBase view)
        {
            if (view != null)
            {
                for (var i = 0; i < openedView.Count; i++)
                {
                    var temp = openedView[i];
                    if (temp == view)
                    {
                        Handler.SetFocus(temp, true);
                        temp.OnFocus();
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 恢复视图焦点。
        /// </summary>
        public static void Resume() { Sort(null, null, null); }

        /// <summary>
        /// 关闭视图。
        /// </summary>
        /// <param name="meta">视图描述</param>
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
        /// 关闭视图。
        /// </summary>
        /// <param name="view">视图实例</param>
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
        /// 关闭所有视图。
        /// </summary>
        /// <param name="exclude">排除的视图</param>
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

    #endregion
}
