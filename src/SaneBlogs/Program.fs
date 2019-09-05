open System
open Domain
open Posts.Load



let postCollector post =
    match post.result with
    | Success x ->
        List.iter (fun x' -> printf "Warning: File '%s':\r\n  %s" post.path.Value x') x.errors
        Some x.post
    | Fail x ->
        List.iter (fun x' -> printf "Warning: File '%s':\r\n  %s" post.path.Value x') x.errors
        None



[<EntryPoint>]
let main argv =
    
    let t = System.IO.Path.GetFullPath(System.IO.Directory.GetCurrentDirectory() + "..\\..\\..\\..\\..\\..")

    let result = asserted {
        let! curDir = t |> DirPath.create
        
        let! appended = Path.combine (curDir.Value, "posts")
        let! postsDir =  DirPath.fromPath appended

        let! appended = Path.combine (curDir.Value, "templates")
        let! tmplDir = DirPath.fromPath appended

        let! appended = Path.combine (curDir.Value, "dist")

        let loadPostResults = Posts.Load.fromDir postsDir
        let posts = List.choose postCollector loadPostResults

        let! render = TemplateRig.load tmplDir

        let result = render posts

        let! indexFile = 
            curDir.Path.Append "dist\\index.html"
            |> Result.bind (FilePath.create result)

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
    
