module App

open Elmish
open Elmish.React
open Feliz
open Zanaptak.TypedCssClasses

type Blm = CssClasses<"https://cdn.jsdelivr.net/npm/bulma@0.9.0/css/bulma.min.css", Naming.PascalCase>
type Fa = CssClasses<"https://use.fontawesome.com/releases/v5.14.0/css/all.css", Naming.PascalCase>

let init () =
    let initialState = { StoryItems = HasNotStartedYet }
    let initialCmd = Cmd.ofMsg (LoadStoryItems Started)
    initialState, initialCmd

let update (msg: Msg) (state: State) =
    match msg with
    | LoadStoryItems Started ->
        let nextState = { state with StoryItems = InProgress }
        nextState, Cmd.fromAsync loadStoryItems

    | LoadStoryItems (Finished (Ok storyItems)) ->
        let nextState =
            { state with
                  StoryItems = Resolved(Ok storyItems) }

        nextState, Cmd.none

    | LoadStoryItems (Finished (Error error)) ->
        let nextState =
            { state with
                  StoryItems = Resolved(Error error) }

        nextState, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    Html.div
        [ prop.style [ style.padding 20 ]
          prop.children
              [ Html.h1
                  [ prop.className Blm.Title
                    prop.text "Elmish Hackernews" ]

                renderItems state.StoryItems ] ]


Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run
