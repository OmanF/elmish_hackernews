[<AutoOpen>]
module View

open Feliz
open Zanaptak.TypedCssClasses

type Blm = CssClasses<"https://cdn.jsdelivr.net/npm/bulma@0.9.0/css/bulma.min.css", Naming.PascalCase>
type Fa = CssClasses<"https://use.fontawesome.com/releases/v5.14.0/css/all.css", Naming.PascalCase>

let renderError (errorMsg: string) =
    Html.h1
        [ prop.style [ style.color.red ]
          prop.text errorMsg ]

let renderItem item =
    Html.div
        [ prop.key item.Id
          prop.className Blm.Box
          prop.style
              [ style.marginTop 15
                style.marginBottom 15 ]
          prop.children
              [ match item.Url with
                | Some url ->
                    Html.a
                        [ prop.style [ style.textDecoration.underline ]
                          prop.target.blank
                          prop.href url
                          prop.text item.Title ]
                | None -> Html.p item.Title ] ]

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
