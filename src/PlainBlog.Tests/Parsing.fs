namespace Tests

open NUnit.Framework
open Posts

module Cases =
    
    type PostTestCase =
        { input:               string
          expectedProps:       (string * string) list
          expectedBody:        string
          expectedErrors:      string list option }

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
          expectedBody = "# Header\r\nBody" 
          expectedErrors = None }

    [<Literal>]
    let doc2 = 
        "@Title: Some title\r\n" +
        "@PublishDate: 2010-a-b\r\n" +
        "\r\n" +
        "# Header\r\n" +
        "Body"

    let case2 =
        { input = doc2
          expectedProps = [ ("Title", "Some title")
                            ("PublishDate", "2010-a-b") ]
          expectedBody = "# Header\r\nBody"
          expectedErrors = Some [ "Couldn't create post data"
                                  "Not all properties are defined on post."
                                  "Date is not in the correct format: {2010-a-b}" ] }


open Cases
open Parse

type TestClass () =

    let assertProp (ek, ev) prop =
        Assert.AreEqual(ek, prop.key)
        Assert.AreEqual(ev, prop.value)

    [<SetUp>]
    member this.Setup () =
        ()

    static member getPostsCases () =
        [ yield (new TestCaseData(case1)).SetName("ParsePost 1")
          yield (new TestCaseData(case2)).SetName("ParsePost 2") ]

    [<Test>]
    [<TestCaseSource("getPostsCases")>]
    member this.ParsePost case =
        let pd = Parse.parseProps case.input

        List.rev pd.properties
        |> List.iter2 assertProp case.expectedProps
        
        Assert.AreEqual(case.expectedBody, pd.body)

        match Posts.Load.parsePost pd with
        | Ok _ when case.expectedErrors.IsSome -> Assert.Fail("Case should have failed.")
        | Ok post -> ()
        | Error xs -> 
            List.iter2 (fun x y -> Assert.AreEqual(x, y)) case.expectedErrors.Value xs

