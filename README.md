# jmsudar.DotNet.Xml

jmsudar.DotNet.Xml is a .NET library providing efficient and easy-to-use XML serialization and deserialization methods with no external third-party dependencies.

## Features

- Robust XML serialization and deserialization.
- Support for serializing to and deserializing from both strings and files.
- Integration of XML namespaces during serialization.
- Custom exception handling for detailed error feedback during serialization and deserialization.

## Getting Started

### Installation

To install the jmsudar.DotNet.Json library, use the following NuGet command:

```
dotnet add package jmsudar.DotNet.Xml
```

Alternately, find the file through the NuGet explorer in Visual Studio.

## Usage

Here's a quick example to get you started:

```csharp
using System;
using jmsudar.DotNet.Xml;

public class Program
{
    public class ExampleObject
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public static void Main()
    {
        var myObject = new ExampleObject { Name = "Jane Doe", Age = 29 };
        string xmlString = XML.Serialize(myObject);
        Console.WriteLine(xmlString);

        var deserializedObject = XML.Deserialize<ExampleObject>(xmlString);
        Console.WriteLine($"Name: {deserializedObject.Name}, Age: {deserializedObject.Age}");
    }
}
```

## License

This project is licensed under the GPL-3.0 License - see the [LICENSE](LICENSE) file for details.

## Contact

JMSudar - [code.jmsudar@gmail.com](mailto:code.jmsudar@gmail.com)

Project Link - [https://github.com/jmsudar/DotNet-XML](https://github.com/jmsudar/DotNet-XML)