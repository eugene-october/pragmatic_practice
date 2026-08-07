using Google.Protobuf;

Console.WriteLine("Hello there!");
address_book.Entrypoint.RunAddressBookUserStory();

namespace address_book
{
    public class Formatter
    {
        public static byte[] Encode(AddressBook obj)
        {
            return obj.ToByteArray();
        }

        public static AddressBook Decode(byte[] binaryRepresentation)
        {
            return AddressBook.Parser.ParseFrom(binaryRepresentation);
        }
    }

    public class Entrypoint
    {
        public static void RunAddressBookUserStory()
        {
            AddressBook ab = new AddressBook
            {
                Name = "John",
                Address = "Wall Street",
                Phone = "1234561111",
                Age = 42
            };

            byte[] encoded = Formatter.Encode(ab);
            Console.WriteLine($"encoded: {encoded}, length: {encoded.Count()}");

            AddressBook decoded = Formatter.Decode(encoded);
            Console.WriteLine($"decoded: {decoded}");
        }

        public static void RunTrivialBinaryWriter()
        {
            string filePath = "data.bin";

            // Create and write to the binary file
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.Create)))
            {
                writer.Write(42);                  // Writes a 4-byte integer
                writer.Write(3.14159);             // Writes an 8-byte double
                writer.Write(true);                // Writes a 1-byte boolean
                writer.Write("Hello World");       // Writes a length-prefixed string
            }
        }
    }
}

