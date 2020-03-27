using DogWalkerAPI.Models;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace DogWalkerAPI.Data
{
    class NeighborhoodRepository
    {
        public SqlConnection Connection
        {
            get
            {
                string _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=LordsOfDogtown;Integrated Security=True";
                return new SqlConnection(_connectionString);
            }
        }
        public List<Neighborhood> GetAllNeighborhoods()
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Id, Name FROM Neighborhood";

                SqlDataReader reader = cmd.ExecuteReader();

                List<Neighborhood> neighborhoods = new List<Neighborhood>();

                while (reader.Read())
                {

                    int idColumnPosition = reader.GetOrdinal("Id");
                    int idValue = reader.GetInt32(idColumnPosition);

                    int nameColumnPosition = reader.GetOrdinal("Name");
                    string nameValue = reader.GetString(nameColumnPosition);

                    Neighborhood neighborhood = new Neighborhood
                    {
                        Id = idValue,
                        Name = nameValue
                    };

                    neighborhoods.Add(neighborhood);
                }

                reader.Close();

                return neighborhoods;
            }
        }
    }
}