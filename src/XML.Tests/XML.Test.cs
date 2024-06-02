using Microsoft.VisualStudio.TestTools.UnitTesting;
using jmsudar.DotNet.Xml;

/// <summary>
/// Placeholder summary
/// </summary>
[TestClass]
public class XMLTests
{

    public class TestObject
    {
        public string? Name { get; set; }
        public int Value { get; set; }
    }

    //Disable at the beginning of testing
    #pragma warning disable CS8600 // Suppressing CS8600
    #pragma warning disable CS8602 // Suppressing CS8602
    #pragma warning disable CS8629 // Suppressing CS8629

    [TestMethod]
    public void Serialize_WithValidObject_ReturnsNonNullXmlString()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };

        string result = XML.Serialize(testObject);

        Assert.IsNotNull(result);
        StringAssert.Contains(result, "<Name>Test</Name>");
        StringAssert.Contains(result, "<Value>123</Value>");
    }

    [TestMethod]
    public void Serialize_WithNullObject_ThrowsArgumentNullException()
    {
        // Disable nullable warning given that this is explicitly testing null
        #pragma warning disable CS8625
        Assert.ThrowsException<ArgumentNullException>(() => XML.Serialize<TestObject>(null));
        #pragma warning restore CS8625
    }

    [TestMethod]
    public void Deserialize_WithValidXml_ReturnsCorrectObject()
    {
        string xml = @"<TestObject><Name>Test</Name><Value>123</Value></TestObject>";

        TestObject result = XML.Deserialize<TestObject>(xml);

        Assert.IsNotNull(result);
        Assert.AreEqual("Test", result.Name);
        Assert.AreEqual(123, result.Value);
    }

    [TestMethod]
    public void Deserialize_WithInvalidXml_ThrowsDeserializationException()
    {
        string invalidXml = @"<TestObject><Name>Test</Name>";

        Assert.ThrowsException<XmlDeserializationException>(() => XML.Deserialize<TestObject>(invalidXml));
    }

    [TestMethod]
    public void SerializeToFile_WithValidObject_WritesFile()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };
        string filePath = Path.GetTempFileName();

        XML.SerializeToFile(testObject, filePath);

        Assert.IsTrue(File.Exists(filePath));
        string content = File.ReadAllText(filePath);
        StringAssert.Contains(content, "<Name>Test</Name>");
        StringAssert.Contains(content, "<Value>123</Value>");

        File.Delete(filePath);
    }

    [TestMethod]
    public void DeserializeFromFile_WithValidFile_ReturnsCorrectObject()
    {
        string filePath = Path.GetTempFileName();
        string xml = @"<TestObject><Name>Test</Name><Value>123</Value></TestObject>";
        File.WriteAllText(filePath, xml);

        TestObject result = XML.DeserializeFromFile<TestObject>(filePath);

        Assert.IsNotNull(result);
        Assert.AreEqual("Test", result.Name);
        Assert.AreEqual(123, result.Value);

        File.Delete(filePath);
    }

    [TestMethod]
    public void DeserializeFromFile_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        string filePath = "nonexistent.xml";

        var exception = Assert.ThrowsException<XmlDeserializationException>(() => XML.DeserializeFromFile<TestObject>(filePath));
        StringAssert.Contains(exception.Message, "IO error during deserialization from file 'nonexistent.xml'");

    }

    [TestMethod]
    public void SerializeToFile_WithInvalidPath_ThrowsArgumentException()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };

        // Disable nullable warning given that this is explicitly testing null
        #pragma warning disable CS8625
        Assert.ThrowsException<ArgumentException>(() => XML.SerializeToFile(testObject, null));
        Assert.ThrowsException<ArgumentException>(() => XML.SerializeToFile(testObject, ""));
        #pragma warning restore CS8625
    }

    [TestMethod]
    public void DeserializeFromFile_WithInvalidXml_ThrowsDeserializationException()
    {
        string filePath = Path.GetTempFileName();
        string invalidXml = "<TestObject><Name>Test";
        File.WriteAllText(filePath, invalidXml);

        Assert.ThrowsException<XmlDeserializationException>(() => XML.DeserializeFromFile<TestObject>(filePath));

        File.Delete(filePath);
    }

    [TestMethod]
    public void ProcessXmlBlockFromFile_ValidManipulation_UpdatesXml()
    {
        string filePath = Path.GetTempFileName();
        string xml = @"<Root><TestObject><Name>Test</Name><Value>123</Value></TestObject></Root>";
        File.WriteAllText(filePath, xml);

        void ManipulateObject(TestObject? obj)
        {
            if (obj != null)
            {
                obj.Name = "Updated";
            }
        }

        XML.ProcessXmlBlockFromFile<TestObject>(filePath, ManipulateObject);

        string updatedContent = File.ReadAllText(filePath);
        StringAssert.Contains(updatedContent, "<Name>Updated</Name>");

        File.Delete(filePath);
    }

    [TestMethod]
    public void ExtractXmlBlock_WithValidXmlBlock_ReturnsCorrectSubstring()
    {
        string xml = @"<Root><TestObject><Name>Test</Name><Value>123</Value></TestObject></Root>";
        string? block = XML.ExtractXmlBlock(xml, "TestObject");

        Assert.IsNotNull(block);
        StringAssert.Contains(block, "<TestObject><Name>Test</Name><Value>123</Value></TestObject>");
    }

    [TestMethod]
    public void ExtractXmlBlock_WithMissingXmlBlock_ReturnsNull()
    {
        string xml = @"<Root><TestObject><Name>Test</Name><Value>123</Value></TestObject></Root>";
        string? block = XML.ExtractXmlBlock(xml, "NonExistentObject");

        Assert.IsNull(block);
    }

    //Re-enable at the end of testing
#pragma warning restore CS8600 // Suppressing CS8600
#pragma warning restore CS8602 // Suppressing CS8602
#pragma warning restore CS8629 // Suppressing CS8629
}
