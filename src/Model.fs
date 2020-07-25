[<AutoOpen>]
module Model

type HackernewsItem =
    { Id: int
      Title: string
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
      StoryItems: Deferred<Result<Map<int, Deferred<Result<HackernewsItem, string>>>, string>> }

type Msg =
    | LoadStoryItems of AsyncOperationStatus<Result<int list, string>>
    | LoadedStoryItem of int * Result<HackernewsItem, string>
    | ChangeStories of Stories
