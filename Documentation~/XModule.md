# XModule

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  

XModule 提供了游戏模块的基础框架，支持模块的生命周期管理和事件系统。

## 功能特性

- 模块生命周期管理（Awake初始化、Start启动、Reset重置、Stop停止）
- 事件系统集成，提供模块间通信能力

## 使用手册

### 1. 基础模块

1. 创建模块
    ```csharp
    // 创建基础模块类
    public class MyModule : XModule.Base
    {
        public override string Name => "MyModule";
    }
    ```

2. 单例模块
    ```csharp
    // 创建单例模块类
    public class MySingletonModule : XModule.Base<MySingletonModule> 
    {
        public override string Name => "MySingletonModule";
    }
    ```

### 2. 模块管理

1. 模块状态
    ```csharp
    // 获取模块名称
    var moduleName = module.Name;

    // 控制模块启用状态
    module.Enabled = true;

    // 获取模块日志标签
    var tags = module.Tags;
    ```

2. 事件系统
    ```csharp
    // 获取模块事件管理器
    var eventManager = module.Event;

    // 注册事件
    module.Event.Reg(eid, callback);

    // 注销事件
    module.Event.Unreg(eid, callback);

    // 触发事件
    module.Event.Notify(eid, args);
    ```

## 常见问题

### 1. 模块的生命周期顺序是什么？
模块生命周期的典型顺序是：
1. 构造函数 - 创建模块实例
2. Awake() - 初始化模块基础设施
3. Start() - 启动模块功能
4. Reset() - 重置模块状态(可选)
5. Stop() - 停止模块并清理资源

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
