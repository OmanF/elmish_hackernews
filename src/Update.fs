[<AutoOpen>]
module Update

open Elmish

let init () =
    { StoryItems = HasNotStartedYet
      CurrentStories = Stories.New },
    Cmd.ofMsg (LoadStoryItems Started)

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

    | LoadStoryItems (Finished (Ok storyIds)) ->
        let storiesMap =
            Map.ofList [ for id in storyIds -> id, InProgress ]

        let nextState =
            { state with
                  StoryItems = Resolved(Ok storiesMap) }

        nextState, Cmd.batch [ for id in storyIds -> Cmd.fromAsync (loadStoryItem id) ]

    | LoadStoryItems (Finished (Error error)) ->
        let nextState =
            { state with
                  StoryItems = Resolved(Error error) }

        nextState, Cmd.none

    | LoadedStoryItem (itemId, Ok item) ->
        match state.StoryItems with
        | Resolved (Ok storiesMap) ->
            let modifiedStoriesMap =
                storiesMap
                |> Map.remove itemId
                |> Map.add itemId (Resolved(Ok item))

            let nextState =
                { state with
                      StoryItems = Resolved(Ok modifiedStoriesMap) }

            nextState, Cmd.none

        | _ -> state, Cmd.none

    | LoadedStoryItem (itemId, Error error) ->
        match state.StoryItems with
        | Resolved (Ok storiesMap) ->
            let modifiedStoriesMap =
                storiesMap
                |> Map.remove itemId
                |> Map.add itemId (Resolved(Error error))

            let nextState =
                { state with
                      StoryItems = Resolved(Ok modifiedStoriesMap) }

            nextState, Cmd.none

        | _ -> state, Cmd.none
