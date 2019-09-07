namespace Render

open Domain

type PageModel =
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

type SiteModel =
    { name: string }