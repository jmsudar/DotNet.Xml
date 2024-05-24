using System.Text;
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
        public static string Serialize<T>(T toSerialize, XmlSerializerNamespaces? namespaces = null)
        {
            // Error if the object to serialize is null
            if (toSerialize == null) throw new ArgumentNullException(nameof(toSerialize), "Input object to serialize cannot be null.");

            try
            {
                var serializer = new XmlSerializer(typeof(T));
                var builder = new StringBuilder();

                using (var writer = new Utf8StringWriter(builder))
                {
                    serializer.Serialize(writer, toSerialize, namespaces);
                }

                return builder.ToString();
            }
            catch (InvalidOperationException ex)
            {
                // Handle any attempted invalid operations
                throw new XmlSerializationException($"Invalid operation attempted while serializing object to XML: {ex.Message}", ex.InnerException);
            }
            catch (Exception ex)
            {
                // Handle other issues
                throw new XmlSerializationException($"Unexpected error: {ex.Message}", ex.InnerException);
            }
        }

        /// <summary>
        /// Serializes an object to a given file path
        /// </summary>
        /// <typeparam name="T">The generic object type to serialize from</typeparam>
        /// <param name="toSerialize">The object you are serializing</param>
        /// <param name="filePath">The file path location for the serialized XML</param>
        /// <param name="namespaces">Optional XML serializer namespaces to include</param>
        /// <exception cref="ArgumentNullException">Thrown if the destination file path is passed in as null</exception>
        /// <exception cref="XmlSerializationException">Catches any IO errors</exception>
        public static void SerializeToFile<T>(T toSerialize, string filePath, XmlSerializerNamespaces? namespaces = null)
        {
            // Error if no file path is provided
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));

            try
            {
                string serializedData = Serialize(toSerialize, namespaces);
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
