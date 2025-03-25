# XScene

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  

XScene 提供了游戏场景的逻辑管理框架，及其生命周期管理和更新逻辑。

## 功能特性

- 场景生命周期管理（ Awake 初始化、Start 启动、Update 更新、Reset 重置、Stop 停止）
- 场景代理支持，可将任意对象(包括Unity场景)转换为场景接口

## 使用手册

### 1. 基础场景

1. 创建场景
```csharp
// 创建基础场景类
public class MyScene : XScene.Base
{
    public override string Name => "MyScene";
}
```

#### 1.2 单例场景
```csharp
// 创建单例场景类
public class MySingletonScene : XScene.Base<MySingletonScene>
{
    public override string Name => "MySingletonScene";
}
```

### 2. 场景管理

#### 2.1 场景切换
```csharp
// 切换逻辑场景
XScene.Goto(scene, args);

// 使用代理切换场景
XScene.OnProxy = scene => new MyScene();
XScene.Goto(sceneObject);
```

#### 2.2 场景状态
```csharp
// 获取当前场景
var currentScene = XScene.Current;

// 获取上一个场景
var lastScene = XScene.Last;

// 获取下一个场景
var nextScene = XScene.Next;
```

#### 2.3 场景事件
```csharp
// 注册场景切换事件
XScene.OnSwap += () => {
    // 场景切换完成后的处理逻辑
};
```

## 常见问题

### XScene与Unity场景系统有什么区别？
XScene 是逻辑场景管理框架，而 Unity 的SceneManager 是物理场景管理系统。XScene 通过 OnProxy 代理机制，可以与 Unity 场景系统集成，实现逻辑场景与物理场景的对应关系。

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)
