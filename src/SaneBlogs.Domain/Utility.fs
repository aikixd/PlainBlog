namespace Domain

[<AutoOpen>]
module Utility =
    type AssertedBuilder() =
        member x.Bind (m, f) =
            match m with
            | Ok m' -> f m'
            | Error e -> Error e
    
        member x.Return v = Ok v

        member x.Zero () = Ok ()
    
    let asserted = new AssertedBuilder()

    let kebabify (str: string) =
        str .ToLower()
            .Replace(" ", "-")
            .Replace("\"", "")

    let (|Prefix|_|) (p:string) (s:string) =
        if s.StartsWith(p) then
            Some(s.Substring(p.Length))
        else
            None