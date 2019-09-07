module TemplateRig

open Domain
open Scriban
open Scriban.Parsing
open Scriban.Runtime

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
    member this.Render posts =
        let so = new Scriban.Runtime.ScriptObject()
        so.Add("posts", List.map PostCtx.fromPostData posts |> Array.ofList)
        so.Add("title", "Hello")
        
        let ctx = new Scriban.TemplateContext();
        ctx.PushGlobal(so)

        ctx.TemplateLoader <- new TemplateLoader(this.templatesDir)
        
        this.rootTemplate.Render(ctx)

    member this.RenderPage page site doc posts =
        let ctx = new Scriban.TemplateContext()

        let so = new Scriban.Runtime.ScriptObject()
        so.Add("doc", doc)
        so.Add("site", site)
        so.Add("page", page)
        so.Add("posts", posts)

        ctx.PushGlobal(so)

        ctx.TemplateLoader <- new TemplateLoader(this.templatesDir)

        this.rootTemplate.Render(ctx)

let load (path: DirPath) =
    asserted { 
        let! rootFile = path.FindFile "Root.*" 
        let root = Engine.parse rootFile
        
        return { rootTemplate = root 
                 templatesDir = path }

        //return rig.RenderPage
    }