namespace Domain

[<AutoOpen>]
module Data =

    [<AutoOpen>]
    module PostTitleModule =
        type PostTitle = | X of string with
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
    module PostPublishDateModule =
        type PostPublishDate = | X of System.DateTime with
        member x.Value =
            match x with | X x -> x
        static member create date =
            X date

    [<AutoOpen>]
    module PostBodyModule =
        type PostBody = | X of string with
        member x.Value =
            match x with | X x -> x
        static member create body =
            if System.String.IsNullOrWhiteSpace(body) then
                Error "Title must not be empty."
            elif body.Length < 2 then
                Error "Title must be at least two chars long."
            else
                Ok (X body)
    
    type PostData = 
        { title:       PostTitle 
          publishDate: PostPublishDate
          body:        PostBody }
        
