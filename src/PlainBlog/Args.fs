module Args

open Argu;

type Arguments = 
    | [<AltCommandLine("-d")>] Working_Directory of path:string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Working_Directory _ -> "Specify a working directory."

let parseArgs args =
    let p = ArgumentParser.Create<Arguments>(programName = "PlainBlog.exe")
    p.Parse args

