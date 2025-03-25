# XView

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  

XView 提供了UI视图的加载、显示、排序和事件管理等功能。

## 功能特性

- 视图生命周期管理：控制视图的加载、显示、隐藏和销毁等生命周期
- 焦点管理系统：支持动态、静态和静默三种事件类型，灵活控制界面焦点变化
- 缓存机制：支持场景级、共享级和无缓存三种缓存策略，优化资源使用
- 渲染排序：自动管理视图的渲染顺序，支持固定渲染顺序和动态排序

## 使用手册

### 1. 基础视图

1. 创建视图
    ```csharp
    // 创建基础视图类
    public class MyView : XView.Base { }
    ```

2. 视图元数据
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

1. 打开视图
    ```csharp
    // 同步打开视图
    var view = XView.Open(meta, args);

    // 异步打开视图
    XView.OpenAsync(meta, callback, args);
    ```

2. 关闭视图
    ```csharp
    // 关闭指定视图
    XView.Close(meta, resume);

    // 关闭所有视图
    XView.CloseAll(exclude);
    ```

3. 视图排序
    ```csharp
    // 设置视图顺序
    XView.Sort(view, below, above);

    // 恢复默认顺序
    XView.Resume();
    ```

### 3. 事件系统

1. 注册事件
    ```csharp
    // 注册事件回调
    Event.Reg(eid, callback);
    Event.Reg<T1>(eid, callback);
    Event.Reg<T1, T2>(eid, callback);
    Event.Reg<T1, T2, T3>(eid, callback);
    ```

2. 注销事件
    ```csharp
    // 注销事件回调
    Event.Unreg(eid, callback);
    Event.Unreg<T1>(eid, callback);
    Event.Unreg<T1, T2>(eid, callback);
    Event.Unreg<T1, T2, T3>(eid, callback);
    ```

3. 触发事件
    ```csharp
    // 触发事件
    Event.Notify(eid, manager, args);
    ```

## 常见问题

### 视图的EventType（焦点类型）有什么区别？
XView支持三种焦点类型：
- **Dynamic（动态）**：当新视图打开时，之前的视图会自动失去焦点。适合需要独占用户输入的视图，如对话框。
- **Static（静态）**：打开新视图不会影响此视图的焦点状态。适合需要保持交互的持久性视图，如操作面板。
- **Silent（静默）**：视图不参与焦点系统。适合纯显示性质的视图，如提示信息或背景元素。

### 如何选择合适的视图缓存类型？
根据视图的使用特性选择缓存类型：
- **Scene（场景级）**：视图实例在场景切换时销毁。适合与当前场景关联的UI。
- **Shared（共享级）**：视图实例在应用程序生命周期内保持。适合全局共享的UI，如主菜单。
- **None（无缓存）**：视图在关闭时立即销毁。适合临时性强、内存占用大的UI。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)
