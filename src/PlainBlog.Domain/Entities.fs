namespace Domain

[<AutoOpen>]
module Entities =
    
    type Post = 
        { title:       Title 
          publishDate: PublishDate
          body:        Body
          intro:       Intro }

    type Page =
        { title: Title
          body:  Body }
        
