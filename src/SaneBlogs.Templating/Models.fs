namespace Templating

open Domain

[<AutoOpen>]
module Models =

    type SiteModel =
        { name: string }

    type PageModel =
        { template: string
          level:    int }
        with
        static member create template =
            { template = template
              level    = 0 }

        static member index =
            { template = "Index"
              level    = 0 }

    type DocModel =
        { title: string
          body:  string }
        with
        static member fromPage (page: Page) =
            { title = page.title.Value
              body  = Markdig.Markdown.ToHtml(page.body.Value) }

    type PostModel = 
        { title:       string
          publishDate: System.DateTime
          body:        string }
        with
        static member fromPost (post: Post) =
            { title       = post.title.Value
              publishDate = post.publishDate.Value
              body        = Markdig.Markdown.ToHtml(post.body.Value) }