namespace Posts

module Load =
    
    open Domain
    open Parse

    type PostLoadSuccessResult =
        { post: PostData
          errors: string list }

    type PostLoadFailResult =
        { errors: string list }

    type PostLoadResultData =
        | Success of PostLoadSuccessResult
        | Fail of PostLoadFailResult

    type PostLoadResult =
        { path: Path
          result: PostLoadResultData }

    type private PostDataAggr =
        { title:       PostTitle option
          publishDate: PostPublishDate option
          body:        PostBody option }
        with
        static member Empty source =
            match PostBody.create source with
            | Ok x -> Ok { title       = None 
                           publishDate = None
                           body        = Some x }
            | Error x -> Error x
    
        member x.Validate () =
               x.title      .IsSome
            && x.publishDate.IsSome
            && x.body       .IsSome
    
        member x.ToPostData () : Result<PostData, string> =
            match x.Validate () with
            | true -> Ok { title       = x.title.Value
                           publishDate = x.publishDate.Value
                           body        = x.body.Value }
            | false -> Error (sprintf "Not all properties are defined on post." )

    let private convertTitle source =
        match PostTitle.create source with
        | Ok x -> Ok (Some x)
        | Error x -> Error (sprintf "Title was not in the correct format: {%s}." source)

    let private convertDate (source: string) =
        match System.DateTime.TryParse source with
        | (true, v) -> Ok (Some (PostPublishDate.create v))
        | (false, _) -> Error (sprintf "Date was not in the correct format: {%s}." source)

    let private bindApply pull push =
        let fn x =
            match pull x with
            | Ok r -> Ok (push r)
            | Error x -> Error x
        fn

    let private applyTitle (data: PostDataAggr) x = 
        bindApply convertTitle (fun x -> { data with title = x }) x

    let private applyPublishDate (data: PostDataAggr) x =
        bindApply convertDate (fun x -> { data with publishDate = x }) x

    let private applyProp prop (data: PostDataAggr) =
        match prop.key with
        | "Title" -> applyTitle data prop.value
        | "PublishDate" -> applyPublishDate data prop.value
        | x -> Error (sprintf "Error: unknown property {%s}" x)

    let rec private applyProps props (data, errors) =
        let fn data errors r =
            match r with
            | Ok x -> (x, errors)
            | Error x -> (data, x :: errors)

        match props with
        | [] -> (data, errors)
        | p :: tail -> 
            applyProp p data 
            |> fn data errors 
            |> applyProps tail

    let private parsePost source =
        let (rest, props) = Parse.parseProps source
        match PostDataAggr.Empty rest with
        | Ok aggr ->
            let (data, errors) = applyProps props (aggr, [])
            (data.ToPostData (), errors)
        | Error x -> (Error x, [])

    let private loadMarkDown text =
        let (r, errs) = parsePost text
        match r with
        | Ok post -> Success { post = post; errors = errs }
        | Error x -> Fail { errors = x :: errs }

    let private loadFile (path: FilePath) =
        let mkResult p r =
            { path = p
              result = r}

        match path.Extension with
        | ".md" ->
            path.ReadAsText ()
            |> loadMarkDown
            |> mkResult path.AsPath

        | x -> mkResult path.AsPath <| Fail { errors = [ (sprintf "Unknown extension: {%s}" x ) ] }

    let fromDir (dir: DirPath) : PostLoadResult list =
        dir.GetFiles ()
        |> List.ofArray
        |> List.map loadFile