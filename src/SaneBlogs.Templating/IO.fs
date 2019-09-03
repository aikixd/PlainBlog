namespace SaneBlogs.Templating

open System.IO

module IO =

    open Types
    open Scriban

    let load path =
        
        let tmpl = parse path