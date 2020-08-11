[<AutoOpen>]
module Extensions

open Elmish

[<AutoOpen>]
module AsyncOperationsOps =
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
    // `Cmd.fromAsync` is a hand-rolled implementation of `Cmd.OfAsync.result`
    // If anything, the use of `Async.StartImmediate`, is somewhat better than the built-in command as it allows, in theory, to include a cancellation token,
    // whereas the built-in command does not.
    // See here: https://elmish.github.io/elmish/cmd.html
    // Search the page for `module OfAsync`, view it's implementation of `result`, then checkout, on the same page, the implementation of `OfAsyncwith.result`.
    // Compare the implementation to the hand-rolled one here... pretty darn close!
    let fromAsync (operation: Async<'msg>) =
        let delayedCmd (dispatch: 'msg -> unit): unit =
            let delayedDispatch =
                async {
                    let! msg = operation
                    dispatch msg
                }

            Async.StartImmediate delayedDispatch

        Cmd.ofSub delayedCmd
