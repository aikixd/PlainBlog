namespace Templating

[<AutoOpen>]
module Template =

    open Domain
    open Scriban
    open Scriban.Parsing
    open Scriban.Runtime

    let mkLink level =
        let fn path =
            match path with
            | Prefix "/" tail -> "./" + (String.replicate level "../") + tail
            | _ -> path
        fn

    type private TemplateLoader(tmplDir: DirPath) =
        interface ITemplateLoader with
            member this.GetPath(_: TemplateContext, 
                                _: SourceSpan, 
                                templateName: string) =
                match tmplDir.FindFile (templateName + ".*") with
                | Ok path -> path.Value
                | Error e -> raise (System.IO.FileNotFoundException("Template not found: " + e))
    
            member this.Load(_: TemplateContext, 
                             _: SourceSpan, 
                             templateName: string): string = 
            
                System.IO.File.ReadAllText(templateName)
        
            member this.LoadAsync(_: TemplateContext, 
                                  _: SourceSpan, 
                                  templatePath: string)
                                  : System.Threading.Tasks.ValueTask<string> = 
            
                new System.Threading.Tasks.ValueTask<string>(System.IO.File.ReadAllTextAsync(templatePath))



    type private PostCtx = 
        { title: string
          publishDate: System.DateTime
          body: string } with
        static member fromPostData (data: Post) =
            { title = data.title.Value
              publishDate = data.publishDate.Value
              body = data.body.Value }

    type Document<'a> = 
        { title: string
          content: 'a }

    type Rig =
        { rootTemplate: Scriban.Template
          templatesDir: DirPath } with
        member this.Render page data  =
            let ctx = new Scriban.TemplateContext()
            
            let so = new Scriban.Runtime.ScriptObject()
            
            so.Add("page", page)

            Map.iter (fun k v -> so.Add(k, v)) data

            so.Import("kebabify", System.Func<string, string>(kebabify))
            so.Import("mkLink", System.Func<string, string>(mkLink page.level))
            
            ctx.PushGlobal(so)
            
            ctx.TemplateLoader <- new TemplateLoader(this.templatesDir)

            let mutable lexerOpts = new LexerOptions()
            lexerOpts.KeepTrivia <- true

            ctx.TemplateLoaderLexerOptions <- lexerOpts
            
            this.rootTemplate.Render(ctx)

        member this.RenderPage (site: SiteModel) (posts: PostModel[]) (model: DocModel) template =
            [ ("site",  site  :> obj)
              ("posts", posts :> obj) 
              ("doc",   model :> obj) ]
            |> Map.ofList
            |> this.Render (PageModel.create template 0)

        member this.RenderPost site posts (model: PostModel) template =
            [ ("site", site)
              ("posts", posts) 
              ("post", model :> obj) ]
            |> Map.ofList
            |> this.Render (PageModel.create template 1)

        member this.RenderIndex site posts =
            [ ("site", site :> obj)
              ("posts", posts :> obj) ]
            |> Map.ofList
            |> this.Render (PageModel.create "Index" 0)

    let load (path: DirPath) =
        asserted { 
            let! rootFile = path.FindFile "Root.*" 
            let root = Engine.parse rootFile
        
            return { rootTemplate = root 
                     templatesDir = path }
        }