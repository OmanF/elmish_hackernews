module App

open Elmish
open Elmish.React

Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run
