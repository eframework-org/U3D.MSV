// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using EFramework.Utility;

namespace EFramework.Modulize
{
    /// <summary>
    /// XModule 提供了游戏模块的基础框架，支持模块的生命周期管理和事件系统。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性:
    /// - 模块生命周期管理（Awake初始化、Start启动、Reset重置、Stop停止）
    /// - 事件系统集成，提供模块间通信能力
    /// - 日志标签系统，自动记录模块状态变化和关键操作
    /// - 单例模式支持，简化模块访问和管理
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
            public virtual string Name { get { name ??= GetType().Name; return name; } }

            /// <summary>
            /// 获取或设置模块是否启用。
            /// </summary>
            public virtual bool Enabled { get; set; }

            internal XEvent.Manager @event;
            /// <summary>
            /// 获取模块的事件管理器，如果未初始化则创建新实例。
            /// </summary>
            public virtual XEvent.Manager Event { get { @event ??= new XEvent.Manager(); return @event; } }

            internal XLog.LogTag tags;
            /// <summary>
            /// 获取或设置模块的日志标签，包含模块名称和哈希值。
            /// </summary>
            public virtual XLog.LogTag Tags { get { tags ??= XLog.GetTag().Set("Name", Name).Set("Hash", GetHashCode().ToString()); return tags; } set { tags = value; } }

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
    }
}
