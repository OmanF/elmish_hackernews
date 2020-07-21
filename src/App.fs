module App

open Elmish
open Elmish.React
open Feliz
open Zanaptak.TypedCssClasses

type Blm = CssClasses<"https://cdn.jsdelivr.net/npm/bulma@0.9.0/css/bulma.min.css", Naming.PascalCase>
type Fa = CssClasses<"https://use.fontawesome.com/releases/v5.14.0/css/all.css", Naming.PascalCase>

let init () =
    let initialState =
        { StoryItems = HasNotStartedYet
          CurrentStories = Stories.New }

    let initialCmd = Cmd.ofMsg (LoadStoryItems Started)
    initialState, initialCmd

let update (msg: Msg) (state: State) =
    match msg with
    | ChangeStories stories ->
        let nextState =
            { state with
                  StoryItems = InProgress
                  CurrentStories = stories }

        let nextCmd = Cmd.fromAsync (loadStoryItems stories)

        nextState, nextCmd

    | LoadStoryItems Started ->
        let nextState = { state with StoryItems = InProgress }
        nextState, Cmd.fromAsync (loadStoryItems state.CurrentStories)

    | LoadStoryItems (Finished items) ->
        let nextState =
            { state with
                  StoryItems = Resolved items }

        nextState, Cmd.none

let render (state: State) (dispatch: Msg -> unit) =
    Html.div
        [ prop.style [ style.padding 20 ]
          prop.children
              [ Html.h1
                  [ prop.className Blm.Title
                    prop.text "Elmish Hackernews" ]

                renderTabs state.CurrentStories dispatch
                renderItems state.StoryItems ] ]


Program.mkProgram init update render
|> Program.withReactSynchronous "elmish-app"
|> Program.run
