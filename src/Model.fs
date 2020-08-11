[<AutoOpen>]
module Model

type HackernewsItem =
    { Id: int
      Title: string
      Time: int
      ItemType: string
      Url: string option
      Score: int }

[<RequireQualifiedAccess>]
type Stories =
    | New
    | Top
    | Best
    | Job

type State =
    { CurrentStories: Stories
      ProcessedBatches: Map<int, Deferred<Result<HackernewsItem, string>>>
      RemainingBatches: List<List<int>>
      ContinueButtonState: Deferred<List<List<int>>>
      StoryItems: Deferred<Result<Map<int, Deferred<Result<HackernewsItem, string>>>, string>> }

type Msg =
    | LoadStoryItems of AsyncOperationStatus<Result<int list, string>>
    | LoadedStoryItem of int * Result<HackernewsItem, string>
    | ChangeStories of Stories
    | ContinueClicked of Deferred<List<List<int>>>
