module Args

open Argu;

type Arguments = 
    | [<Mandatory>][<EqualsAssignment>] Blog_Name of blogName:string
    | [<AltCommandLine("-wd")>] Working_Directory of path:string
    | [<AltCommandLine("-od")>] Out_Dir of path:string
with
    interface IArgParserTemplate with
        member x.Usage =
            match x with
            | Working_Directory _ -> "Specify a working directory. Absolute or relative to system defined working dir."
            | Blog_Name _ -> "Specify a blog name. Mandatory.\r\nExample: --blog-name=\"My blog name\""
            | Out_Dir _ -> "Specify the output directory. Absolute or relative to working dir path."

let parseArgs args =
    let p = ArgumentParser.Create<Arguments>(programName = "PlainBlog.exe")
    p.Parse args