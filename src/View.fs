[<AutoOpen>]
module View

open Feliz
open Zanaptak.TypedCssClasses

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

let renderTabs selectedStories dispatch =
    let switchStories stories =
        if selectedStories <> stories then dispatch (ChangeStories stories)

    Html.div
        [ prop.className
            [ Blm.Tabs
              Blm.IsToggle
              Blm.IsFullwidth ]
          prop.children
              [ Html.ul
                  [ for stories in storyCategories ->
                      Html.li
                          [ prop.classes [ if selectedStories = stories then Blm.IsActive ]
                            prop.onClick (fun _ -> switchStories stories)
                            prop.children [ Html.a [ Html.span (storiesName stories) ] ] ] ] ] ]


let renderError (errorMsg: string) =
    Html.h1
        [ prop.style [ style.color.red ]
          prop.text errorMsg ]

let renderItem item =
    Html.div
        [ prop.className Blm.Box
          prop.style
              [ style.marginTop 15
                style.marginBottom 15 ]
          prop.children
              [ Html.div
                  [ prop.className [ Blm.Columns; Blm.IsMobile ]
                    prop.children
                        [ Html.div
                            [ prop.className [ Blm.Column; Blm.IsNarrow ]
                              prop.children
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

                          Html.div
                              [ prop.className Blm.Column
                                prop.children
                                    [ match item.Url with
                                      | Some url ->
                                          Html.a
                                              [ prop.style [ style.textDecoration.underline ]
                                                prop.target.blank
                                                prop.href url
                                                prop.text item.Title ]

                                      | None -> Html.p item.Title ] ] ] ] ] ]

let spinner =
    Html.div
        [ prop.style
            [ style.textAlign.center
              style.marginTop 20 ]
          prop.children [ Html.i [ prop.className [ Fa.Fa; Fa.FaCog; Fa.FaSpin; Fa.Fa2X ] ] ] ]

let renderItems =
    function
    | HasNotStartedYet -> Html.none
    | InProgress -> spinner
    | Resolved (Error errorMsg) -> renderError errorMsg
    | Resolved (Ok items) -> React.fragment [ for item in items -> renderItem item ]
