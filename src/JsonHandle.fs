[<AutoOpen>]
module JsonHandle

open Fable.SimpleHttp
open Thoth.Json

let rnd = System.Random()

let itemDecoder =
    Decode.object (fun fields ->
        { Id = fields.Required.At [ "id" ] Decode.int
          Title = fields.Required.At [ "title" ] Decode.string
          Time = fields.Required.At [ "time" ] Decode.int
          ItemType = fields.Required.At [ "type" ] Decode.string
          Url = fields.Optional.At [ "url" ] Decode.string
          Score = fields.Required.At [ "score" ] Decode.int })

let storiesEndpoint stories =
    let fromBaseUrl =
        sprintf "https://hacker-news.firebaseio.com/v0/%sstories.json"

    match stories with
    | Stories.New -> fromBaseUrl "new"
    | Stories.Top -> fromBaseUrl "top"
    | Stories.Best -> fromBaseUrl "best"
    | Stories.Job -> fromBaseUrl "job"

let (|HttpOk|HttpError|) status =
    match status with
    | 200 -> HttpOk
    | _ -> HttpError

let loadStoryItem (itemId: int) =
    async {
        do! Async.Sleep(rnd.Next(100, 3000))

        let endpoint =
            sprintf "https://hacker-news.firebaseio.com/v0/item/%d.json" itemId

        let! (status, responseText) = Http.get endpoint

        match status with
        | HttpOk ->
            match Decode.fromString itemDecoder responseText with
            | Ok storyItem -> return LoadedStoryItem(itemId, Ok storyItem)
            | Error parseError -> return LoadedStoryItem(itemId, Error parseError)

        | HttpError ->
            return LoadedStoryItem
                       (itemId,
                        Error
                            ("HTTP error while loading response "
                             + string itemId))
    }

let loadStoryItems stories =
    async {
        let endpoint = storiesEndpoint stories
        let! (status, responseText) = Http.get endpoint

        match status with
        | HttpOk ->
            let storyIds =
                Decode.fromString (Decode.list Decode.int) responseText

            match storyIds with
            | Ok storyIds -> return LoadStoryItems(Finished(Ok storyIds))

            | Error errorMsg -> return LoadStoryItems(Finished(Error errorMsg))

        | HttpError -> return LoadStoryItems(Finished(Error "Could not load story."))
    }
