using Grpc.Core;
using Grpc.Net.Client;
using GrpcServer;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GrpcClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Local server address.
            var channel = GrpcChannel.ForAddress("https://localhost:5001");
            // Instantiate our server.
            var client = new BookSearch.BookSearchClient(channel);


            
            using (var call = client.GetBookInfo(new BookSearchRequest()))          // Our request
            {
                while (await call.ResponseStream.MoveNext())
                {
                    var book = call.ResponseStream.Current;

                    Console.WriteLine($"{ book.Title }, { book.Author }: { book.Price } ");

                }
            }



            Console.ReadLine();
        }
    }
}
