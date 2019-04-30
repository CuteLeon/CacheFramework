# xIR 缓存框架扩展方案

## 现行缓存框架：

​	目前系统内缓存功能通过 XPSaver(XPSaverCore) 和 XPCollectionAssist 实现。

- 保存缓存：
  - XPSaver.GetSingleton().SaveObject()
  - XPSaver.GetSingleton().SaveObject<>()
  - XPSaver.GetSingleton().SaveObjectWithTryFind<>()
  - ...
- 读取缓存：
  - XPCollectionAssist.GetSingleton().FindXPO<>()
  - XPCollectionAssist.GetSingleton().GetList<>()
  - XPCollectionAssist.GetSingleton().AttachT<>()
  - XPCollectionAssist.GetSingleton().DetachT<>()
  - XPCollectionAssist.GetSingleton().FindXPOForReloadFromDB<>()
  - ...



## 新缓存框架要点：

- 缓存加载时机：立即加载、懒加载；
- 缓存过期时机：手动释放、最后一次使用延时释放；
- 封装于现行框架：不改变现行框架的外观和调用方式；

### 方案：

​	在现行框架 XPSaver 和 XPCollectionAssist 方法内部调用懒加载缓存框架；