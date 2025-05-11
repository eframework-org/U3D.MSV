// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using System.Collections.Generic;
using System.Reflection;
using EFramework.Utility;

namespace EFramework.Modulize
{
    /// <summary>
    /// XModule 提供了业务开发的基础模块，支持模块的生命周期管理和事件系统集成。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 生命周期管理：提供了 Awake、Start、Reset、Stop 状态控制
    /// - 事件系统集成：通过事件系统对模块内（间）的业务进行解耦合
    /// 
    /// 使用手册
    /// 1. 创建模块
    /// 
    /// 1.1 基础模块
    /// 
    ///     // 基础模块
    ///     public class MyModule : XModule.Base
    ///     {
    ///         public override string Name =&gt; "MyModule";
    ///     }
    /// 
    /// 1.2 单例模块
    /// 
    ///     // 单例模块
    ///     public class MySingletonModule : XModule.Base&lt;MySingletonModule&gt; 
    ///     {
    ///         public override string Name =&gt; "MySingletonModule";
    ///     }
    /// 
    /// 2. 模块管理
    /// 
    /// 2.1 模块状态
    /// 
    ///     // 获取模块名称
    ///     var moduleName = module.Name;
    /// 
    ///     // 控制模块启用状态
    ///     module.Enabled = true;
    /// 
    ///     // 获取模块日志标签
    ///     var tags = module.Tags;
    /// 
    /// 2.2 事件系统
    /// 
    ///     // 获取模块事件管理器
    ///     var eventManager = module.Event;
    /// 
    ///     // 注册事件
    ///     module.Event.Reg(eid, callback);
    /// 
    ///     // 注销事件
    ///     module.Event.Unreg(eid, callback);
    /// 
    ///     // 触发事件
    ///     module.Event.Notify(eid, args);
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public class XModule
    {
        /// <summary>
        /// 定义了模块的基础接口，包含模块的基本属性和生命周期方法。
        /// </summary>
        public interface IBase
        {
            /// <summary>
            /// 获取模块名称。
            /// </summary>
            string Name { get; }

            /// <summary>
            /// 获取或设置模块是否启用。
            /// </summary>
            bool Enabled { get; set; }

            /// <summary>
            /// 获取模块的事件管理器。
            /// </summary>
            XEvent.Manager Event { get; }

            /// <summary>
            /// 获取或设置模块的日志标签。
            /// </summary>
            XLog.LogTag Tags { get; set; }

            /// <summary>
            /// 模块初始化时调用。
            /// </summary>
            void Awake();

            /// <summary>
            /// 模块启动时调用。
            /// </summary>
            /// <param name="args">启动参数</param>
            void Start(params object[] args);

            /// <summary>
            /// 重置模块状态。
            /// </summary>
            void Reset();

            /// <summary>
            /// 停止模块运行。
            /// </summary>
            void Stop();
        }

        /// <summary>
        /// 提供了模块的基础实现，包含默认的生命周期管理和事件系统集成。
        /// </summary>
        public class Base : IBase
        {
            internal string name;

            /// <summary>
            /// 获取模块名称，如果未设置则返回类型名称。
            /// </summary>
            public virtual string Name
            {
                get
                {
                    name ??= GetType().Name;
                    return name;
                }
            }

            /// <summary>
            /// 获取或设置模块是否启用。
            /// </summary>
            public virtual bool Enabled { get; set; }

            internal XEvent.Manager @event;

            /// <summary>
            /// 获取模块的事件管理器，如果未初始化则创建新实例。
            /// </summary>
            public virtual XEvent.Manager Event
            {
                get
                {
                    @event ??= new XEvent.Manager();
                    return @event;
                }
            }

            internal XLog.LogTag tags;

            /// <summary>
            /// 获取或设置模块的日志标签，包含模块名称和哈希值。
            /// </summary>
            public virtual XLog.LogTag Tags
            {
                get
                {
                    tags ??= XLog.GetTag().Set("Name", Name).Set("Hash", GetHashCode().ToString());
                    return tags;
                }
                set { tags = value; }
            }

            /// <summary>
            /// 模块初始化时调用，记录日志。
            /// </summary>
            public virtual void Awake()
            {
                XLog.Notice("Module has been awaked.", Tags);
            }

            /// <summary>
            /// 模块启动时调用，设置启用状态并记录日志。
            /// </summary>
            /// <param name="args">启动参数</param>
            public virtual void Start(params object[] args)
            {
                Enabled = true;
                XLog.Notice("Module has been started.", Tags);
            }

            /// <summary>
            /// 重置模块状态，记录日志。
            /// </summary>
            public virtual void Reset()
            {
                XLog.Notice("Module has been reseted.", Tags);
            }

            /// <summary>
            /// 停止模块运行，清理事件并重置状态。
            /// </summary>
            public virtual void Stop()
            {
                Enabled = false;
                Event?.Clear();
                Reset();
                XLog.Notice("Module has been stopped.", Tags);
            }
        }

        /// <summary>
        /// 提供了模块的单例模式支持，自动管理模块实例的生命周期。
        /// </summary>
        /// <typeparam name="TModule">模块类型</typeparam>
        public class Base<TModule> : Base where TModule : IBase, new()
        {
            internal static TModule instance;

            /// <summary>
            /// 获取模块的单例实例，如果未创建则自动创建并初始化。
            /// </summary>
            public static TModule Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new TModule();
                        instance.Awake();
                    }

                    return instance;
                }
            }
        }

        /// <summary>
        /// 定义了事件的标记特性，使用此特性可以为事件定义参数。
        /// </summary>
        [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
        public sealed class Event : Attribute
        {
            /// <summary>
            /// 事件标识。
            /// </summary>
            public int ID { get; internal set; }

            /// <summary>
            /// 模块类型。
            /// </summary>
            public Type Module { get; internal set; }

            /// <summary>
            /// 单次回调。
            /// </summary>
            public bool Once { get; internal set; }

            /// <summary>
            /// 模块实例。
            /// </summary>
            public IBase Target { get; internal set; }

            /// <summary>
            /// 回调函数。
            /// </summary>
            public MethodInfo Callback { get; internal set; }

            public Event(object id, Type module = null, bool once = false)
            {
                ID = id == null ? -1 : id.GetHashCode();
                Module = module;
                Once = once;
            }

            /// <summary>
            /// 视图元素特性的全局缓存。
            /// </summary>
            internal static readonly Dictionary<Type, List<Event>> cached = new();

            /// <summary>
            /// 根据类型获取视图元素标记的特性。
            /// </summary>
            /// <param name="type">目标类型</param>
            /// <returns>标记的特性列表</returns>
            public static IReadOnlyList<Event> Get(Type type)
            {
                if (type == null) return null;

                if (!cached.TryGetValue(type, out var events))
                {
                    events = new List<Event>();
                    var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    foreach (var method in methods)
                    {
                        var attrs = method.GetCustomAttributes<Event>();
                        foreach (var attr in attrs)
                        {
                            if (attr.Module != null)
                            {
                                if (!typeof(IBase).IsAssignableFrom(attr.Module))
                                {
                                    XLog.Error("XModule.Event: module {0} does not implements {1}.", attr.Module, typeof(IBase));
                                    continue;
                                }
                                else
                                {
                                    var prop = attr.Module.GetProperty("Instance", BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                                    if (prop != null) attr.Target = prop.GetValue(null) as IBase;
                                    if (attr.Target == null)
                                    {
                                        XLog.Error("XModule.Event: unable to find instance of module {0}.", attr.Module);
                                        continue;
                                    }
                                }
                            }
                            attr.Callback = method;
                            events.Add(attr);
                        }
                    }
                    cached.Add(type, events);
                }

                return events;
            }
        }
    }
}
