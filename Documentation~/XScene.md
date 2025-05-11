# XScene

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.MSV)

XScene 是一个基于模块框架设计的场景状态机，支持业务场景的切换及其生命周期管理。

## 功能特性

- 业务场景切换：通过状态机模式实现业务场景的切换功能
- 生命周期管理：提供了Start、Update、Stop等状态控制

## 使用手册

### 1. 创建场景

1. 基础场景
    ```csharp
    // 基础场景
    public class MyScene : XScene.Base
    {
        public override string Name => "MyScene";
    }
    ```

2. 单例场景
    ```csharp
    // 单例场景
    public class MySingletonScene : XScene.Base<MySingletonScene>
    {
        public override string Name => "MySingletonScene";
    }
    ```

### 2. 场景管理

1. 场景切换
    ```csharp
    // 获取代理实例
    XScene.OnProxy = proxy => new MyScene(proxy);

    // 切换业务场景
    XScene.Goto(scene, args);
    ```

2. 场景状态
    ```csharp
    // 获取当前场景
    var currentScene = XScene.Current;

    // 获取上一个场景
    var lastScene = XScene.Last;

    // 获取下一个场景
    var nextScene = XScene.Next;
    ```

3. 场景事件
    ```csharp
    // 注册场景切换事件
    XScene.OnSwap += () => {
        // 场景切换完成后的处理逻辑
    };
    ```

## 常见问题

### 1. XScene 与 Unity 场景系统有什么区别？

1. XScene 是业务场景管理框架，而 Unity 的 SceneManager 是渲染场景管理系统。
2. XScene 可以与 Unity 场景系统集成，实现业务场景与渲染场景的对应关系。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)
