module Engine

open Domain
open Scriban.Parsing

type ScrTmpl = Scriban.Template



let parse (path: FilePath) =
    let text = path.ReadAsText ()
    
    let mutable lexerOpts = new LexerOptions()
    lexerOpts.KeepTrivia <- true
    
    Scriban.Template.Parse(text, path.Value, lexerOptions = (System.Nullable lexerOpts))
        