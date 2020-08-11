[<AutoOpen>]
module Update

open Elmish

let init () =
    { StoryItems = HasNotStartedYet
      ProcessedBatches = Map.empty
      RemainingBatches = []
      ContinueButtonState = HasNotStartedYet
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
        let allStoriesIds = List.splitInto 50 storyIds
        let workingBatch = allStoriesIds |> List.head
        let remainingBatches = allStoriesIds |> List.tail

        let storiesMap =
            Map.ofList [ for id in workingBatch -> id, InProgress ]

        let nextState =
            { state with
                  RemainingBatches = remainingBatches
                  StoryItems = Resolved(Ok storiesMap) }

        nextState, Cmd.batch [ for id in workingBatch -> Cmd.fromAsync (loadStoryItem id) ]

    | LoadStoryItems (Finished (Error error)) ->
        let nextState =
            { state with
                  StoryItems = Resolved(Error error) }

        nextState, Cmd.none

    | ContinueClicked (Resolved remainingBatches) ->
        match remainingBatches with
        | h :: t ->
            let storiesMap =
                Map.ofList [ for id in h -> id, InProgress ]

            let nextState =
                { state with
                      RemainingBatches = t
                      ContinueButtonState = InProgress
                      StoryItems = Resolved(Ok storiesMap) }

            nextState, Cmd.batch [ for id in h -> Cmd.fromAsync (loadStoryItem id) ]

        | [] ->
            { state with
                  ContinueButtonState = HasNotStartedYet },
            Cmd.none

    | ContinueClicked _ -> state, Cmd.none

    | LoadedStoryItem (itemId, Ok item) ->
        match state.StoryItems with
        | Resolved (Ok storiesMap) ->
            let modifiedStoriesMap =
                storiesMap
                |> Map.remove itemId
                |> Map.add itemId (Resolved(Ok item))
                |> Map.fold (fun acc key value -> Map.add key value acc) state.ProcessedBatches

            let nextState =
                { state with
                      ProcessedBatches = modifiedStoriesMap
                      ContinueButtonState = HasNotStartedYet
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
