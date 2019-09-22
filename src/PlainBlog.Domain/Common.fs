namespace Domain

[<AutoOpen>]
module Common =

    [<AutoOpen>]
    module TitleModule =
        type Title = | X of string with
        member x.Value =
            match x with | X x -> x
        static member create title =
            if System.String.IsNullOrWhiteSpace(title) then
                Error "Title must not be empty."
            elif title.Length < 2 then
                Error "Title must be at least two chars long."
            else
                Ok (X title)

    [<AutoOpen>]
    module PublishDateModule =
        type PublishDate = | X of System.DateTime with
        member x.Value =
            match x with | X x -> x
        static member create date =
            X date
        static member parse (input: string) =
            match System.DateTime.TryParse input with
            | (true, v) -> Ok (PublishDate.create v)
            | (false, _) -> Error (sprintf "Date is not in the correct format: {%s}" input)

    [<AutoOpen>]
    module BodyModule =
        type Body = | X of string with
        member x.Value =
            match x with | X x -> x
        static member create body =
            if System.String.IsNullOrWhiteSpace(body) then
                Error "Body must not be empty."
            elif body.Length < 2 then
                Error "Body must be at least two chars long."
            else
                Ok (X body)

    [<AutoOpen>]
    module IntroModule =
        type Intro = | X of string with
        member x.Value =
            match x with | X x -> x
        static member create (body: Body) =
            let s = body.Value.Split([| "\r\n"; "\n" |], 2, System.StringSplitOptions.RemoveEmptyEntries)
            X s.[0]