[<AutoOpen>]
module Extensions

open Elmish

[<AutoOpen>]
module AsyncOperationsStatus =
    type Deferred<'t> =
        | HasNotStartedYet
        | InProgress
        | Resolved of 't

    type AsyncOperationStatus<'t> =
        | Started
        | Finished of 't

[<AutoOpen>]
module Async =
    let map f (computation: Async<'t>) =
        async {
            let! x = computation
            return f x
        }

[<AutoOpen>]
module Cmd =
    let fromAsync (operation: Async<'msg>) =
        let delayedCmd (dispatch: 'msg -> unit): unit =
            let delayedDispatch =
                async {
                    let! msg = operation
                    dispatch msg
                }

            Async.StartImmediate delayedDispatch

        Cmd.ofSub delayedCmd
