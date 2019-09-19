namespace Domain

open System
open System.IO;

[<AutoOpen>]
module PathModule =
    type private SysPath = System.IO.Path
    type private SysFile = System.IO.File
    type private SysDir = System.IO.Directory

    type Path = | X_ of string
        with
        member x.Value =
            match x with X_ x -> x
        member x.Append part =
            SysPath.Combine(x.Value, part)
            |> Path.create
        member x.IsDirPath =
            try
                File.GetAttributes(x.Value).HasFlag FileAttributes.Directory
            with
            // We don't care for the exceptions, path may be incomplete. 
            | e -> false
        member x.IsFilePath = not x.IsDirPath
        
        static member create path =
            try
                path 
                |> System.Environment.ExpandEnvironmentVariables 
                |> SysPath.GetFullPath
                |> X_ 
                |> Ok
            with
            | e -> Error e.Message
        
        static member combine ([<ParamArray>] parts) =
            try
                SysPath.Combine(parts)
                |> Path.create
            with
            | e -> Error e.Message
    
    // rename to 'Dir'
    type DirPath = | X_ of Path with
        member x.Value =
            match x with X_ x -> x.Value
        member x.Path =
            match x with X_ x -> x

        member x.GetFiles () =
            // I assert the `GetFiles` works correctly
            let chooser = (function Ok x -> Some x | _ -> None)
            
            SysDir.GetFiles(x.Value)
            |> Array.map Path.create
            |> Array.choose chooser
            |> Array.map FilePath.fromPath
            |> Array.choose chooser

        member x.FindFile pattern =
            let r = SysDir.GetFiles(x.Value, pattern)

            match r.Length with
            | 1 -> 
                r.[0]
                |> Path.create
                |> Result.bind FilePath.fromPath
                
            | 0 -> Error "No file found"
            | _ -> Error (sprintf "Multiple matches found: %s" (Array.reduce (fun x y -> x + "\r\n" + y) r))
        
        static member create path =
            match Path.create path with
            | Ok path -> DirPath.fromPath path
            | Error x -> Error x

        static member fromPath (path: Path) =
            match path.IsDirPath with
            | true -> Ok (X_ path)
            | false -> Error (sprintf "Path is not a directory: {%s}."  path.Value)

    // rename to File
    and FilePath = | X_ of Path with
        member x.Value =
            match x with X_ x -> x.Value
        member x.Extension =
            SysPath.GetExtension(x.Value)
        member x.ReadAsText () =
            SysFile.ReadAllText(x.Value)
        member x.AsPath =
            match x with X_ x -> x
        member x.Name =
            SysPath.GetFileNameWithoutExtension(x.Value)

        static member create text (path: Path) =
            try
                Directory.CreateDirectory(SysPath.GetDirectoryName(path.Value)) |> ignore
                File.WriteAllText(path.Value, text)
                FilePath.fromPath path
            with
            | e -> Error e.Message

        static member fromPath (path: Path) =
            match path.IsFilePath with
            | true -> Ok (X_ path)
            | false -> Error (sprintf "Path is not a file: {%s}."  path.Value)