[<AutoOpen>]
module Model

type HackernewsItem =
    { Id: int
      Title: string
      Url: string option }

type State =
    { StoryItems: Deferred<Result<HackernewsItem list, string>> }

type Msg = LoadStoryItems of AsyncOperationStatus<Result<HackernewsItem list, string>>
