namespace Tests

open NUnit.Framework
open Posts

module Docs =
    
    type DocTestCase =
        { input:               string
          expectedTitle:       string
          expectedPublishDate: System.DateTime
          expectedBody:        string
          expectedErrors:      string list
          shouldSucceed:       bool }

    [<Literal>]
    let doc1 = 
        "@Title: Some title\r\n" +
        "@PublishDate: 2010-2-1\r\n" +
        "\r\n" +
        "# Header\r\n" +
        "Body"

    let case1 =
        { input = doc1
          expectedTitle = "Some title"
          expectedPublishDate = new System.DateTime(2010, 2, 1)
          expectedBody = "# Header\r\nBody"
          expectedErrors = []
          shouldSucceed = true }


open Docs

[<TestClass>]
type TestClass () =

    [<SetUp>]
    member this.Setup () =
        ()

    static member getCases () =
        [ yield (new TestCaseData(case1)).SetName("ParsePost 1") ]

    [<Test>]
    [<TestCaseSource("getCases")>]
    member this.ParsePost case =
        let (result, errors) = Parse.parsePost case.input
        
        match result with
        | Ok data ->
            Assert.AreEqual(case.shouldSucceed, true)
            Assert.AreEqual(case.expectedTitle, data.title.Value)
            Assert.AreEqual(case.expectedPublishDate, data.publishDate.Value)
            Assert.AreEqual(case.expectedBody, data.body.Value)
        | Error x ->
            Assert.AreEqual(case.shouldSucceed, false)

        Assert.AreEqual(case.expectedErrors, errors)

