module Load

open Domain
open Parse

let bindApply pull push =
    let fn x =
        match pull x with
        | Ok r -> Ok (push r)
        | Error x -> Error x
    fn

let rec applyProps applyProp props (data, errors) =
    let fn data errors r =
        match r with
        | Ok x -> (x, errors)
        | Error x -> (data, x :: errors)

    match props with
    | [] -> (data, errors)
    | p :: tail -> 
        applyProp p data 
        |> fn data errors 
        |> applyProps applyProp tail

type EntityLoadResult<'a> =
    { entity: 'a
      errors: string list }

type LoadedEntity<'a> =
    { file: FilePath
      entityLoad: EntityLoadResult<'a> }

let private loadMarkDown = parseProps

let file mkEntity (file: FilePath) =
    let mkResult r =
        match r with
        | Ok er -> Ok { file = file
                        entityLoad = er }
        | Error x -> Error ( sprintf "Cant't load file { %s }" file.Value :: x )

    match file.Extension with
    | ".md" ->
        file.ReadAsText ()
        |> loadMarkDown
        |> mkEntity
        |> mkResult

    | x -> Error [ sprintf "Can't load file { %s }" file.Value
                   sprintf "Unknown extension: { %s }" x ]

let fromDir mkEntity (dir: DirPath) =
    dir.GetFiles ()
    |> Array.map (file mkEntity)
    |> List.ofArray