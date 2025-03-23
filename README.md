# U3D.MSV

[![Version](https://img.shields.io/npm/v/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)
[![Downloads](https://img.shields.io/npm/dm/org.eframework.u3d.msv)](https://www.npmjs.com/package/org.eframework.u3d.msv)

U3D.MSV 是一个模块化开发框架，提供了标准的模块(Module)-场景(Scene)-视图(View)架构实现，支持模块管理、场景切换、视图控制等核心功能。

## 功能特性

- [XModule](Documentation~/XModule.md) 是模块管理基类，提供了模块的生命周期管理和事件系统集成。该模块支持模块的初始化、启动、重置和停止等操作，并提供了统一的日志和事件处理机制。
- [XScene](Documentation~/XScene.md) 是场景管理模块，提供了场景的生命周期管理和场景切换功能。该模块支持场景的初始化、更新、重置和停止等操作，并提供了灵活的场景切换机制。
- [XView](Documentation~/XView.md) 是视图管理模块，提供了视图的加载、显示、排序和事件管理等功能。该模块支持同步和异步加载，并提供了灵活的视图缓存和事件处理机制。

## 常见问题

更多问题，请查阅[问题反馈](CONTRIBUTING.md#问题反馈)。

## 项目信息

- [更新记录](CHANGELOG.md)
- [贡献指南](CONTRIBUTING.md)
- [许可证](LICENSE.md) 