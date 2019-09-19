namespace Tests

open NUnit.Framework
open Posts

module Docs =
    
    type DocTestCase =
        { input:               string
          expectedProps:       (string * string) list
          expectedBody:        string }

    [<Literal>]
    let doc1 = 
        "@Title: Some title\r\n" +
        "@PublishDate: 2010-2-1\r\n" +
        "\r\n" +
        "# Header\r\n" +
        "Body"

    let case1 =
        { input = doc1
          expectedProps = [ ("Title", "Some title")
                            ("PublishDate", "2010-2-1") ]
          expectedBody = "# Header\r\nBody" }


open Docs
open Parse

type TestClass () =

    let assertProp (ek, ev) prop =
        Assert.AreEqual(ek, prop.key)
        Assert.AreEqual(ev, prop.value)

    [<SetUp>]
    member this.Setup () =
        ()

    static member getCases () =
        [ yield (new TestCaseData(case1)).SetName("ParsePost 1") ]

    [<Test>]
    [<TestCaseSource("getCases")>]
    member this.ParsePost case =
        let pd = Parse.parseProps case.input

        List.rev pd.properties
        |> List.iter2 assertProp case.expectedProps
        
        Assert.AreEqual(case.expectedBody, pd.body)

