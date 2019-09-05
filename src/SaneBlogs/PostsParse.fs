namespace Posts

module Parse =

    open Domain

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

    let private (<|>) f1 f2 =
        let fn source props =
            match f1 source props with
            | (true, props) -> (true, props)
            | (false, props) -> f2 source props
        fn

    let private parseEmpty (line: string) properies =
        (System.String.IsNullOrWhiteSpace(line), properies)

    let private parseProperty line properties =
        let m = System.Text.RegularExpressions.Regex.Match(line, "@(\w+) *: *([ \t\w-,.;@\"'`\\/\(\)\[\]\{\}]+)")

        match m.Success with
        | true -> (true, (m.Groups.[1].Value, m.Groups.[2].Value) :: properties)
        | false -> (false, properties)

    let private takeLine (source: string) =
        match System.String.IsNullOrWhiteSpace(source) with
        | true -> Error "End of text."
        | false ->    
            let arr = source.Split([|'\n'|], 2)
            match arr.Length with
            | 2 -> Ok (arr.[0], arr.[1])
            | 1 -> Ok (arr.[0], "")
            | 0 -> Ok ("", "")
            | _ -> Error "Undefined."
    
    let private parseLine source properties =
        match takeLine source with
        | Ok (line, rest) ->
            match (parseProperty <|> parseEmpty) line properties with
            | (true, props) -> (true, rest, props)
            | (false, props) -> (false, source, props)
        | _ -> (false, source, properties)

    let rec private parsePostImpl source props =
        match parseLine source props with
        | (false, rest, props) -> (rest, props)
        | (true, rest, props) -> parsePostImpl rest props

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
        match prop with
        | ("Title", v) -> applyTitle data v
        | ("PublishDate", v) -> applyPublishDate data v
        | (x, _) -> Error (sprintf "Error: unknown property {%s}" x)

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

    let parsePost source =
        let (rest, props) = parsePostImpl source []
        match PostDataAggr.Empty rest with
        | Ok aggr ->
            let (data, errors) = applyProps props (aggr, [])
            (data.ToPostData (), errors)
        | Error x -> (Error x, [])