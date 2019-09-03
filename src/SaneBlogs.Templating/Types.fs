module Types

open System.IO;

type PathName = | PathName of string
    with
    static member create path =
        path 
        |> System.Environment.ExpandEnvironmentVariables 
        |> Path.GetFullPath

