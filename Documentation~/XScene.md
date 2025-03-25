# XScene

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  

XScene 提供了游戏场景的逻辑管理框架，支持场景的切换、生命周期管理和更新逻辑。

## 功能特性

- 场景生命周期管理（Awake初始化、Start启动、Update更新、Reset重置、Stop停止）
- 场景代理支持，可将任意对象(包括Unity场景)转换为场景接口
- 单例模式支持，简化场景访问和管理

## 使用手册

### 1. 基础场景

#### 1.1 创建场景
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
XScene是逻辑场景管理框架，而Unity的SceneManager是物理场景管理系统。XScene通过OnProxy代理机制，可以与Unity场景系统集成，实现逻辑场景与物理场景的对应关系。

### 场景的生命周期顺序是什么？
场景生命周期的典型顺序是：
1. 构造函数 - 创建场景实例
2. Awake() - 初始化场景基础设施
3. Start() - 启动场景功能
4. Update() - 更新场景状态
5. Reset() - 重置场景状态(场景切换时)
6. Stop() - 停止场景并清理资源(场景切换时)

### 如何在场景切换时传递参数？
可以通过Goto方法的可变参数实现：
```csharp
// 传递参数给下一个场景
XScene.Goto(new NextScene(), "param1", 42, playerData);

// 在目标场景的Start方法中接收参数
public override void Start(params object[] args)
{
    base.Start(args);
    
    if (args.Length > 0)
    {
        string param1 = args[0] as string;
        int param2 = (int)args[1];
        PlayerData data = args[2] as PlayerData;
    }
}
```

更多问题，请查阅[问题反馈](../CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](../CHANGELOG.md)
- [贡献指南](../CONTRIBUTING.md)
- [许可证](../LICENSE)
