namespace Posts

module Load =
    
    open Domain
    open Parse
    open Load

    type private PostDataAggr =
        { title:       Title option
          publishDate: PublishDate option
          body:        Body option }
        with
        static member Empty source =
            match Body.create source with
            | Ok x -> Ok { title       = None 
                           publishDate = None
                           body        = Some x }
            | Error x -> Error [ x ]
    
        member x.Validate () =
               x.title      .IsSome
            && x.publishDate.IsSome
            && x.body       .IsSome
    
        member x.ToPost () : Result<Post, string> =
            match x.Validate () with
            | true -> Ok { title       = x.title.Value
                           publishDate = x.publishDate.Value
                           body        = x.body.Value
                           intro       = Intro.create x.body.Value }
            | false -> Error (sprintf "Not all properties are defined on post.")

    let private applyTitle (data: PostDataAggr) = 
        bindApply Title.create (fun x -> { data with title = Some x })

    let private applyPublishDate (data: PostDataAggr) =
        bindApply PublishDate.parse (fun x -> { data with publishDate = Some x })

    let private applyProp prop data =
        match prop.key with
        | "Title" -> applyTitle data prop.value
        | "PublishDate" -> applyPublishDate data prop.value
        | x -> Error (sprintf "Error: unknown property {%s}" x)

    let parsePost (parsed: Parse.ParseData) =
        let conv aggr =
            let (data, errors) = applyProps applyProp parsed.properties (aggr, [])
            match data.ToPost () with
            | Ok data' -> Ok { entity = data' 
                               errors = errors }
            | Error x -> Error ("Couldn't create post data" :: x :: errors)
        
        PostDataAggr.Empty parsed.body
        |> Result.bind conv