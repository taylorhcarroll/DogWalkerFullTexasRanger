using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Data.SqlClient;
using DogWalkerAPI.Models;

namespace DogWalkerAPI.Data
{
    class OwnerRepository
    {
        public SqlConnection Connection
        {
            get
            {
                string _connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=LordsOfDogtown;Integrated Security=True";
                return new SqlConnection(_connectionString);
            }
        }

        public List<Owner> GetAllOwners()
        {

            using (SqlConnection conn = Connection)
            {
                conn.Open();

                using SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, n.Name as 'Neighborhood Name' FROM Owner o LEFT JOIN Neighborhood n on o.NeighborhoodId = n.Id";
                SqlDataReader reader = cmd.ExecuteReader();

                List<Owner> owners = new List<Owner>();

                while (reader.Read())
                {

                    int idColumnPosition = reader.GetOrdinal("Id");
                    int idValue = reader.GetInt32(idColumnPosition);

                    int nameColumnPosition = reader.GetOrdinal("Name");
                    string nameValue = reader.GetString(nameColumnPosition);

                    int neighborhoodIdColumnPosition = reader.GetOrdinal("NeighborhoodId");
                    int neighborhoodIdValue = reader.GetInt32(neighborhoodIdColumnPosition);

                    int neighborhoodColumnPosition = reader.GetOrdinal("Neighborhood Name");
                    string neighborhoodValue = reader.GetString(neighborhoodColumnPosition);

                    Owner owner = new Owner
                    {
                        Id = idValue,
                        Name = nameValue,
                        NeighborhoodId = neighborhoodIdValue,
                        Neighborhood = new Neighborhood
                        {
                            Name = neighborhoodValue,
                            Id = neighborhoodIdValue
                        }
                    };
                    owners.Add(owner);
                }
                reader.Close();
                return owners;
            }
        }

        public Owner GetOwnerById(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT o.Id, o.Name, o.NeighborhoodId, n.Name as 'Neighborhood Name' FROM Owner o LEFT JOIN Neighborhood n on o.NeighborhoodId = n.Id WHERE o.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner newOwner = new Owner();

                    if (reader.Read())
                    {
                        int idColumnPosition = reader.GetOrdinal("Id");
                        int IdValue = reader.GetInt32(idColumnPosition);

                        int NameColumnPosition = reader.GetOrdinal("Name");
                        string NameValue = reader.GetString(NameColumnPosition);

                        int neighborhoodColumnPosition = reader.GetOrdinal("Neighborhood Name");
                        string neighborhoodValue = reader.GetString(neighborhoodColumnPosition);

                        int neighborhoodIdColumnPosition = reader.GetOrdinal("neighborhoodId");
                        int neighborhoodIdValue = reader.GetInt32(neighborhoodIdColumnPosition);

                        newOwner = new Owner
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
                    }

                    reader.Close();
                    return newOwner;
                }
            }
        }


        public void AddOwner(Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {

                    cmd.CommandText = "INSERT INTO Owner (Name, NeighborhoodId, Address, Phone) OUTPUT INSERTED.Id Values (@Name, @NeighborhoodId, @Address, @Phone)";
                    cmd.Parameters.Add(new SqlParameter("@Name", owner.Name));
                    cmd.Parameters.Add(new SqlParameter("@NeighborhoodId", owner.NeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@Phone", owner.Phone));
                    cmd.Parameters.Add(new SqlParameter("@Address", owner.Address));

                    int id = (int)cmd.ExecuteScalar();

                    owner.Id = id;
                }
            }
        }

        public void UpdateOwner(int id, Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"UPDATE Owner
                                     SET NeighborhoodId = @neighborhoodId
                                     WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}