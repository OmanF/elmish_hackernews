[<AutoOpen>]
module Model

type HackernewsItem =
    { Id: int
      Title: string
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
      StoryItems: Deferred<Result<HackernewsItem list, string>> }

type Msg =
    | ChangeStories of Stories
    | LoadStoryItems of AsyncOperationStatus<Result<HackernewsItem list, string>>
