[<AutoOpen>]
module JsonHandle

open Fable.SimpleHttp
open Thoth.Json

let itemDecoder =
    Decode.object (fun fields ->
        { Id = fields.Required.At [ "id" ] Decode.int
          Title = fields.Required.At [ "title" ] Decode.string
          Url = fields.Optional.At [ "url" ] Decode.string })

let storiesEndpoint =
    "https://hacker-news.firebaseio.com/v0/topstories.json"

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

let loadStoryItems =
    async {
        let! (status, responseText) = Http.get storiesEndpoint

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
