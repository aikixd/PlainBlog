namespace Pages

module Load =
    open Domain
    open Parse
    open Load

    type private PageDataAggr =
        { title: Title option
          body:  Body option }
        with
        static member Empty source =
            match Body.create source with
            | Ok x -> Ok { title = None; body = Some x }
            | Error x -> Error x

        member x.Validate () =
               x.title.IsSome
            && x.body .IsSome

        member x.ToPage () : Result<Page, string> =
            match x.Validate () with
            | true -> Ok { title = x.title.Value
                           body =  x.body.Value }
            | false -> Error (sprintf "Not all properties are defined on page")

    let private applyTitle (data: PageDataAggr) =
        bindApply Title.create (fun x -> { data with title = Some x })

    let private applyProp prop data =
        match prop.key with
        | "Title" -> applyTitle data prop.value
        | x -> Error (sprintf "Error: unknown property {%s}" x)

    let mkPage (parsed: Parse.ParseData) =
        let conv aggr =
            let (data, errors) = applyProps applyProp parsed.properties (aggr, [])
            match data.ToPage () with
            | Ok data' -> Ok { entity = data' 
                               errors = errors }
            | Error x -> Error (sprintf "Couldn't create page data: %s" x)
        
        PageDataAggr.Empty parsed.body
        |> Result.bind conv
