# XView

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  
[![DeepWiki](https://img.shields.io/badge/DeepWiki-Explore-blue)](https://deepwiki.com/eframework-org/U3D.MSV)

XView 提供了业务开发的基础视图，通过业务处理器（Handler）的模式实现了视图的管理功能。

## 功能特性

- 业务处理器：通过 handler 控制视图，实现了加载、显示、排序等功能
- 多插件适配：支持 Unity UI、FairyGUI、Next Gen GUI 等 UI 插件

## 使用手册

### 1. 创建视图

1. 基础视图
    ```csharp
    // 基础视图
    public class MyView : XView.Base { }
    ```

2. 视图描述
    ```csharp
    // 创建描述
    var meta = new XView.Meta(
        path: "Prefabs/MyView",           // 预制体路径
        fixedRQ: 0,                       // 固定渲染顺序
        focus: XView.EventType.Dynamic,   // 焦点类型
        cache: XView.CacheType.Scene,     // 缓存类型
        multiple: false                   // 是否支持多实例
    );
    ```

3. 事件系统
    ```csharp
    // 注册事件
    Event.Reg(eid, callback);
    Event.Reg<T1>(eid, callback);
    Event.Reg<T1, T2>(eid, callback);
    Event.Reg<T1, T2, T3>(eid, callback);
    ```

    ```csharp
    // 注销事件
    Event.Unreg(eid, callback);
    Event.Unreg<T1>(eid, callback);
    Event.Unreg<T1, T2>(eid, callback);
    Event.Unreg<T1, T2, T3>(eid, callback);
    ```

    ```csharp
    // 触发事件
    Event.Notify(eid, manager, args);
    ```

4. 属性标记
    ```csharp
    private class MyView : XView.Base
    {
        // UI绑定、UI事件绑定
        [XView.Element("@Login", "OnClickBtnLogin")]
        private Button m_BtnLogin;

        // 模块事件注册
        [XModule.Event(MyEvent.OnMyTest1, typeof(MyModule))]
        private void OnMyTest1()
        {
        }
    
        private void OnClickBtnLogin()
        {
        }
    }
    ```

### 2. 视图管理

1. 初始化
    ```csharp
    // 创建自定义Handler
    public class MyHandler : XView.IHandler 
    {
        public void Load(XView.IMeta meta, Transform parent, out XView.IBase view, out GameObject panel)
        {
            // 实现视图加载逻辑
        }

        public void LoadAsync(XView.IMeta meta, Transform parent, Action<XView.IBase, GameObject> callback)
        {
            // 实现异步加载逻辑
        }

        public bool Loading(XView.IMeta meta) { 
            // 是否正在加载视图
        }

        public void SetOrder(XView.IBase view, int order)
        {
            // 实现视图排序逻辑
        }

        public void SetFocus(XView.IBase view, bool focus)
        {
            // 实现焦点设置逻辑
        }
    }

    // 初始化视图系统
    var handler = new MyHandler();
    XView.Initialize(handler);
    ```

2. 打开视图
    ```csharp
    // 同步打开视图
    var view = XView.Open(meta, args);

    // 异步打开视图
    XView.OpenAsync(meta, callback, args);
    ```

3. 关闭视图
    ```csharp
    // 关闭指定视图
    XView.Close(meta, resume);

    // 关闭所有视图
    XView.CloseAll(exclude);
    ```

4. 视图排序
    ```csharp
    // 设置视图顺序
    XView.Sort(view, below, above);

    // 恢复默认顺序
    XView.Resume();
    ```

## 常见问题

### 1. 视图的 EventType（焦点类型）有什么区别？

1. Dynamic（动态）：当新视图打开时，之前的视图会自动失去焦点，适合需要独占用户输入的视图，如对话框。
2. Static（静态）：打开新视图不会影响此视图的焦点状态，适合需要保持交互的持久性视图，如操作面板。
3. Silent（静默）：视图不参与焦点系统，适合纯显示性质的视图，如提示信息或背景元素。

### 2. 视图的 CacheType（缓存类型）有什么区别？

1. Scene（场景级）：视图实例在场景切换时销毁，适合与当前场景关联的UI。
2. Shared（共享级）：视图实例在应用程序生命周期内保持，适合全局共享的UI，如主菜单。
3. None（无缓存）：视图在关闭时立即销毁，适合临时性强、内存占用大的UI。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)
