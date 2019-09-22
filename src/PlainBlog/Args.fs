module Args

open Argu;

type Arguments = 
    | [<AltCommandLine("-d")>] Working_Directory of path:string
    | [<Mandatory>][<EqualsAssignment>] Blog_Name of blogName:string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Working_Directory _ -> "Specify a working directory."
            | Blog_Name _ -> "Specify a blog name. Mandatory.\r\nExample: --blog-name=\"My blog name\""

let parseArgs args =
    let p = ArgumentParser.Create<Arguments>(programName = "PlainBlog.exe")
    p.Parse args

