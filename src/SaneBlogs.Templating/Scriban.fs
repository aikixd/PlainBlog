namespace SaneBlogs.Templating

type ScrTmpl = Scriban.Template

module Scriban =

    open Types
    open System.IO

    let parse (PathName path) =
        match File.Exists path with
        | true -> ScrTmpl.Parse(File.ReadAllText(path), path) |> Ok
        | false -> sprintf "File '%s' doen't exist." path |> Error