module Parse

open Domain

let private (<|>) f1 f2 =
    let fn source =
        match f1 source with
        | Ok x -> Ok x
        | Error () -> f2 source
    fn

type Property = 
    { key: string
      value: string }

let private parseEmpty (line: string) =
    match System.String.IsNullOrWhiteSpace(line) with
    | true -> Ok []
    | false -> Error ()

let private parseProperty line =
    let m = System.Text.RegularExpressions.Regex.Match
                ( line, 
                  "@(\w+) *: *([ \t\w-,.;@\"'`\\/\(\)\[\]\{\}]+)" )

    match m.Success with
    | true -> Ok <| [ { key   = m.Groups.[1].Value
                        value = m.Groups.[2].Value } ]
    | false -> Error ()

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

let rec private parsePropsImpl source props =
    let result = takeLine source
    
    match result with
    | Ok (line, rest) ->
        match (parseProperty <|> parseEmpty) line with
        | Ok x -> parsePropsImpl rest (x @ props)
        | Error _ -> (source, props)
    | Error _ -> (source, props)

let parseProps source =
    parsePropsImpl source []