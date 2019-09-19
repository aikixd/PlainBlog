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
        List.iter (fun x' -> printf "Warning: File '%s':\r\n  %s" x.file.Value x') x.entityLoad.errors
        Some x.entityLoad.entity
    | Error x -> 
        printf "%s" x
        None

let prepPosts posts =
    posts
    |> List.map PostModel.fromPost
    |> Array.ofList

let mkPage (tmpl: Rig) site posts (page: Page) =
    let posts = prepPosts posts
    ( kebabify page.title.Value, 
      tmpl.RenderPage site posts (DocModel.fromPage page) "Page" )

let mkPost (tmpl: Rig) site posts (post: Post) =
    let posts = prepPosts posts
    ( "posts\\" + kebabify post.title.Value, 
      tmpl.RenderPost site posts (PostModel.fromPost post) "Post" )

let mkIndex (tmpl: Rig) site posts () =
    let posts = prepPosts posts
    ( "Index",
      tmpl.RenderIndex site posts )

let generate (curDir: DirPath) render model =
    let (path, result) = render model
    
    curDir.Path.Append ("dist\\" + path + ".html")
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
            |> Load.fromDir parsePost 
            |> List.choose loadCollector
            |> List.sortByDescending (fun x -> x.publishDate)

        let pages =
            pagesDir
            |> Load.fromDir parsePage
            |> List.choose loadCollector

        let! tmpl = Template.load tmplDir

        let site = { name = "Some blog name" }
        

        let pagesFiles = 
               List.map (generate curDir (mkPage tmpl site posts)  ) pages
        let postsFiles =
               List.map (generate curDir (mkPost tmpl site posts)  ) posts

        let indexFile = (generate curDir (mkIndex tmpl site posts) ) ()

        let! scss =
            curDir.Path.Append("style\\main.scss")
            |> Result.bind FilePath.fromPath

        let result = SassCompiler.Compile(scss.ReadAsText ())

        curDir.Path.Append("dist\\style.css")
        |> Result.bind (FilePath.create result.CompiledContent)
        |> ignore

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
    
