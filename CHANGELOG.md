# 更新记录

## [0.0.4] - 2025-05-13
### 变更
- 修改 puer 适配器的类型声明 index.d.ts 文件
- 修改 XView 模块事件的回调参数格式
- 更新 XModule、XScene、XView 模块的文档
- 更新依赖库版本

## [0.0.3] - 2025-05-12
## 变更
- 重构 XView 模块的事件实现及测试用例
- 修改 XView.IHandler 的接口，增加了 SetBinding 用于实现视图绑定逻辑
- 修改 XView.sharedHandler 为 Handler，支持从 puer 环境下进行调用
- 更新 XModule 模块文档
- 更新 XView 模块文档
- 更新依赖库版本

### 新增
- 新增 C# 版本的 XView.Element 特性实现
- 新增 C# 版本的 XModule.Event 特性实现
- 新增 [DeepWiki](https://deepwiki.com) 智能索引，方便开发者快速查找相关文档

### 修复
- 修复 puer 环境下 XView 适配层的事件注册错误

## [0.0.2] - 2025-03-31
### 变更
- 更新依赖库版本

### 新增
- 支持多引擎测试工作流

## [0.0.1] - 2025-03-26
### 新增
- 首次发布
