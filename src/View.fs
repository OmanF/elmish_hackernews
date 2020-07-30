[<AutoOpen>]
module View

open Feliz
open Fable.DateFunctions
open Zanaptak.TypedCssClasses
open Fable.Core.JsInterop

type Blm = CssClasses<"https://cdn.jsdelivr.net/npm/bulma@0.9.0/css/bulma.min.css", Naming.PascalCase>
type Fa = CssClasses<"https://use.fontawesome.com/releases/v5.14.0/css/all.css", Naming.PascalCase>

let storiesName =
    function
    | Stories.New -> "New"
    | Stories.Best -> "Best"
    | Stories.Job -> "Job"
    | Stories.Top -> "Top"

let storyCategories =
    [ Stories.New
      Stories.Top
      Stories.Best
      Stories.Job ]

let timeDistanceInWords (item: HackernewsItem) =
    let formatOptions = createEmpty<IFormatDistanceOptions>
    formatOptions.includeSeconds <- true
    formatOptions.addSuffix <- true

    let epochPoint =
        System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)

    let itemTime =
        epochPoint.AddSeconds(item.Time).ToLocalTime()

    itemTime.FormatDistanceToNow(formatOptions)

let sortByTime (storyItems: List<int * Deferred<Result<HackernewsItem, string>>>) =
    storyItems
    |> List.sortWith (fun (_, defItem1) (_, defItem2) ->
        match defItem1, defItem2 with
        | Resolved (Ok itemA), Resolved (Ok itemB) -> if itemA.Time >= itemB.Time then -1 else 1
        | Resolved (Ok itemA), _ -> -1
        | _, Resolved (Ok itemB) -> 1
        | _, _ -> 1)

let div (classes: string list) (children: ReactElement list) =
    Html.div
        [ prop.className classes
          prop.children children ]

let spinner =
    Html.div
        [ prop.style
            [ style.textAlign.center
              style.marginTop 20 ]
          prop.children [ Html.i [ prop.className [ Fa.Fa; Fa.FaCog; Fa.FaSpin; Fa.Fa2X ] ] ] ]

let title =
    Html.h1
        [ prop.className Blm.Title
          prop.text "Elmish Hacker News" ]

let renderTab currentStories stories dispatch =
    Html.li
        [ prop.className [ if currentStories = stories then Blm.IsActive ]
          prop.onClick (fun _ -> if (currentStories <> stories) then dispatch (ChangeStories stories))
          prop.children [ Html.a [ Html.span (storiesName stories) ] ] ]

let renderTabs currentStories dispatch =
    Html.div
        [ prop.className
            [ Blm.Tabs
              Blm.IsToggle
              Blm.IsFullwidth ]
          prop.children [ Html.ul [ for story in storyCategories -> renderTab currentStories story dispatch ] ] ]

let renderError (errorMsg: string) =
    Html.h1
        [ prop.style [ style.color.red ]
          prop.text errorMsg ]

let renderItemContent (item: HackernewsItem) =
    Html.div
        [ div [ Blm.Columns; Blm.IsMobile ]
              [ div [ Blm.Column; Blm.IsNarrow ]
                    [ Html.div
                        [ prop.className [ Blm.Icon ]
                          prop.style [ style.marginLeft 20 ]
                          prop.children
                              [ Html.i [ prop.className [ Fa.Fa; Fa.FaPoll; Fa.Fa2X ] ]
                                Html.span
                                    [ prop.style
                                        [ style.marginLeft 10
                                          style.marginRight 10 ]
                                      prop.text item.Score ] ] ] ] ]

          div [ Blm.Column ]
              [ match item.Url with
                | Some url ->
                    Html.a
                        [ prop.style [ style.textDecoration.underline ]
                          prop.target.blank
                          prop.href url
                          prop.text item.Title ]

                | None -> Html.p item.Title ]

          div [] [ Html.span [ prop.text (timeDistanceInWords item) ] ] ]

let renderStoryItem (itemId: int) storyItem =
    let renderItem =
        match storyItem with
        | HasNotStartedYet -> Html.none
        | InProgress -> spinner
        | Resolved (Error error) -> renderError error
        | Resolved (Ok storyItem) -> renderItemContent storyItem

    Html.div
        [ prop.key itemId
          prop.className Blm.Box
          prop.style
              [ style.marginTop 15
                style.marginBottom 15 ]
          prop.children [ renderItem ] ]

let renderStories items =
    match items with
    | HasNotStartedYet -> Html.none
    | InProgress -> spinner
    | Resolved (Error error) -> renderError error
    | Resolved (Ok items) ->
        items
        |> Map.toList
        |> sortByTime
        |> List.map (fun (id, storyItem) -> renderStoryItem id storyItem)
        |> Html.div

let render state dispatch =
    Html.div
        [ prop.style [ style.padding 20 ]
          prop.children
              [ title
                renderTabs state.CurrentStories dispatch
                renderStories state.StoryItems ] ]
