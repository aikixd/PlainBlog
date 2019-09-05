namespace Posts

module Load =
    
    open Domain
    open Data

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

    let private loadMarkDown text =
        let (r, errs) = Parse.parsePost text
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

    let fromDir (dir: DirPath) =
        dir.GetFiles ()
        |> List.ofArray
        |> List.map loadFile



        