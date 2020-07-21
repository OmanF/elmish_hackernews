[<AutoOpen>]
module JsonHandle

open Fable.SimpleHttp
open Thoth.Json

let itemDecoder =
    Decode.object (fun fields ->
        { Id = fields.Required.At [ "id" ] Decode.int
          Title = fields.Required.At [ "title" ] Decode.string
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


let loadStoryItem (itemId: int) =
    async {
        let endpoint =
            sprintf "https://hacker-news.firebaseio.com/v0/item/%d.json" itemId

        let! (status, responseText) = Http.get endpoint

        match status with
        | 200 ->
            match Decode.fromString itemDecoder responseText with
            | Ok storyItem -> return Some storyItem
            | Error _ -> return None

        | _ -> return None
    }

let loadStoryItems stories =
    async {
        let endpoint = storiesEndpoint stories

        let! (status, responseText) = Http.get endpoint

        match status with
        | 200 ->
            let storyIds =
                Decode.fromString (Decode.list Decode.int) responseText

            match storyIds with
            | Ok storyIds ->
                let! storyItems =
                    storyIds
                    |> List.truncate 10
                    |> List.map loadStoryItem
                    |> Async.Parallel
                    |> Async.map (Array.choose id >> List.ofArray)

                return LoadStoryItems(Finished(Ok storyItems))

            | Error errorMsg -> return LoadStoryItems(Finished(Error errorMsg))
        | _ -> return LoadStoryItems(Finished(Error responseText))
    }
