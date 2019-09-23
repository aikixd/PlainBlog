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
        static member create template level =
            { template = template
              level    = level }

        static member index =
            { template = "Index"
              level    = 0 }

    type DocModel =
        { title: string
          body:  string }
        with
        static member fromPage toHtml (page: Page) =
            { title = page.title.Value
              body  = toHtml page.body.Value }

    type PostModel = 
        { title:       string
          publishDate: System.DateTime
          body:        string
          intro:       string }
        with
        static member fromPost toHtml (post: Post) =
            { title       = post.title.Value
              publishDate = post.publishDate.Value
              body        = toHtml post.body.Value
              intro       = toHtml post.intro.Value }