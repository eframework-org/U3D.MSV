# XScene

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)  

XScene 是场景管理模块，提供了场景的生命周期管理和场景切换功能。该模块支持场景的初始化、更新、重置和停止等操作，并提供了灵活的场景切换机制。

## 功能特性

- 支持场景生命周期管理
- 提供场景切换功能
- 支持场景参数传递
- 提供场景状态追踪
- 支持场景代理机制

## 使用手册

### 1. 基础场景

#### 1.1 创建场景
```csharp
// 创建基础场景类
public class MyScene : XScene.Base
{
    public override string Name => "MyScene";

    public override void Awake()
    {
        // 场景初始化逻辑
    }

    public override void Start(params object[] args)
    {
        // 场景启动逻辑
    }

    public override void Update()
    {
        // 场景更新逻辑
    }

    public override void Reset()
    {
        // 场景重置逻辑
    }

    public override void Stop()
    {
        // 场景停止逻辑
    }
}
```

#### 1.2 单例场景
```csharp
// 创建单例场景类
public class MySingletonScene : XScene.Base<MySingletonScene>
{
    
}
```

### 2. 场景管理

#### 2.1 场景切换
```csharp
// 切换场景
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

更多问题，请查阅[问题反馈](../../CONTRIBUTING.md#问题反馈)。

### 1. 场景切换失败
问题：调用 Goto 方法后场景没有切换。
解决方案：
- 检查 OnProxy 是否正确设置
- 确认场景对象是否正确创建
- 验证场景参数是否正确传递

### 2. 场景状态异常
问题：Current、Last 或 Next 状态不符合预期。
解决方案：
- 检查场景切换流程
- 确认场景生命周期方法调用顺序
- 验证场景对象是否正确释放

### 3. 场景更新问题
问题：场景的 Update 方法没有被调用。
解决方案：
- 检查场景是否被正确激活
- 确认场景对象是否被正确创建
- 验证场景切换是否完成

## 项目信息

- [更新记录](../../CHANGELOG.md)
- [贡献指南](../../CONTRIBUTING.md)
- [许可证](../../LICENSE)
