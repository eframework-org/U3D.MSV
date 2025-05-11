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

3. 事件特性
    ```csharp
    // 定义模块
    public class MyModule : XModule.Base<MyModule> { }

    // 定义事件
    public enum MyEvent
    {
        Event1,
        Event2,
        Event3,
    }

    // 标记事件
    public class MyListener {
        // 在类中使用事件特性标记方法
        [XModule.Event(MyEvent.Event1, typeof(MyModule), false)]
        public void OnEvent1() { }

        // 支持单次回调的事件（通知一次后自动注销）
        [XModule.Event(MyEvent.Event2, typeof(MyModule), true)]
        public void OnEvent2(params object[] args) { }

        // 支持无模块绑定的事件
        [XModule.Event(MyEvent.Event3, null, false)]
        public void OnEvent3(int param1, bool param2) { }

        // 支持静态方法
        [XModule.Event(MyEvent.Event3, typeof(MyModule), true)]
        public static void OnEvent3Static() { }
    }

    // 获取类型中标记的所有事件特性
    var events = XModule.Event.Get(typeof(MyListener));

    // TODO: 根据标记的事件特性注册事件
    ```

事件特性支持以下功能：
  - 自动获取指定模块的单例
  - 支持单次触发模式（Once=true）
  - 支持多种方法签名（无参、有参、返回值）
  - 支持实例方法和静态方法
  - 支持继承类中的事件特性

注意：事件特性标记只维护事件的元数据，需要业务层自行控制注册/注销事件的行为。

## 常见问题

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE.md)
