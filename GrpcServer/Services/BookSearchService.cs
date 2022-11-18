using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;




namespace GrpcServer.Services
{
    public class BookSearchService : BookSearch.BookSearchBase
    {
        private readonly ILogger<BookSearchService> _logger;
        public BookSearchService(ILogger<BookSearchService> logger)
        {
            _logger = logger;
        }



        public override async Task GetBookInfo(BookSearchRequest request, IServerStreamWriter<BookModel> responseStream, ServerCallContext context)
        {
            // Get connection string from 'appsettings.json'.
            string oradb = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("ConnectionStrings")["Oradb"];
            // Create connection with Oracle database.
            OracleConnection conn = new OracleConnection(oradb);
            conn.Open();
            Console.WriteLine();
            Console.WriteLine("Connected to Oracle" + conn.ServerVersion);
            Console.WriteLine();




            string[] qeury_words = { "Harry", "Potter" };
            List<BookModel> books = DBCommsController.handleQuery(conn, qeury_words);
            foreach (var book in books)
            {
                await responseStream.WriteAsync(book);

            }


        }




    }
}
