using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GrpcServer
{
    public class DBCommsController
    {

        /// <summary>
        /// Construct and executes book search query.
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="my_query"></param>
        /// <returns></returns>
        public static List<BookModel> handleQuery(OracleConnection conn, string[] query_words)
        {
            
            // Create a Sql query as a string. Query will insert each word of 'query_words' into a temporary table.
            string startCmdText = "BEGIN ";
            string mainCmdText = "";
            string procedureCmdText = "search_proc(); ";
            string endCmdText = "END;";

            string mainText;
            foreach (string word in query_words)
            {
                mainText = $"INSERT INTO temp_search_table VALUES ('{word}'); ";

                mainCmdText += mainText;

            }
            
            string commandText = startCmdText + mainCmdText + procedureCmdText + endCmdText;
            
            // Begin execute. 
            OracleCommand cmd1 = new OracleCommand();
            cmd1.Connection = conn;
            cmd1.CommandText = commandText;
            int feed = cmd1.ExecuteNonQuery();
            // returns the number of rows affected, for the following: If the command is UPDATE, INSERT, or DELETE. For all other types -1.
            Console.WriteLine("FEED: " + feed);



            // Execute a user-defined function with a parameter of how many words were inserted into the temporary table.
            //      Returns book/s that match the query words which those matches are intersected together.
            OracleCommand cmd2 = new OracleCommand();
            cmd2.Connection = conn;
            cmd2.CommandText = "SELECT ID, title FROM TABLE(main_func(" + query_words.Length + "))";

            // Because database will return asynchronously and not in one chunk but many use 'Reader'.
            OracleDataReader reader = cmd2.ExecuteReader();
            Console.WriteLine("START READER");

            List<BookModel> selected_rows = new List<BookModel>();
            BookModel book;
            while (reader.Read())
            {
                Console.WriteLine("ID : " + reader.GetString(0) + " " + "Title : " + reader.GetString(1));

                // construct a 'BookModel' to comply with return type of this function.
                book = new BookModel
                {
                    Title = reader.GetString(1),
                    Author = "Null",
                    Price = 0.0
                };
                selected_rows.Add(book);

            }



            // Clear all records from temporary tables. Precaution for if user makes another search in the same session/ connection(DB). 
            OracleCommand cmd3 = new OracleCommand();
            cmd3.Connection = conn;
            cmd3.CommandText = "TRUNCATE TABLE temp_search_table";
            cmd3.ExecuteNonQuery();

            OracleCommand cmd4 = new OracleCommand();
            cmd4.Connection = conn;
            cmd4.CommandText = "TRUNCATE TABLE temp_results_table";
            cmd4.ExecuteNonQuery();


            // Clean up
            reader.Dispose();



            return selected_rows;
        }

    }
}
