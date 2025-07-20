using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace jmsudar.DotNet.Xml
{
    /// <summary>
    /// Methods for serializing and deserializing XML
    /// </summary>  
    public static class XML
    {
        /// <summary>
        /// Serializes an object into XML txt
        /// </summary>
        /// <typeparam name="T">The generic object type to serialize from</typeparam>
        /// <param name="toSerialize">The object you are serializing</param>
        /// <param name="namespaces">Optional XML serializer namespaces to include</param>
        /// <returns>Serialized XML text representing your object</returns>
        /// <exception cref="ArgumentNullException">Thrown if toSerialize is passed in as null</exception>
        /// <exception cref="XmlSerializationException">Custom exception surfacing any errors that occur during runtime</exception>
        public static string Serialize<T>(T toSerialize, bool removeEmptyNodes = true, bool prettyPrint = true, bool omitXmlDeclaration = true, XmlSerializerNamespaces? namespaces = null)
        {
            if (toSerialize == null)
                throw new ArgumentNullException(nameof(toSerialize), "Input object to serialize cannot be null.");

            try
            {
                var serializer = new XmlSerializer(typeof(T));

                var settings = new XmlWriterSettings
                {
                    Indent = prettyPrint,
                    IndentChars = "\t",
                    NewLineChars = Environment.NewLine,
                    NewLineHandling = NewLineHandling.Replace,
                    OmitXmlDeclaration = omitXmlDeclaration,
                    Encoding = new UTF8Encoding(false) // no BOM
                };

                var builder = new StringBuilder();

                using (var stringWriter = new StringWriter(builder))
                using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
                {
                    serializer.Serialize(xmlWriter, toSerialize, namespaces);
                }

                string xmlContent = builder.ToString();

                if (removeEmptyNodes)
                {
                    xmlContent = RemoveEmptyXml(xmlContent, settings);
                }

                xmlContent = RemoveDefaultNamespaces(xmlContent, settings);

                if (omitXmlDeclaration)
                {
                    xmlContent = RemoveXmlDeclaration(xmlContent);
                }

                return xmlContent;
            }
            catch (InvalidOperationException ex)
            {
                throw new XmlSerializationException($"Invalid operation attempted while serializing object to XML: {ex.Message}", ex.InnerException);
            }
            catch (Exception ex)
            {
                throw new XmlSerializationException($"Unexpected error: {ex.Message}", ex.InnerException);
            }
        }

        /// <summary>
        /// Removes the XML declaration from the beginning of an XML string
        /// </summary>
        /// <param name="xmlContent">The XML content to sanitize</param>
        /// <returns>The provided XML minus the XML declaration</returns>
        private static string RemoveXmlDeclaration(string xmlContent)
        {
            if (xmlContent.StartsWith("<?xml"))
            {
                int endOfDecl = xmlContent.IndexOf("?>");
                if (endOfDecl != -1)
                {
                    return xmlContent.Substring(endOfDecl + 2).TrimStart();
                }
            }

            return xmlContent;
        }

        /// <summary>
        /// Sanitizes XML content to remove any empty nodes
        /// </summary>
        /// <param name="xmlContent">The XML content to sanitize</param>
        /// <returns>The provided XML minus any empty nodes</returns>
        private static string RemoveEmptyXml(string xmlContent, XmlWriterSettings settings)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            if (doc.DocumentElement != null)
            {
                RemoveEmptyNodes(doc.DocumentElement);
            }

            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, settings))
            {
                doc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Performs a removal in place of any empty XML nodes
        /// </summary>
        /// <param name="node">The XML node being assessed</param>
        private static void RemoveEmptyNodes(XmlNode ?node)
        {
            // Check for null before proceeding
            if (node == null)
            {
                return;
            }

            for (int i = node.ChildNodes.Count - 1; i >= 0; i--)
            {
                var childNode = node.ChildNodes[i];
                if (childNode is XmlElement element)
                {
                    bool isEmpty = string.IsNullOrWhiteSpace(element.InnerXml)
                        && !element.HasAttributes;
                    bool isNil = element.HasAttribute("xsi:nil");

                    if (isEmpty || isNil)
                    {
                        node.RemoveChild(childNode);
                    }
                    else
                    {
                        RemoveEmptyNodes(childNode);
                    }
                }
            }
        }

        /// <summary>
        /// Removes default namespaces from an XML string
        /// </summary>
        /// <param name="xmlContent">The XML content to sanitize</param>
        /// <returns>An XML content minus any default namespaces</returns>
        private static string RemoveDefaultNamespaces(string xmlContent, XmlWriterSettings settings)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlContent);

            if (doc.DocumentElement != null)
            {
                doc.DocumentElement.RemoveAttribute("xmlns:xsi");
                doc.DocumentElement.RemoveAttribute("xmlns:xsd");
            }

            using (var stringWriter = new StringWriter())
            using (var xmlTextWriter = XmlWriter.Create(stringWriter, settings))
            {
                doc.WriteTo(xmlTextWriter);
                xmlTextWriter.Flush();
                return stringWriter.GetStringBuilder().ToString();
            }
        }

        /// <summary>
        /// Serializes an object to a given file path
        /// </summary>
        /// <typeparam name="T">The generic object type to serialize from</typeparam>
        /// <param name="toSerialize">The object you are serializing</param>
        /// <param name="filePath">The file path location for the serialized XML</param>
        /// <param name="removeEmptyNodes">Whether or not to remove empty nodes, defaults to true</param>
        /// <param name="namespaces">Optional XML serializer namespaces to include</param>
        /// <exception cref="ArgumentNullException">Thrown if the destination file path is passed in as null</exception>
        /// <exception cref="XmlSerializationException">Catches any IO errors</exception>
        public static void SerializeToFile<T>(T toSerialize, string filePath, bool removeEmptyNodes = true, bool prettyPrint = true, bool omitXmlDeclaration = true, XmlSerializerNamespaces? namespaces = null)
        {
            // Error if no file path is provided
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            try
            {
                string serializedData = Serialize(toSerialize, removeEmptyNodes, prettyPrint, omitXmlDeclaration, namespaces);
                File.WriteAllText(filePath, serializedData, Encoding.UTF8);
            }
            catch (IOException ex)
            {
                // Handle any IO errors during runtime
                throw new XmlSerializationException($"IO error during serialization to file '{filePath}'.", ex.InnerException);
            }
        }

        /// <summary>
        /// Deserializes XML text into a modeled object
        /// </summary>
        /// <typeparam name="T">The generic object type to serialize into</typeparam>
        /// <param name="toDeserialize">The XML text to deserialize</param>
        /// <returns>The object that was deserialized from XML</returns>
        /// <exception cref="XmlDeserializationException">Custom exception surfacing any errors that occur during runtime</exception>
        public static T? Deserialize<T>(string toDeserialize)
        {
            try
            {
                var deserializer = new XmlSerializer(typeof(T));
                using var reader = new StringReader(toDeserialize);
                return (T?)deserializer.Deserialize(reader);
            }
            catch (InvalidOperationException ex)
            {
                // Handle any attempted invalid operations
                throw new XmlDeserializationException($"Invalid operation attempted while deserializing object from XML: {ex.Message}", ex.InnerException);
            }
            catch (Exception ex)
            {
                // Handle other issues
                throw new XmlDeserializationException($"Unexpected error: {ex.Message}", ex.InnerException);
            }
        }

        /// <summary>
        /// Deserializes an XML file into a modeled object
        /// </summary>
        /// <typeparam name="T">The generic object type to serialize into</typeparam>
        /// <param name="filePath">The target path at which to find the XML to deserialize</param>
        /// <returns>The object that was deserialized from XML</returns>
        /// <exception cref="ArgumentException">Thrown if the target file path is passed in as null</exception>
        /// <exception cref="XmlDeserializationException">Catches any IO errors</exception>
        public static T? DeserializeFromFile<T>(string filePath)
        {
            // Error if no file path is provided
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            try
            {
                string fileContent = File.ReadAllText(filePath, Encoding.UTF8);
                return Deserialize<T>(fileContent);
            }
            catch (IOException ex)
            {
                // Handle any IO errors during runtime
                throw new XmlDeserializationException($"IO error during deserialization from file '{filePath}'.", ex.InnerException);
            }
        }

        /// <summary>
        /// Extracts an XML substring from an XML file and manipulates its object representation
        /// </summary>
        /// <typeparam name="T">The target XML object you want to manipulate</typeparam>
        /// <param name="filePath">The path to your target XML file</param>
        /// <param name="manipulateObject">The object manipulation you wish to perform</param>
        /// <exception cref="InvalidOperationException"></exception>
        public static void ProcessXmlBlockFromFile<T>(string filePath, Action<T?> manipulateObject)
        {
            string xmlContent = File.ReadAllText(filePath);

            string propertyName = typeof(T).Name;

            string? block = ExtractXmlBlock(xmlContent, propertyName);

            if (string.IsNullOrEmpty(block))
            {
                throw new InvalidOperationException("The specified XML block was not found: " + propertyName);
            }

            T? obj = Deserialize<T>(block);

            manipulateObject(obj);

            if (obj == null)
            {
                throw new InvalidOperationException("Object manipulation resulted in null object");
            }

            string newBlock = Serialize(obj);

            // Preserve the original indentation context
            string indentedNewBlock = PreserveIndentation(xmlContent, block, newBlock);

            string newXmlContent = xmlContent.Replace(block, indentedNewBlock);

            File.WriteAllText(filePath, newXmlContent);
        }

        /// <summary>
        /// Extracts a specific XML block from within an XML emcoded string
        /// </summary>
        /// <param name="xmlContent">The XML string you are manipulating</param>
        /// <param name="propertyName">The property name you wish to extract</param>
        /// <returns>A substring containing the target XML block</returns>
        public static string? ExtractXmlBlock(string xmlContent, string propertyName)
        {
            string startTag = "<" + propertyName + ">";
            string endTag = "</" + propertyName + ">";

            int startIndex = xmlContent.IndexOf(startTag);
            if (startIndex == -1)
            {
                return null;
            }
            int endIndex = xmlContent.IndexOf(endTag, startIndex) + endTag.Length;
            if (endIndex == -1)
            {
                return null;
            }
            return xmlContent.Substring(startIndex, endIndex - startIndex);
        }

        /// <summary>
        /// Preserves the original indentation context when replacing an XML block
        /// </summary>
        /// <param name="originalXmlContent">The original XML content</param>
        /// <param name="originalBlock">The original XML block that was extracted</param>
        /// <param name="newBlock">The new XML block to replace it with</param>
        /// <returns>The new XML block with preserved indentation</returns>
        private static string PreserveIndentation(string originalXmlContent, string originalBlock, string newBlock)
        {
            // Find the indentation of the original block
            int blockStartIndex = originalXmlContent.IndexOf(originalBlock);
            if (blockStartIndex == -1)
            {
                return newBlock;
            }

            // Look backwards from the block start to find the beginning of the line
            int lineStartIndex = blockStartIndex;
            while (lineStartIndex > 0 && originalXmlContent[lineStartIndex - 1] != '\n' && originalXmlContent[lineStartIndex - 1] != '\r')
            {
                lineStartIndex--;
            }

            // Extract the indentation (whitespace between line start and block start)
            string indentation = originalXmlContent.Substring(lineStartIndex, blockStartIndex - lineStartIndex);

            // Only preserve if it's actually whitespace (spaces or tabs)
            if (string.IsNullOrEmpty(indentation) || !indentation.All(c => char.IsWhiteSpace(c)))
            {
                return newBlock;
            }

            // Apply the same indentation to each line of the new block
            var lines = newBlock.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length <= 1)
            {
                return newBlock;
            }

            var indentedLines = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (i == 0)
                {
                    // First line keeps original indentation from context
                    indentedLines.Add(lines[i]);
                }
                else
                {
                    // Subsequent lines get the base indentation plus their relative indentation
                    string trimmedLine = lines[i].TrimStart();
                    if (!string.IsNullOrEmpty(trimmedLine))
                    {
                        // Count the original indentation in this line
                        int originalIndentCount = lines[i].Length - trimmedLine.Length;
                        string relativeIndent = new string('\t', originalIndentCount);
                        indentedLines.Add(indentation + relativeIndent + trimmedLine);
                    }
                    else
                    {
                        indentedLines.Add(string.Empty);
                    }
                }
            }

            return string.Join(Environment.NewLine, indentedLines);
        }
    }

    /// <summary>
    /// String writer to enforce UTF-8 encoding
    /// </summary>
    public sealed class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder sb) : base(sb) { }
        public override Encoding Encoding => Encoding.UTF8;
    }

    /// <summary>
    /// Custom exception for XML serialization errors
    /// </summary>
    public class XmlSerializationException : Exception
    {
        public XmlSerializationException(string message, Exception ?innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Custom exception for XML deserialization errors
    /// </summary>
    public class XmlDeserializationException : Exception
    {
        public XmlDeserializationException(string message, Exception ?innerException)
            : base(message, innerException)
        { }
    }
}
