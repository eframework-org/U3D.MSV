// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

using System;
using EFramework.Utility;

namespace EFramework.Modulize
{
    /// <summary>
    /// XScene 提供了游戏场景的逻辑管理框架，生命周期管理和更新逻辑。
    /// </summary>
    /// <remarks>
    /// <code>
    /// 功能特性
    /// - 场景生命周期管理（ Awake 初始化、Start 启动、Update 更新、Reset 重置、Stop 停止）
    /// - 场景代理支持，可将任意对象(包括Unity场景)转换为场景接口
    /// 
    /// 使用手册
    /// 1. 基础场景
    /// 
    ///     1. 创建场景
    /// 
    ///         // 创建基础场景类
    ///         public class MyScene : XScene.Base
    ///         {
    ///             public override string Name => "MyScene";
    ///         }
    /// 
    ///     2. 单例场景
    /// 
    ///         // 创建单例场景类
    ///         public class MySingletonScene : XScene.Base<MySingletonScene>
    ///         {
    ///             public override string Name => "MySingletonScene";
    ///         }
    /// 
    /// 2. 场景管理
    /// 
    ///     1. 场景切换
    /// 
    ///         // 切换逻辑场景
    ///         XScene.Goto(scene, args);
    /// 
    ///         // 使用代理切换场景
    ///         XScene.OnProxy = scene => new MyScene();
    ///         XScene.Goto(sceneObject);
    /// 
    ///     2. 场景状态
    /// 
    ///         // 获取当前场景
    ///         var currentScene = XScene.Current;
    /// 
    ///         // 获取上一个场景
    ///         var lastScene = XScene.Last;
    /// 
    ///         // 获取下一个场景
    ///         var nextScene = XScene.Next;
    /// 
    ///     3. 场景事件
    /// 
    ///         // 注册场景切换事件
    ///         XScene.OnSwap += () => {
    ///             // 场景切换完成后的处理逻辑
    ///         };
    /// 
    /// 常见问题
    /// 1. XScene 与 Unity 场景系统有什么区别？
    /// 
    /// XScene 是逻辑场景管理框架，而 Unity 的 SceneManager 是物理场景管理系统。XScene 通过 OnProxy 代理机制，可以与 Unity 场景系统集成，实现逻辑场景与物理场景的对应关系。
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
