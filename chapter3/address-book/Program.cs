using System.Text.Json;
using Google.Protobuf;

Console.WriteLine("Hello there!");
address_book.Entrypoint.RunAddressBookUserStory();
address_book.Entrypoint.RunTrivialBinaryWriter();

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

    public class FormatterSimple
    {
        public static byte[] Encode(AddressBook obj)
        {
            byte[] byteArray = JsonSerializer.SerializeToUtf8Bytes(obj);

            return byteArray;
        }

        public static AddressBook Decode(byte[] binaryRepresentation)
        {
            AddressBook? decoded = JsonSerializer.Deserialize<AddressBook>(binaryRepresentation);

            return decoded!;
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
            AddressBook ab = new AddressBook
            {
                Name = "Jack",
                Address = "Black pearl",
                Phone = "23482342304",
                Age = 38
            };

            byte[] encoded = FormatterSimple.Encode(ab);
            Console.WriteLine($"encoded: {encoded}, length: {encoded.Count()}");

            AddressBook decoded = FormatterSimple.Decode(encoded);
            Console.WriteLine($"decoded: {decoded}");
        }
    }
}

