namespace Tests

open NUnit.Framework
open Parse

[<TestClass>]
type TestClass () =

    [<SetUp>]
    member this.Setup () =
        ()

    [<Test>]
    [<TestCase("name", "value", "@name: value")>]
    [<TestCase("name1", "value1", "@name1: value1")>]
    [<TestCase("name1", "va lue", "@name1: va lue")>]
    [<TestCase("name1", "[va} (lue@.", "@name1: [va} (lue@.")>]
    member this.ParseLine name value line =
        let (str, props) = Parse.parseLine line []
        let (n, v) = props.[0]

        Assert.AreEqual(1, props.Length)
        Assert.AreEqual("", str)
        Assert.AreEqual(name, n)
        Assert.AreEqual(value, v)
