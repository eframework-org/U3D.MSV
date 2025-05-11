# XModule

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.MSV)

XModule 提供了业务开发的基础模块，支持模块的生命周期管理和事件系统集成。

## 功能特性

- 生命周期管理：提供了Awake、Start、Reset、Stop状态控制
- 事件系统集成：通过事件系统对模块内（间）的业务进行解耦合

## 使用手册

### 1. 创建模块

1. 基础模块
    ```csharp
    // 基础模块
    public class MyModule : XModule.Base
    {
        public override string Name => "MyModule";
    }
    ```

2. 单例模块
    ```csharp
    // 单例模块
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

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
