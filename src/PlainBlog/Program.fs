open System
open Domain
open Load
open Posts.Load
open Pages.Load
open Templating
open LibSassHost

let loadCollector loadRes =
    match loadRes with
    | Ok x ->
        printfn "√ - %s" x.file.Value
        List.iter (fun x' -> printfn "    ! - %s" x') x.entityLoad.errors
        Some x.entityLoad.entity
    | Error x -> 
        printf "X -"
        List.iteri 
            (fun i x' -> printfn "%s %s" 
                                 (if i = 0 then "" else "    >")
                                 x' ) 
            x
        None

let mkPage render toHtml (page: Page) =
    ( kebabify page.title.Value, 
      render (DocModel.fromPage toHtml page) "Page" )

let mkPost render toHtml (post: Post) =
    ( "posts\\" + kebabify post.title.Value, 
      render (PostModel.fromPost toHtml post) "Post" )

let mkIndex render =
    ( "Index",
      render () )

let saveHtml (pubDir: Path) (path, result) =
    pubDir.Append (path + ".html")
    |> Result.bind (FilePath.create result)

let generateSite (args: Argu.ParseResults<Args.Arguments>) =
    asserted {
        let curDirAppender = 
            match args.Contains Args.Arguments.Working_Directory with
            | true -> args.GetResult Args.Arguments.Working_Directory
            | false -> ".\\"
        
        let! curDir = 
            Path.combine(System.IO.Directory.GetCurrentDirectory(), curDirAppender)
            |> Result.bind DirPath.fromPath

        let! pubDir =
            match args.Contains Args.Arguments.Out_Dir with
            | true ->
                curDir.Path.Append (args.GetResult Args.Arguments.Out_Dir)
            | false ->
                curDir.Path.Append "dist"

        printfn "Working dir: %s" curDir.Value
        
        let! postsDir = 
            curDir.Path.Append "posts"
            |> Result.bind DirPath.fromPath
        
        let! pagesDir =
            curDir.Path.Append "pages"
            |> Result.bind DirPath.fromPath

        let! tmplDir =
            curDir.Path.Append "templates"
            |> Result.bind DirPath.fromPath

        let! staticDir =
            curDir.Path.Append "static"
            |> Result.bind DirPath.fromPath

        let posts =
            postsDir
            |> Load.fromDir parsePost 
            |> List.choose loadCollector
            |> List.sortByDescending (fun x -> x.publishDate)

        let mdToHtml = Markdown.mdToHtml (args.TryGetResult Args.Arguments.Markdown_Extensions)

        let postModels =
            posts
            |> List.map (PostModel.fromPost mdToHtml)
            |> Array.ofList


        let pages =
            pagesDir
            |> Load.fromDir parsePage
            |> List.choose loadCollector

        let! tmpl = Template.load tmplDir

        let site = { name = args.GetResult Args.Arguments.Blog_Name }

        let pagesFiles = 
            pages
            |> List.map ((mkPage (tmpl.RenderPage site postModels) mdToHtml) >> (saveHtml pubDir))
        let postsFiles =
            posts
            |> List.map ((mkPost (tmpl.RenderPost site postModels) mdToHtml) >> (saveHtml pubDir))

        let indexFile = 
            tmpl.RenderIndex site postModels
            |> mkIndex 
            |> saveHtml pubDir

        let! scss =
            curDir.Path.Append("style\\main.scss")
            |> Result.bind FilePath.fromPath

        let result = SassCompiler.Compile(scss.ReadAsText ())

        pubDir.Append("style.css")
        |> Result.bind (FilePath.create result.CompiledContent)
        |> ignore
    }

[<EntryPoint>]
let main argv =
    
    Console.OutputEncoding <- Text.Encoding.UTF8;

    let args =
        try
            Some (Args.parseArgs argv)
        with e -> 
            printfn "%s" e.Message
            None

    match args with
    | Some args ->
        let result = generateSite args

        match result with
        | Ok () ->
            0
        | Error x ->
            printfn "Could not determine required directories: \r\n%s" x
            1
    | None -> 0