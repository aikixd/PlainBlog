module Markdown

open Markdig

let private emptyPipeline = (new Markdig.MarkdownPipelineBuilder()).Build()

let private mkPipeline extensions =
    (new Markdig.MarkdownPipelineBuilder())
        .Configure(extensions)
        .Build()

let mdToHtml extensions =
    let pl =
        match extensions with
        | Some str -> mkPipeline str
        | None -> emptyPipeline
    
    fun text -> Markdig.Markdown.ToHtml(text, pl)
            