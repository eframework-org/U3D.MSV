declare namespace CS {
    namespace EFramework.Modulize {
        namespace XModule {
            /**
             * 事件标记特性，使用此特性可以为事件定义参数。
             * @param eid 事件标识。
             * @param module 模块类型。
             * @param once 回调一次。
             */
            export function Event(eid: number, module: CS.System.Type = null, once: boolean = false): (target: any, propertyKey: string) => void
        }

        namespace XView {
            /**
             * 视图模块特性，用于绑定模块。
             * @param type 模块类型。
             */
            export function Module(type: CS.System.Type): (target: any) => void

            /**
             * 视图元素特性，支持自定义描述。
             * @param name 元素名称。
             * @param method 自定义描述。
             */
            export function Element(name: string, extras: string = null): (target: any, propertyKey: string) => void
        }
    }
}
