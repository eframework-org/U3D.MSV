# XModule

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  

XModule 是模块管理基类，提供了模块的生命周期管理和事件系统集成。该模块支持模块的初始化、启动、重置和停止等操作，并提供了统一的日志和事件处理机制。

## 功能特性

- 支持模块生命周期管理
- 提供事件系统集成
- 支持模块状态控制
- 提供统一的日志标签
- 支持单例模式实现

## 使用手册

### 1. 基础模块

#### 1.1 创建模块
```csharp
// 创建基础模块类
public class MyModule : XModule.Base
{
    public override string Name => "MyModule";

    public override void Awake()
    {
        // 模块初始化逻辑
    }

    public override void Start(params object[] args)
    {
        // 模块启动逻辑
    }

    public override void Reset()
    {
        // 模块重置逻辑
    }

    public override void Stop()
    {
        // 模块停止逻辑
    }
}
```

#### 1.2 单例模块
```csharp
// 创建单例模块类
public class MySingletonModule : XModule.Base<MySingletonModule>
{
    // 使用 Instance 属性访问单例实例
    public static MySingletonModule Instance => XModule.Base<MySingletonModule>.Instance;
}
```

### 2. 模块管理

#### 2.1 模块状态
```csharp
// 获取模块名称
var moduleName = module.Name;

// 控制模块启用状态
module.Enabled = true;

// 获取模块日志标签
var tags = module.Tags;
```

#### 2.2 事件系统
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

#### 2.3 日志标签
```csharp
// 设置模块日志标签
module.Tags = XLog.GetTag()
    .Set("CustomTag", "Value");

// 使用模块日志标签记录日志
XLog.Notice("Module message", module.Tags);
```

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

### 1. 模块初始化失败
问题：模块的 Awake 方法没有被调用。
解决方案：
- 检查模块实例是否正确创建
- 确认模块继承关系是否正确
- 验证模块构造函数是否正常

### 2. 事件系统异常
问题：模块的事件没有被正确处理。
解决方案：
- 检查事件管理器是否正确初始化
- 确认事件注册和注销时机
- 验证事件参数是否正确传递

### 3. 日志记录问题
问题：模块的日志没有被正确记录。
解决方案：
- 检查日志标签是否正确设置
- 确认日志级别是否合适
- 验证日志系统是否正常工作

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
