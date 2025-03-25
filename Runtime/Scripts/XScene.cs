// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using EFramework.Utility;

namespace EFramework.Modulize
{
    /// <summary>
    /// XScene 提供了游戏场景的逻辑管理框架，支持场景的切换、生命周期管理和更新逻辑。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性:
    /// - 场景生命周期管理（Awake初始化、Start启动、Update更新、Reset重置、Stop停止）
    /// - 场景代理支持，可将任意对象(包括Unity场景)转换为场景接口
    /// - 单例模式支持，简化场景访问和管理
    /// </code>
    /// 更多信息请参考模块文档。
    /// </remarks>
    public partial class XScene
    {
        /// <summary>
        /// 定义了场景的基础接口，包含场景的基本属性和生命周期方法。
        /// </summary>
        public interface IBase
        {
            /// <summary>
            /// 获取场景名称。
            /// </summary>
            string Name { get; }

            /// <summary>
            /// 场景初始化时调用。
            /// </summary>
            void Awake();

            /// <summary>
            /// 场景启动时调用。
            /// </summary>
            /// <param name="args">启动参数</param>
            void Start(params object[] args);

            /// <summary>
            /// 场景每帧更新时调用。
            /// </summary>
            void Update();

            /// <summary>
            /// 重置场景状态。
            /// </summary>
            void Reset();

            /// <summary>
            /// 停止场景运行。
            /// </summary>
            void Stop();
        }

        /// <summary>
        /// 提供了场景的基础实现，继承自模块系统并实现场景接口。
        /// </summary>
        public class Base : XModule.Base, IBase
        {
            /// <summary>
            /// 场景每帧更新时的默认实现。
            /// </summary>
            public virtual void Update() { }
        }

        /// <summary>
        /// 提供了场景的单例模式支持，自动管理场景实例的生命周期。
        /// </summary>
        /// <typeparam name="TScene">场景类型</typeparam>
        public class Base<TScene> : Base where TScene : IBase, new()
        {
            internal static TScene instance;

            /// <summary>
            /// 获取场景的单例实例，如果未创建则自动创建并初始化。
            /// </summary>
            public static TScene Instance
            {
                get
                {
                    if (instance == null)
                    {
                        instance = new TScene();
                        instance.Awake();
                    }
                    return instance;
                }
            }
        }
    }

    /// <summary>
    /// 场景管理类，提供场景切换和管理的核心功能。
    /// </summary>
    public partial class XScene
    {
        /// <summary>
        /// 场景切换完成时触发的事件。
        /// </summary>
        public static event Action OnSwap;

        /// <summary>
        /// 场景代理函数，用于将场景对象转换为场景接口。
        /// </summary>
        public static Func<object, IBase> OnProxy;

        /// <summary>
        /// 获取上一个场景。
        /// </summary>
        public static IBase Last { get; internal set; }

        /// <summary>
        /// 获取当前场景。
        /// </summary>
        public static IBase Current { get; internal set; }

        /// <summary>
        /// 获取下一个场景。
        /// </summary>
        public static IBase Next { get; internal set; }

        internal static object[] Args;

        internal static bool Inited;

        /// <summary>
        /// 更新场景状态，处理场景切换逻辑。
        /// </summary>
        internal static void Update()
        {
            Current?.Update();
            if (Next != null)
            {
                Current?.Reset();
                Current?.Stop();
                Last = Current;
                Current = Next;
                Next = null;
                var args = Args;
                Args = null;
                Current?.Start(args);
                OnSwap?.Invoke();
            }
        }

        /// <summary>
        /// 切换到指定场景，使用场景代理进行转换。
        /// </summary>
        /// <param name="scene">目标场景对象</param>
        /// <param name="args">场景启动参数</param>
        /// <exception cref="Exception">当场景代理未设置时抛出异常</exception>
        public static void Goto(object scene, params object[] args)
        {
            if (OnProxy == null) throw new Exception("OnProxy is null");
            Goto(OnProxy.Invoke(scene), args);
        }

        /// <summary>
        /// 切换到指定场景。
        /// </summary>
        /// <param name="scene">目标场景</param>
        /// <param name="args">场景启动参数</param>
        public static void Goto(IBase scene, params object[] args)
        {
            Next = scene;
            Args = args;
            if (!Inited)
            {
                Inited = true;
                XLoom.SetInterval(Update, 0);
            }
        }
    }
}
