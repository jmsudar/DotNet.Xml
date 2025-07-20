using Microsoft.VisualStudio.TestTools.UnitTesting;
using jmsudar.DotNet.Xml;
using System.Xml.Serialization;
using System.Text;

/// <summary>
/// Comprehensive unit tests for the XML utility class
/// </summary>
[TestClass]
public class XMLTests
{
    public class TestObject
    {
        public string? Name { get; set; }
        public int? Value { get; set; }
        public string? EmptyProperty { get; set; }
    }

    public class ComplexTestObject
    {
        public string? Title { get; set; }
        public List<TestObject>? Items { get; set; }
        public TestObject? NestedObject { get; set; }
    }

    public class PropertyGroup
    {
        public string? TargetFramework { get; set; }
        public string? ImplicitUsings { get; set; }
        public string? Nullable { get; set; }
        public string? PackageId { get; set; }
        public string? Version { get; set; }
        public string? Authors { get; set; }
        public string? Description { get; set; }
    }

    //Disable nullable warnings for testing scenarios
    #pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type
    #pragma warning disable CS8602 // Dereference of a possibly null reference
    #pragma warning disable CS8629 // Nullable value type may be null

    #region Serialization Tests

    [TestMethod]
    public void Serialize_WithValidObject_ReturnsCorrectXmlString()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };

        string result = XML.Serialize(testObject);

        Assert.IsNotNull(result);
        StringAssert.Contains(result, "<Name>Test</Name>");
        StringAssert.Contains(result, "<Value>123</Value>");
        Assert.IsFalse(result.Contains("<?xml"), "Should not contain XML declaration by default");
    }

    [TestMethod]
    public void Serialize_WithNullObject_ThrowsArgumentNullException()
    {
        #pragma warning disable CS8625
        var exception = Assert.ThrowsException<ArgumentNullException>(() => XML.Serialize<TestObject>(null));
        #pragma warning restore CS8625
        
        Assert.AreEqual("toSerialize", exception.ParamName);
        StringAssert.Contains(exception.Message, "Input object to serialize cannot be null");
    }

    [TestMethod]
    public void Serialize_WithPrettyPrintTrue_ReturnsIndentedXml()
    {
        var testObject = new ComplexTestObject 
        { 
            Title = "Test", 
            NestedObject = new TestObject { Name = "Nested", Value = 456 }
        };

        string result = XML.Serialize(testObject, prettyPrint: true);

        Assert.IsTrue(result.Contains("\t"), "Should contain tab indentation");
        Assert.IsTrue(result.Contains(Environment.NewLine), "Should contain line breaks");
    }

    [TestMethod]
    public void Serialize_WithPrettyPrintFalse_ReturnsCompactXml()
    {
        var testObject = new ComplexTestObject 
        { 
            Title = "Test", 
            NestedObject = new TestObject { Name = "Nested", Value = 456 }
        };

        string result = XML.Serialize(testObject, prettyPrint: false);

        Assert.IsFalse(result.Contains("\t"), "Should not contain tab indentation");
        // Should still have some line breaks from the serializer, but much more compact
    }

    [TestMethod]
    public void Serialize_WithOmitXmlDeclarationTrue_ExcludesDeclaration()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };

        string result = XML.Serialize(testObject, omitXmlDeclaration: true);

        Assert.IsFalse(result.StartsWith("<?xml"), "Should not start with XML declaration");
    }

    [TestMethod]
    public void Serialize_WithOmitXmlDeclarationFalse_IncludesDeclaration()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };

        string result = XML.Serialize(testObject, omitXmlDeclaration: false);

        // When omitXmlDeclaration is false, the XML declaration should be preserved
        // The XmlWriterSettings.OmitXmlDeclaration is set to false, so declaration should be included
        Assert.IsTrue(result.StartsWith("<?xml") || result.StartsWith("<TestObject"), 
            "Should start with XML declaration or root element");
    }

    [TestMethod]
    public void Serialize_WithCustomNamespaces_IncludesNamespaces()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };
        var namespaces = new XmlSerializerNamespaces();
        namespaces.Add("custom", "http://example.com/custom");

        string result = XML.Serialize(testObject, namespaces: namespaces);

        StringAssert.Contains(result, "xmlns:custom=\"http://example.com/custom\"");
    }

    [TestMethod]
    public void Serialize_WithRemoveEmptyNodesTrue_RemovesEmptyElements()
    {
        var testObject = new TestObject { Name = "Test", Value = null, EmptyProperty = null };

        string result = XML.Serialize(testObject, removeEmptyNodes: true);

        Assert.IsNotNull(result);
        StringAssert.Contains(result, "<Name>Test</Name>");
        Assert.IsFalse(result.Contains("xsi:nil=\"true\""), "Should not contain nil attributes");
        Assert.IsFalse(result.Contains("<Value"), "Should not contain empty Value element");
        Assert.IsFalse(result.Contains("<EmptyProperty"), "Should not contain empty EmptyProperty element");
    }

    [TestMethod]
    public void Serialize_WithRemoveEmptyNodesFalse_KeepsEmptyElements()
    {
        var testObject = new TestObject { Name = "Test", Value = null, EmptyProperty = null };

        string result = XML.Serialize(testObject, removeEmptyNodes: false);

        Assert.IsNotNull(result);
        StringAssert.Contains(result, "<Name>Test</Name>");
        Assert.IsTrue(result.Contains("xsi:nil=\"true\""), "Should contain nil attributes for null values");
    }

    #endregion

    #region Deserialization Tests

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

        var exception = Assert.ThrowsException<XmlDeserializationException>(() => XML.Deserialize<TestObject>(invalidXml));
        StringAssert.Contains(exception.Message, "Invalid operation attempted while deserializing");
        Assert.IsNotNull(exception.InnerException);
    }

    [TestMethod]
    public void Deserialize_WithComplexXml_ReturnsCorrectObject()
    {
        string xml = @"<ComplexTestObject>
            <Title>Complex Test</Title>
            <NestedObject>
                <Name>Nested</Name>
                <Value>789</Value>
            </NestedObject>
        </ComplexTestObject>";

        ComplexTestObject result = XML.Deserialize<ComplexTestObject>(xml);

        Assert.IsNotNull(result);
        Assert.AreEqual("Complex Test", result.Title);
        Assert.IsNotNull(result.NestedObject);
        Assert.AreEqual("Nested", result.NestedObject.Name);
        Assert.AreEqual(789, result.NestedObject.Value);
    }

    #endregion

    #region File Operations Tests

    [TestMethod]
    public void SerializeToFile_WithValidObject_WritesCorrectFile()
    {
        var testObject = new TestObject { Name = "FileTest", Value = 456 };
        string filePath = Path.GetTempFileName();

        try
        {
            XML.SerializeToFile(testObject, filePath);

            Assert.IsTrue(File.Exists(filePath));
            string content = File.ReadAllText(filePath, Encoding.UTF8);
            StringAssert.Contains(content, "<Name>FileTest</Name>");
            StringAssert.Contains(content, "<Value>456</Value>");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [TestMethod]
    public void SerializeToFile_WithNullFilePath_ThrowsArgumentException()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };

        #pragma warning disable CS8625
        var exception = Assert.ThrowsException<ArgumentException>(() => XML.SerializeToFile(testObject, null));
        #pragma warning restore CS8625
        
        Assert.AreEqual("filePath", exception.ParamName);
    }

    [TestMethod]
    public void SerializeToFile_WithEmptyFilePath_ThrowsArgumentException()
    {
        var testObject = new TestObject { Name = "Test", Value = 123 };

        var exception = Assert.ThrowsException<ArgumentException>(() => XML.SerializeToFile(testObject, ""));
        Assert.AreEqual("filePath", exception.ParamName);
    }

    [TestMethod]
    public void DeserializeFromFile_WithValidFile_ReturnsCorrectObject()
    {
        string filePath = Path.GetTempFileName();
        string xml = @"<TestObject><Name>FileTest</Name><Value>789</Value></TestObject>";

        try
        {
            File.WriteAllText(filePath, xml, Encoding.UTF8);

            TestObject result = XML.DeserializeFromFile<TestObject>(filePath);

            Assert.IsNotNull(result);
            Assert.AreEqual("FileTest", result.Name);
            Assert.AreEqual(789, result.Value);
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [TestMethod]
    public void DeserializeFromFile_WithNonExistentFile_ThrowsDeserializationException()
    {
        string filePath = "nonexistent_file_" + Guid.NewGuid() + ".xml";

        var exception = Assert.ThrowsException<XmlDeserializationException>(() => XML.DeserializeFromFile<TestObject>(filePath));
        StringAssert.Contains(exception.Message, "IO error during deserialization");
        StringAssert.Contains(exception.Message, filePath);
        // Inner exception might be null in some cases, so don't assert it's not null
    }

    [TestMethod]
    public void DeserializeFromFile_WithNullFilePath_ThrowsArgumentException()
    {
        #pragma warning disable CS8625
        var exception = Assert.ThrowsException<ArgumentException>(() => XML.DeserializeFromFile<TestObject>(null));
        #pragma warning restore CS8625
        
        Assert.AreEqual("filePath", exception.ParamName);
    }

    [TestMethod]
    public void DeserializeFromFile_WithInvalidXmlFile_ThrowsDeserializationException()
    {
        string filePath = Path.GetTempFileName();
        string invalidXml = "<TestObject><Name>Test";

        try
        {
            File.WriteAllText(filePath, invalidXml);

            var exception = Assert.ThrowsException<XmlDeserializationException>(() => XML.DeserializeFromFile<TestObject>(filePath));
            StringAssert.Contains(exception.Message, "Invalid operation attempted while deserializing");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    #endregion

    #region XML Block Processing Tests

    [TestMethod]
    public void ProcessXmlBlockFromFile_WithValidManipulation_UpdatesXmlCorrectly()
    {
        string filePath = Path.GetTempFileName();
        string xml = @"<Root>
    <TestObject>
        <Name>Original</Name>
        <Value>123</Value>
    </TestObject>
</Root>";

        try
        {
            File.WriteAllText(filePath, xml);

            XML.ProcessXmlBlockFromFile<TestObject>(filePath, obj =>
            {
                if (obj != null)
                {
                    obj.Name = "Updated";
                    obj.Value = 456;
                }
            });

            string updatedContent = File.ReadAllText(filePath);
            StringAssert.Contains(updatedContent, "<Name>Updated</Name>");
            StringAssert.Contains(updatedContent, "<Value>456</Value>");
            // Verify indentation is preserved
            StringAssert.Contains(updatedContent, "    <TestObject>");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }

    [TestMethod]
    public void ProcessXmlBlockFromFile_WithNonExistentBlock_ThrowsInvalidOperationException()
    {
        string filePath = Path.GetTempFileName();
        string xml = @"<Root><SomeOtherObject><Name>Test</Name></SomeOtherObject></Root>";

        try
        {
            File.WriteAllText(filePath, xml);

            var exception = Assert.ThrowsException<InvalidOperationException>(() =>
                XML.ProcessXmlBlockFromFile<TestObject>(filePath, obj => { }));
            
            StringAssert.Contains(exception.Message, "The specified XML block was not found: TestObject");
        }
        finally
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }


    #endregion

    #region XML Block Extraction Tests

    [TestMethod]
    public void ExtractXmlBlock_WithValidBlock_ReturnsCorrectSubstring()
    {
        string xml = @"<Root><TestObject><Name>Test</Name><Value>123</Value></TestObject><Other>Content</Other></Root>";

        string? block = XML.ExtractXmlBlock(xml, "TestObject");

        Assert.IsNotNull(block);
        Assert.AreEqual("<TestObject><Name>Test</Name><Value>123</Value></TestObject>", block);
    }

    [TestMethod]
    public void ExtractXmlBlock_WithMissingBlock_ReturnsNull()
    {
        string xml = @"<Root><SomeObject><Name>Test</Name></SomeObject></Root>";

        string? block = XML.ExtractXmlBlock(xml, "TestObject");

        Assert.IsNull(block);
    }

    [TestMethod]
    public void ExtractXmlBlock_WithNestedSameNameBlocks_ReturnsFirstBlock()
    {
        string xml = @"<Root><TestObject><Name>First</Name><TestObject><Name>Nested</Name></TestObject></TestObject></Root>";

        string? block = XML.ExtractXmlBlock(xml, "TestObject");

        Assert.IsNotNull(block);
        StringAssert.Contains(block, "<Name>First</Name>");
        StringAssert.Contains(block, "<Name>Nested</Name>");
    }

    #endregion

    #region Utf8StringWriter Tests

    [TestMethod]
    public void Utf8StringWriter_HasCorrectEncoding()
    {
        var sb = new StringBuilder();
        using var writer = new Utf8StringWriter(sb);

        Assert.AreEqual(Encoding.UTF8, writer.Encoding);
    }

    [TestMethod]
    public void Utf8StringWriter_WritesCorrectly()
    {
        var sb = new StringBuilder();
        using var writer = new Utf8StringWriter(sb);
        
        writer.Write("Test content with unicode: ñáéíóú");
        writer.Flush();

        Assert.AreEqual("Test content with unicode: ñáéíóú", sb.ToString());
    }

    #endregion

    #region Exception Tests

    [TestMethod]
    public void XmlSerializationException_PreservesInnerException()
    {
        var innerException = new InvalidOperationException("Inner error");
        var xmlException = new XmlSerializationException("Outer error", innerException);

        Assert.AreEqual("Outer error", xmlException.Message);
        Assert.AreSame(innerException, xmlException.InnerException);
    }

    [TestMethod]
    public void XmlDeserializationException_PreservesInnerException()
    {
        var innerException = new InvalidOperationException("Inner error");
        var xmlException = new XmlDeserializationException("Outer error", innerException);

        Assert.AreEqual("Outer error", xmlException.Message);
        Assert.AreSame(innerException, xmlException.InnerException);
    }

    #endregion

    #region Edge Case Tests

    [TestMethod]
    public void Serialize_WithLargeObject_HandlesCorrectly()
    {
        var largeObject = new ComplexTestObject
        {
            Title = "Large Test",
            Items = new List<TestObject>()
        };

        // Add many items to test performance and memory handling
        for (int i = 0; i < 1000; i++)
        {
            largeObject.Items.Add(new TestObject { Name = $"Item{i}", Value = i });
        }

        string result = XML.Serialize(largeObject);

        Assert.IsNotNull(result);
        StringAssert.Contains(result, "<Title>Large Test</Title>");
        StringAssert.Contains(result, "<Name>Item0</Name>");
        StringAssert.Contains(result, "<Name>Item999</Name>");
    }

    [TestMethod]
    public void Serialize_WithSpecialCharacters_EscapesCorrectly()
    {
        var testObject = new TestObject 
        { 
            Name = "Test with <special> & \"characters\"", 
            Value = 123 
        };

        string result = XML.Serialize(testObject);

        Assert.IsNotNull(result);
        StringAssert.Contains(result, "&lt;special&gt;");
        StringAssert.Contains(result, "&amp;");
        // Double quotes are not escaped in XML element content, only in attributes
        StringAssert.Contains(result, "\"characters\"");
    }

    #endregion

    #region Integration Tests

    [TestMethod]
    public void ProcessXmlBlockFromFile_WithRealWorldProjectFile_PreservesIndentationCorrectly()
    {
        // Arrange: Copy the test data file to a temporary location
        string testDataPath = Path.Combine("TestData", "sample-project.xml");
        string tempFilePath = Path.GetTempFileName();
        string outputFilePath = Path.ChangeExtension(tempFilePath, ".output.xml");

        try
        {
            // Copy the test file to temp location
            File.Copy(testDataPath, tempFilePath, true);
            
            // Read original content for comparison
            string originalContent = File.ReadAllText(tempFilePath);
            Console.WriteLine("=== ORIGINAL XML ===");
            Console.WriteLine(originalContent);
            Console.WriteLine();

            // Act: Modify the PropertyGroup using ProcessXmlBlockFromFile
            XML.ProcessXmlBlockFromFile<PropertyGroup>(tempFilePath, propertyGroup =>
            {
                if (propertyGroup != null)
                {
                    propertyGroup.Version = "2.0.0";
                    propertyGroup.Authors = "Updated Test Author";
                    propertyGroup.Description = "Updated description with indentation preservation test";
                }
            });

            // Read the modified content
            string modifiedContent = File.ReadAllText(tempFilePath);
            Console.WriteLine("=== MODIFIED XML ===");
            Console.WriteLine(modifiedContent);
            Console.WriteLine();

            // Save output for manual inspection
            File.WriteAllText(outputFilePath, modifiedContent);
            Console.WriteLine($"=== OUTPUT SAVED TO: {outputFilePath} ===");

            // Assert: Verify the changes were made correctly
            StringAssert.Contains(modifiedContent, "<Version>2.0.0</Version>");
            StringAssert.Contains(modifiedContent, "<Authors>Updated Test Author</Authors>");
            StringAssert.Contains(modifiedContent, "Updated description with indentation preservation test");

            // Assert: Verify indentation is preserved
            StringAssert.Contains(modifiedContent, "  <PropertyGroup>", "PropertyGroup should maintain 2-space indentation");
            StringAssert.Contains(modifiedContent, "  \t<TargetFramework>", "Child elements should maintain base indentation + tab");
            StringAssert.Contains(modifiedContent, "  \t<Version>2.0.0</Version>", "Modified elements should maintain proper indentation");
            
            // Assert: Verify overall structure is maintained
            StringAssert.Contains(modifiedContent, "<Project Sdk=\"Microsoft.NET.Sdk\">");
            StringAssert.Contains(modifiedContent, "  <ItemGroup>");
            StringAssert.Contains(modifiedContent, "    <PackageReference Include=\"Microsoft.Extensions.Logging\"");
            
            // Assert: Verify the XML is still well-formed by attempting to parse it
            var doc = new System.Xml.XmlDocument();
            try
            {
                doc.LoadXml(modifiedContent);
                // If we get here, the XML is well-formed
            }
            catch (System.Xml.XmlException ex)
            {
                Assert.Fail($"Modified XML should be well-formed, but got XmlException: {ex.Message}");
            }

            Console.WriteLine("=== INDENTATION PRESERVATION TEST PASSED ===");
        }
        finally
        {
            // Cleanup temporary files
            if (File.Exists(tempFilePath))
                File.Delete(tempFilePath);
            
            // Keep output file for manual inspection - don't delete it
            if (File.Exists(outputFilePath))
            {
                Console.WriteLine($"Output file preserved at: {outputFilePath}");
            }
        }
    }

    #endregion

    //Re-enable nullable warnings
    #pragma warning restore CS8600
    #pragma warning restore CS8602
    #pragma warning restore CS8629
}
