using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using DogWalkerAPI.Models;

namespace DogWalkerAPI.Data
{
    class WalkerRepository
    {
        public SqlConnection Connection
        {
            get
            {
                // This is "address" of the database
                string _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=LordsOfDogtown;Integrated Security=True";
                return new SqlConnection(_connectionString);
            }
        }
            public List<Walker> GetAllWalkers()
        {
            using (SqlConnection conn = Connection)
            {
                // Note, we must Open() the connection, the "using" block   doesn't do that for us.
                conn.Open();

                // We must "use" commands too.
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    // Here we setup the command with the SQL we want to execute before we execute it.
                    cmd.CommandText = "SELECT Id, Name, NeighborhoodId FROM Walker";

                    // Execute the SQL in the database and get a "reader" that will give us access to the data.
                    SqlDataReader reader = cmd.ExecuteReader();

                    // A list to hold the departments we retrieve from the database. 
                    List<Walker> walkers = new List<Walker>();

                    // Read() will return true if there's more data to read
                    while (reader.Read())
                    {
                        // The "ordinal" is the numeric position of the column in the query results.
                        //  For our query, "Id" has an ordinal value of 0 and "FirstName" is 1.
                        int idColumnPosition = reader.GetOrdinal("Id");

                        // We user the reader's GetXXX methods to get the value for a particular ordinal.
                        int idValue = reader.GetInt32(idColumnPosition);

                        int nameColumnPosition = reader.GetOrdinal("Name");
                        string NameValue = reader.GetString(nameColumnPosition);

                        int neighborhoodColumnPosition = reader.GetOrdinal("NeighborhoodId");
                        int NeighborhoodValue = reader.GetInt32(neighborhoodColumnPosition);

                        // Now let's create a new Walker object using the data from the database.
                        Walker walker = new Walker
                        {
                            Id = idValue,
                            Name = NameValue,
                            NeighborhoodId = NeighborhoodValue
                        };

                        // ...and add that Walker object to our list.
                        walkers.Add(walker);
                    }

                    // We should Close() the reader. Unfortunately, a "using" block won't work here.
                    reader.Close();

                    // Return the list of walkers who whomever called this method.
                    return walkers;
                }
            }
        }
        public Walker GetWalkerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, NeighborhoodId FROM Walker WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    // If we only expect a single row back from the database, we don't need a while loop.
                    if (reader.Read())
                    {
                        walker = new Walker
                        {
                            Id = id,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };
                    }

                    reader.Close();

                    return walker;
                }
            }
        }
        public Walker GetWalkerByNeighborhoodId(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, NeighborhoodId FROM Walker WHERE NeighborhoodId = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    // If we only expect a single row back from the database, we don't need a while loop.
                    if (reader.Read())
                    {
                        walker = new Walker
                        {
                            Id = id,
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };
                    }

                    reader.Close();

                    return walker;
                }
            }
        }
        public List<Walker> GetAllWalkersByNeighborhoodId(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT w.Id, w.Name, w.NeighborhoodId, n.Name as 'Neighborhood Name' FROM Walker w LEFT JOIN Neighborhood n on w.NeighborhoodId = n.Id WHERE n.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Walker> allNeighborhoodWalkers = new List<Walker>();

                    Walker walker = null;

                    while (reader.Read())

                    {
                        int idColumnPosition = reader.GetOrdinal("Id");
                        int IdValue = reader.GetInt32(idColumnPosition);

                        int NameColumnPosition = reader.GetOrdinal("Name");
                        string NameValue = reader.GetString(NameColumnPosition);

                        int neighborhoodColumnPosition = reader.GetOrdinal("Neighborhood Name");
                        string neighborhoodValue = reader.GetString(neighborhoodColumnPosition);

                        int neighborhoodIdColumnPosition = reader.GetOrdinal("neighborhoodId");
                        int neighborhoodIdValue = reader.GetInt32(neighborhoodIdColumnPosition);

                        walker = new Walker
                        {
                            Id = IdValue,
                            Name = NameValue,
                            NeighborhoodId = neighborhoodIdValue,
                            Neighborhood = new Neighborhood
                            {
                                Name = neighborhoodValue,
                                Id = neighborhoodIdValue
                            }
                        };
                        allNeighborhoodWalkers.Add(walker);
                    }

                    reader.Close();
                    return allNeighborhoodWalkers;
                }
            }
        }


        public void AddWalker(Walker walker)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = "INSERT INTO Walker (Name, NeighborhoodId) OUTPUT INSERTED.Id Values (@Name, @NeighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@Name", walker.Name));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", walker.NeighborhoodId));

                    int id = (int)cmd.ExecuteScalar();

                    walker.Id = id;
                }
            }
        }

    }
}
