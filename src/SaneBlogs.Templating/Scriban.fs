module Engine

open Domain

type ScrTmpl = Scriban.Template



let parse (path: FilePath) =
    let text = path.ReadAsText ()
    Scriban.Template.Parse(text, path.Value)
        