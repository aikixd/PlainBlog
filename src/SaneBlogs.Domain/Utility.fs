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

