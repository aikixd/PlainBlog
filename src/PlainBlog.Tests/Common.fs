namespace Tests

open NUnit.Framework
open Templating

type CommonTests () =
    [<Test>]
    [<TestCase(0, "/index.html", "./index.html")>]
    [<TestCase(1, "/index.html", "./../index.html")>]
    [<TestCase(1, "index.html", "index.html")>]
    [<TestCase(1, "./index.html", "./index.html")>]
    member this.MkLink level path canon =
        let r = mkLink level path

        Assert.AreEqual(canon, r)