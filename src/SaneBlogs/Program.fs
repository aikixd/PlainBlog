open System
open Domain
open Load
open Posts.Load
open Pages.Load
open Render
open LibSassHost

let loadCollector loadRes =
    match loadRes with
    | Ok x ->
        List.iter (fun x' -> printf "Warning: File '%s':\r\n  %s" x.file.Value x') x.entityLoad.errors
        Some x.entityLoad.entity
    | Error x -> 
        printf "%s" x
        None

let kebabify (str: string) =
    str.ToLower().Replace(" ", "-").Replace("\"", "")

let renderPage (curDir: DirPath) site render posts (page: Page)  =
    let result = render "Page" site (PageModel.fromPage page) (Array.ofList posts)
    let kebabed = kebabify page.title.Value
    
    curDir.Path.Append ("dist\\" + kebabed + ".html")
    |> Result.bind (FilePath.create result)

let renderPost (cirDir: DirPath) site render posts post =
    let result = render "Post" site (PostModel.fromPost post) (Array.ofList posts)
    let kebabed = kebabify post.title.Value

    cirDir.Path.Append("dist\\posts\\" + kebabed + ".html")
    |> Result.bind (FilePath.create result)

let renderIndex (cirDir: DirPath) site render posts =
    let posts =
        List.map PostModel.fromPost posts

    let result = render "Index" site { name = "blog name" } (Array.ofList posts)
    
    cirDir.Path.Append("dist\\index.html")
    |> Result.bind (FilePath.create result)

[<EntryPoint>]
let main argv =
    
    let t = System.IO.Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + "..\\..\\..\\..\\..\\..")

    let result = asserted {
        let! curDir = t |> DirPath.create
        
        let! postsDir = 
            curDir.Path.Append "posts"
            |> Result.bind DirPath.fromPath
        
        let! pagesDir =
            curDir.Path.Append "pages"
            |> Result.bind DirPath.fromPath

        let! tmplDir =
            curDir.Path.Append "templates"
            |> Result.bind DirPath.fromPath

        let posts =
            postsDir
            |> Load.fromDir mkPost 
            |> List.choose loadCollector

        let pages =
            pagesDir
            |> Load.fromDir mkPage
            |> List.choose loadCollector

        let! rig = TemplateRig.load tmplDir

        let site = { name = "Some blopg name" }

        let fn = renderPage curDir site rig.RenderPage posts
        
        let pagesFiles = 
            pages
            |> List.map fn

        let postsFiles =
            posts
            |> List.map (renderPost curDir site rig.RenderPage posts)

        let indexFile = renderIndex curDir site rig.RenderPage posts

        let! scss =
            curDir.Path.Append("style\\main.scss")
            |> Result.bind FilePath.fromPath

        let result = SassCompiler.Compile(scss.ReadAsText ())

        curDir.Path.Append("dist\\style.css")
        |> Result.bind (FilePath.create result.CompiledContent)
        |> ignore
        
        //let result = render posts

        //let! indexFile = 
        //    curDir.Path.Append "dist\\index.html"
        //    |> Result.bind (FilePath.create result)

        ()
    }

    match result with
    | Ok () ->
        0
    | Error x ->
        printfn "Could not determine required directories: \r\n%s" x
        1

        

    // load the templates
    // load posts
    // analyze posts
    //   - sort by date
    //   - create series
    // create side bar timeline
    // create posts list pages
    // create service pages
    
