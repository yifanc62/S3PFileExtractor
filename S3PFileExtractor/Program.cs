using System;
using System.IO;
using System.Text;

namespace S3PFileExtractor {
    internal static class Program {
        private static void Main(string[] args) {
            if (args.Length == 0) {
                Console.Out.WriteLine("Usage: S3PFileExtractor.exe file [file...]");
                return;
            }
            var success = 0;
            foreach (var file in args) {
                if (!File.Exists(file)) {
                    Console.Error.WriteLine($"Error: File '{file}' not found.");
                    continue;
                }
                using (var reader = new BinaryReader(File.OpenRead(file))) {
                    var header = reader.ReadBytes(4);
                    if (Encoding.ASCII.GetString(header) != "S3P0") {
                        Console.Error.WriteLine($"Error: File '{file}' header not match.");
                        continue;
                    }
                    var folder = Path.ChangeExtension(file, null);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                    var count = Convert(reader.ReadBytes(4));
                    for (var i = 0; i < count; i++) {
                        var position = Convert(reader.ReadBytes(4));
                        var length = Convert(reader.ReadBytes(4));
                        var headPosition = reader.BaseStream.Position;
                        reader.BaseStream.Seek(position, SeekOrigin.Begin);
                        header = reader.ReadBytes(4);
                        if (Encoding.ASCII.GetString(header) != "S3V0") {
                            Console.Error.WriteLine($"Error: File '{file}' inner header {i} not match.");
                            continue;
                        }
                        var headerLength = Convert(reader.ReadBytes(4));
                        reader.BaseStream.Seek(headerLength - 8, SeekOrigin.Current);
                        var data = reader.ReadBytes(length - headerLength);
                        Console.Out.WriteLine($@"Writing {Path.GetFileName(folder)}\{i:D4}.wma");
                        File.WriteAllBytes($@"{folder}\{i:D4}.wma", data);
                        reader.BaseStream.Seek(headPosition, SeekOrigin.Begin);
                    }
                }
                success++;
            }
            Console.Out.WriteLine($"Done! {success} file{(success > 1 ? "s" : "")} extracted.");
            Console.ReadKey(true);
        }
        private static int Convert(byte[] content) {
            return content[0] | (content[1] << 8) | (content[2] << 16) | (content[3] << 24);
        }
    }
}