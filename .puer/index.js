// Copyright (c) 2025 EFramework Organization. All rights reserved.
// Use of this source code is governed by a MIT-style
// license that can be found in the LICENSE file.

const XObject = CS.EFramework.Utility.XObject
const XString = CS.EFramework.Utility.XString
const XLog = CS.EFramework.Utility.XLog
const XEvent = CS.EFramework.Utility.XEvent

//#region XModule
const XModuleEvent = "__xmodule_event"
const XObjectThis = "__xobject_this"

class XModuleBase {
    constructor() {
        const othis = this.constructor.prototype[XObjectThis]
        if (othis) {
            for (let i = 0; i < othis.length; i++) {
                let key = othis[i]
                let value = this[key]
                if (value && typeof (value) == "function") {
                    this[key] = value.bind(this)
                }
            }
        }
    }

    static get Instance() {
        if (this.instance == null) {
            this.instance = new this()
            this.instance.Awake()
        }
        return this.instance
    }

    get Name() { return this.constructor.name }

    set Enabled(value) { this.enabled = value }

    get Enabled() { return this.enabled == null ? false : this.enabled }

    get Event() {
        if (this.event == null) this.event = new XEvent.Manager()
        return this.event
    }

    set Tags(value) { this.tags = value }

    get Tags() {
        if (this.tags == null) {
            this.tags = XLog.GetTag()
            this.tags.Set("Name", this.Name)
            this.tags.Set("Hash", XObject.HashCode(this))
        }
        return this.tags
    }

    Awake() {
        XLog.Notice("Module has been awaked.", this.Tags);
    }

    Start(...args) {
        const evts = this.constructor.prototype[XModuleEvent]
        if (evts) {
            for (let field in evts) {
                let meta = evts[field]
                let eid = meta.eid
                let module = meta.module
                let once = meta.once
                if (typeof (eid) != "number") {
                    XLog.Error("XModule.Event: invalid eid: {0} for callback: {1}", this.Tags, eid, field)
                    continue
                }
                let callback = this[field]
                if (!callback) {
                    XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of nil callback.", this.Tags, eid, field)
                    continue
                }
                if (typeof (callback) != "function") {
                    XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of non-function callback.", this.Tags, eid, field)
                    continue
                }
                if (module) {
                    if (module.Instance == null) {
                        XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of nil module instance.", this.Tags, eid, field)
                        continue
                    }
                    if (module.Instance.Event == null) {
                        XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of nil event instance.", this.Tags, eid, field)
                        continue
                    }
                }
                callback = callback.bind(this)
                this[field] = callback
                if (module) {
                    module.Instance.Event.Reg(eid, callback, null, once)
                } else {
                    this.Event.Reg(eid, callback, null, once)
                }
            }
        }

        this.Enabled = true
        XLog.Notice("Module has been started.", this.Tags);
    }

    Reset() {
        XLog.Notice("Module has been reseted.", this.Tags);
    }

    Stop() {
        this.Enabled = false
        if (this.Event) this.Event.Clear()
        this.Reset()
        XLog.Notice("Module has been stopped.", this.Tags);
    }
}

CS.EFramework.Modulize.XModule.Base = XModuleBase
CS.EFramework.Modulize.XModule.Base$1 = XModuleBase

CS.EFramework.Modulize.XModule.Event = function (eid, module, once = false) {
    return function (target, propertyKey) {
        target[XModuleEvent] = target[XModuleEvent] || {}
        target[XModuleEvent][propertyKey] = { eid: eid, module: module, once: once }
    }
}
//#endregion

//#region XScene
class XSceneBase extends XModuleBase { }

CS.EFramework.Modulize.XScene.Base = XModuleBase
CS.EFramework.Modulize.XScene.Base$1 = XModuleBase
//#endregion

//#region XView
const XView = CS.EFramework.Modulize.XView
const XViewElement = "__xview_element"
const XViewModule = "__xview_module"

class XViewEvent extends XEvent.Manager {
    constructor(context = null) {
        super()
        this.context = context || this
        this.proxies = new Map()
    }

    Reg(eid, callback, manager = null, once = false) {
        if (!callback) return false
        manager = manager || this.context

        const ret = manager.Reg(eid, callback, once)
        if (ret) {
            if (!this.proxies.has(eid)) {
                this.proxies.set(eid, [])
            }
            const proxy = {
                ID: XObject.HashCode(callback),
                Context: manager,
                Callback: callback
            }
            this.proxies.get(eid).push(proxy)
        }
        return ret
    }

    Unreg(eid, callback = null) {
        let ret = true
        const list = this.proxies.get(eid)
        if (list) {
            if (callback) {
                const hashCode = XObject.HashCode(callback)
                for (let i = list.length - 1; i >= 0; i--) {
                    const proxy = list[i]
                    if (proxy.ID === hashCode) {
                        ret &= proxy.Context.Unreg(eid, proxy.Callback)
                        list.splice(i, 1)
                    }
                }
            } else {
                for (const proxy of list) {
                    ret &= proxy.Context.Unreg(eid, proxy.Callback)
                }
                this.proxies.delete(eid)
            }
        }
        return ret
    }

    Notify(eid, ...managerOrArgs) {
        if (managerOrArgs.length > 0 && managerOrArgs[0] instanceof XEvent.Manager) {
            const args = managerOrArgs.slice(1)
            managerOrArgs[0].Notify(eid, ...args)
        } else {
            this.context.Notify(eid, ...managerOrArgs)
        }
    }

    Clear() {
        if (this.proxies) {
            this.proxies.forEach((list, eid) => {
                for (const proxy of list) {
                    proxy.Context.Unreg(eid, proxy.Callback)
                }
            })
            this.proxies.clear()
        }
        super.Clear()
    }
}

class XViewBase extends CS.UnityEngine.MonoBehaviour {
    constructor(proxy) {
        super(proxy)
        const othis = this.constructor.prototype[XObjectThis]
        if (othis) {
            for (let i = 0; i < othis.length; i++) {
                let key = othis[i]
                let value = this[key]
                if (value && typeof (value) == "function") {
                    this[key] = value.bind(this)
                }
            }
        }
    }

    get Meta() { return this.CProxy.Meta }

    get Panel() { return this.CProxy.Panel }

    get Module() {
        if (this.module == null && !this.bModule) {
            this.bModule = true
            const module = this.constructor[XViewModule]
            if (module) this.module = module.Instance
        }
        return this.module
    }

    get Event() {
        if (this.event == null) {
            this.event = new XViewEvent(this.Module?.Event)
        }
        return this.event
    }

    set Tags(value) { this.tags = value }

    get Tags() {
        if (this.tags == null) {
            this.tags = XLog.GetTag()
            this.tags.Set("Name", this.name)
            this.tags.Set("Comp", this.constructor.name)
            this.tags.Set("Hash", XObject.HashCode(this))
            if (this.Module) this.tags.Set("Module", this.Module.Name)
        }
        return this.tags
    }

    OnOpen(...args) { }

    OnFocus() { }

    OnBlur() { }

    OnClose(done) { if (done) done.Invoke() }

    Awake() {
        const eles = this.constructor.prototype[XViewElement]
        if (eles) {
            for (let field in eles) {
                let meta = eles[field]
                let name = meta.name
                let method = meta.method
                let ele = this.transform.Index(name)
                this[field] = ele
                if (!ele) {
                    XLog.Error("XView.Element: binding {0} for field: {1} failed because of nil node.", this.Tags, name, field)
                    continue
                }
                if (method) {
                    let callback = this[method]
                    if (!callback) {
                        XLog.Error("XView.Element: binding {0}'s event: {1} for field: {2} failed because of nil callback.", this.Tags, name, method, field)
                        continue
                    }
                    if (typeof (callback) != "function") {
                        XLog.Error("XView.Element: binding {0}'s event: {1} for field: {2} failed because of non-function callback.", this.Tags, name, method, field)
                        continue
                    }
                    callback = callback.bind(this)
                    this[method] = callback
                    let type = ele.GetType().FullName
                    if (type == "FairyGUI.GButton") ele.onClick.Add(callback)
                    else if (type == "FairyGUI.GList") ele.itemRenderer = callback
                    else if (type == "FairyGUI.GRichTextField") ele.onClickLink.Add(callback)
                    else if (type == "FairyGUI.GComboBox") ele.onChanged.Add(callback)
                    else XLog.Error("XView.Element: binding {0}'s event: {1} for field: {2} failed because of non-supported type: {3}.", this.Tags, name, method, field, type)
                }
            }
        }

        const evts = this.constructor.prototype[XModuleEvent]
        if (evts) {
            const ievts = new Array()
            for (let field in evts) {
                let meta = evts[field]
                let eid = meta.eid
                let module = meta.module
                let once = meta.once
                if (typeof (eid) != "number") {
                    XLog.Error("XModule.Event: invalid eid: {0} for callback: {1}", this.Tags, eid, field)
                    continue
                }
                let callback = this[field]
                if (!callback) {
                    XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of nil callback.", this.Tags, eid, field)
                    continue
                }
                if (typeof (callback) != "function") {
                    XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of non-function callback.", this.Tags, eid, field)
                    continue
                }
                if (module) {
                    if (module.Instance == null) {
                        XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of nil module instance.", this.Tags, eid, field)
                        continue
                    }
                    if (module.Instance.Event == null) {
                        XLog.Error("XModule.Event: binding eid-{0}'s callback: {1} failed because of nil event instance.", this.Tags, eid, field)
                        continue
                    }
                }
                callback = callback.bind(this)
                this[field] = callback
                ievts.push({ eid: eid, module: module, once: once, callback: callback })
            }
            this[XModuleEvent] = ievts
        }
    }

    OnEnable() {
        const evts = this[XModuleEvent]
        if (evts) {
            for (let i = 0; i < evts.length; i++) {
                let evt = evts[i]
                this.Event.Reg(evt.eid, evt.callback, evt.once)
            }
        }
    }

    OnDisable() {
        if (this.event) {
            this.event.Clear()
        }
    }

    Focus() { XView.Focus(this) }

    Close(resume = true) { XView.Close(this, resume) }
}

CS.EFramework.Modulize.XView.Base = XViewBase
CS.EFramework.Modulize.XView.Base$1 = XViewBase

CS.EFramework.Modulize.XView.Module = function (type) {
    return function (target) { target[XViewModule] = type }
}

CS.EFramework.Modulize.XView.Element = function (name, method) {
    return function (target, propertyKey) {
        target[XViewElement] = target[XViewElement] || {}
        target[XViewElement][propertyKey] = { name: name, method: method }
    }
}

const Meta = CS.EFramework.Modulize.XView.Meta
const _Open = CS.EFramework.Modulize.XView.Open
const _OpenAsync = CS.EFramework.Modulize.XView.OpenAsync
const _Load = CS.EFramework.Modulize.XView.Load
const _Find = CS.EFramework.Modulize.XView.Find
const _Sort = CS.EFramework.Modulize.XView.Sort
const _Focus = CS.EFramework.Modulize.XView.Focus
const _Close = CS.EFramework.Modulize.XView.Close

CS.EFramework.Modulize.XView.Open = function (target, belowOrParent, above, parent, ...args) {
    if (belowOrParent instanceof Meta && above instanceof Meta) return _Open(target, belowOrParent, above, parent, ...args)?.JProxy
    else if (belowOrParent instanceof CS.UnityEngine.Transform) return _Open(target, belowOrParent, ...getFixArgs(2, arguments))?.JProxy
    else return _Open(target, ...getFixArgs(1, arguments))?.JProxy
}

CS.EFramework.Modulize.XView.OpenAsync = function (target, belowOrParentOrCB, aboveOrCB, parent, callback, ...args) {
    if (belowOrParentOrCB instanceof Meta && aboveOrCB instanceof Meta) {
        if (typeof callback === "function") return _OpenAsync(target, belowOrParentOrCB, aboveOrCB, parent, getFixCallback(callback), ...args)
        else return _OpenAsync(target, belowOrParentOrCB, aboveOrCB, parent, ...getFixArgs(4, arguments))
    }
    else if (belowOrParentOrCB instanceof CS.UnityEngine.Transform) {
        if (typeof aboveOrCB === "function") return _OpenAsync(target, belowOrParentOrCB, getFixCallback(aboveOrCB), ...getFixArgs(3, arguments))
        else return _OpenAsync(target, belowOrParentOrCB, ...getFixArgs(2, arguments))
    }
    else {
        if (typeof belowOrParentOrCB === "function") return _OpenAsync(target, getFixCallback(belowOrParentOrCB), ...getFixArgs(2, arguments))
        else return _OpenAsync(target, ...getFixArgs(1, arguments))
    }
}

CS.EFramework.Modulize.XView.Load = function (meta, parent, closeIfOpened) { return _Load(meta, parent, closeIfOpened)?.JProxy }

CS.EFramework.Modulize.XView.Find = function (meta) { return _Find(meta)?.JProxy }

CS.EFramework.Modulize.XView.Sort = function (window, below, above) { return _Sort(window.CProxy, below.CProxy, above.CProxy) }

CS.EFramework.Modulize.XView.Focus = function (meta) {
    if (meta instanceof Meta) return _Focus(meta)
    else return _Focus(meta.CProxy)
}

CS.EFramework.Modulize.XView.Close = function (meta, resume = true) {
    if (meta instanceof Meta) return _Close(meta, resume)
    else return _Close(meta.CProxy, resume)
}

function getFixArgs(index, args) {
    let fixArgs = []
    for (let i = index; i < args.length; i++) {
        let arg = args[i]
        if (arg !== undefined) fixArgs.push(arg)
        else break
    }
    return fixArgs
}

function getFixCallback(callback) {
    let fixcallback = (window) => {
        window = window?.JProxy
        callback(window)
    }
    return fixcallback
}
//#endregion
