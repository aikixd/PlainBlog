module Data 

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

type PostPublishDate = | PostPublishDate of System.DateTime


type PostText = | PostText of string
    
type PostData = 
    { title:       PostTitle 
      publishDate: PostPublishDate
      source:      PostText }
        
