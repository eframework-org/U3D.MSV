declare namespace CS {
    namespace EFramework.Modulize {
        namespace XModule {
            /**
             * 事件绑定器
             * @param eid 事件标识
             * @param module 模块类型
             * @param once 回调一次
             */
            export function Event(eid: number, module: CS.System.Type = null, once: boolean = false): (target: any, propertyKey: string) => void
        }

        namespace XView {
            /**
             * 模块绑定器
             * @param type 模块类型
             */
            export function Module(type: CS.System.Type): (target: any) => void

            /**
             * 组件绑定器
             * @param name 组件名称
             * @param method 函数名称
             */
            export function Element(name: string, method: string = null): (target: any, propertyKey: string) => void
        }
    }
}
