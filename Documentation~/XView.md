# XView

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  

XView 是视图管理模块，提供了视图的加载、显示、排序和事件管理等功能。该模块支持同步和异步加载，并提供了灵活的视图缓存和事件处理机制。

## 功能特性

- 支持同步和异步视图加载
- 提供视图层级管理和排序功能
- 支持视图缓存机制（场景缓存、共享缓存）
- 提供灵活的事件系统
- 支持视图焦点管理
- 支持多实例视图

## 使用手册

### 1. 基础视图

#### 1.1 创建视图
```csharp
// 创建基础视图类
public class MyView : XView.Base
{
    public override void OnOpen(params object[] args)
    {
        // 视图打开时的初始化逻辑
    }

    public override void OnClose(Action done)
    {
        // 视图关闭时的清理逻辑
        done();
    }
}
```

#### 1.2 视图元数据
```csharp
// 创建视图元数据
var meta = new XView.Meta(
    path: "Prefabs/MyView",           // 视图预制体路径
    fixedRQ: 0,                       // 固定渲染顺序
    focus: XView.EventType.Dynamic,   // 焦点类型
    cache: XView.CacheType.Scene,     // 缓存类型
    multiple: false                   // 是否支持多实例
);
```

### 2. 视图管理

#### 2.1 打开视图
```csharp
// 同步打开视图
var view = XView.Open(meta, args);

// 异步打开视图
XView.OpenAsync(meta, callback, args);
```

#### 2.2 关闭视图
```csharp
// 关闭指定视图
XView.Close(meta, resume);

// 关闭所有视图
XView.CloseAll(exclude);
```

#### 2.3 视图排序
```csharp
// 设置视图顺序
XView.Sort(view, below, above);

// 恢复默认顺序
XView.Resume();
```

### 3. 事件系统

#### 3.1 注册事件
```csharp
// 注册事件回调
Event.Reg(eid, callback);
Event.Reg<T1>(eid, callback);
Event.Reg<T1, T2>(eid, callback);
Event.Reg<T1, T2, T3>(eid, callback);
```

#### 3.2 注销事件
```csharp
// 注销事件回调
Event.Unreg(eid, callback);
Event.Unreg<T1>(eid, callback);
Event.Unreg<T1, T2>(eid, callback);
Event.Unreg<T1, T2, T3>(eid, callback);
```

#### 3.3 触发事件
```csharp
// 触发事件
Event.Notify(eid, manager, args);
```

## 常见问题

更多问题，请查阅[问题反馈](../../CONTRIBUTING.md#问题反馈)。

### 1. 视图加载失败
问题：视图预制体加载失败，返回 null。
解决方案：
- 检查预制体路径是否正确
- 确认预制体是否已正确导入
- 验证资源加载系统是否正常工作

### 2. 事件回调未触发
问题：注册的事件回调没有被触发。
解决方案：
- 检查事件 ID 是否匹配
- 确认事件管理器是否正确初始化
- 验证事件触发时机是否正确

### 3. 视图排序异常
问题：视图显示顺序不符合预期。
解决方案：
- 检查视图的 FixedRQ 设置
- 确认 Sort 方法的参数顺序
- 验证视图层级关系是否正确

## 项目信息

- [更新记录](../../CHANGELOG.md)
- [贡献指南](../../CONTRIBUTING.md)
- [许可证](../../LICENSE)
