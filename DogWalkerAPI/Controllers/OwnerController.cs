﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Microsoft.Data.SqlClient;
using DogWalkerAPI.Models;
using Microsoft.AspNetCore.Http;
using DogWalkerAPI.Data;

namespace DogWalkerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OwnerController : ControllerBase
    {
        private readonly IConfiguration _config;
        public OwnerController(IConfiguration config)
        {
            _config = config;
        }

        public SqlConnection Connection
        {
            get
            {
                return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            }
        }
        //Get All
        [HttpGet]
        public async Task<IActionResult> Get(
            [FromQuery] string include,
            [FromQuery] string q)
        {
            if (include != "neighborhood")
            {
                var owner = GetOwners(q);
                return Ok(owner);
            }
            else
            {
                var owner = GetOwnersWithNeighborhood(q);
                return Ok(owner);
            }
            //using (SqlConnection conn = Connection)
            //{
            //    conn.Open();
            //    using (SqlCommand cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = "SELECT Id, Name, Address, NeighborhoodId, Phone FROM Owner";
            //        SqlDataReader reader = cmd.ExecuteReader();
            //        List<Owner> owners = new List<Owner>();

            //        while (reader.Read())
            //        {
            //            Owner owner = new Owner
            //            {
            //                Id = reader.GetInt32(reader.GetOrdinal("Id")),
            //                Name = reader.GetString(reader.GetOrdinal("Name")),
            //                Address = reader.GetString(reader.GetOrdinal("Address")),
            //                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
            //                Phone = reader.GetString(reader.GetOrdinal("Phone"))
            //            };
            //            owners.Add(owner);
            //        }
            //        reader.Close();

            //        return Ok(owners);
            //    }
            //}
        }

        //Get by ID
        [HttpGet("{id}", Name = "GetOwner")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string include)
        {
            if (include != "neighborhood")
            {
                var owner = GetOwnerWithNeighborhood(id);
                return Ok(owner);
            }
            else
            {
                var owner = GetOwner(id);
                return Ok(owner);
            }
            //using (SqlConnection conn = Connection)
            //{
            //    conn.Open();
            //    using (SqlCommand cmd = conn.CreateCommand())
            //    {
            //        cmd.CommandText = @"SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone, n.Name NeighborhoodName, d.Id DogId ,d.Name DogName, d.Breed, d.Notes
            //            FROM Owner o
            //            Left Join  Neighborhood n
            //            On o.NeighborhoodId = n.Id
            //            Left Join Dog d
            //            On o.Id = d.OwnerId
            //            Where o.Id = @id";
            //        cmd.Parameters.Add(new SqlParameter("@id", id));
            //        SqlDataReader reader = cmd.ExecuteReader();

            //        Owner owner = null;

            //        if (reader.Read())
            //        {
            //            owner = new Owner
            //            {
            //                Id = reader.GetInt32(reader.GetOrdinal("Id")),
            //                Name = reader.GetString(reader.GetOrdinal("Name")),
            //                Address = reader.GetString(reader.GetOrdinal("Address")),
            //                Phone = reader.GetString(reader.GetOrdinal("Phone")),
            //                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
            //                Neighborhood = new Neighborhood()
            //                {
            //                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
            //                    Name = reader.GetString(reader.GetOrdinal("NeighborhoodName")),
            //                },
            //                Dogs = new List<Dog>()
            //            };
            //        }
            //        owner.Dogs.Add(new Dog()
            //        {
            //            Id = reader.GetInt32(reader.GetOrdinal("DogId")),
            //            OwnerId = reader.GetInt32(reader.GetOrdinal("Id")),
            //            Name = reader.GetString(reader.GetOrdinal("DogName")),
            //            Breed = reader.GetString(reader.GetOrdinal("Breed")),
            //            Notes = reader.GetString(reader.GetOrdinal("Notes"))
            //        });
            //    reader.Close();
            //    return Ok(owner);
            //    }
            //}
        }
        //Post
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Owner owner)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Owner (Name, Address, NeighborhoodId, Phone)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @address, @neighborhoodId, @phone)";
                    cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
                    cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                    cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));
                    int newId = (int)cmd.ExecuteScalar();
                    owner.Id = newId;
                    return CreatedAtRoute("GetOwner", new { id = newId }, owner);
                }
            }
        }
        //Put
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Owner owner)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        {
                            cmd.CommandText = @"UPDATE Owner
                                            SET Name = @name,
                                            Address = @address,
                                            NeighborhoodId = @neighborhoodId,
                                            Phone = @phone
                                            WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@name", owner.Name));
                            cmd.Parameters.Add(new SqlParameter("@address", owner.Address));
                            cmd.Parameters.Add(new SqlParameter("@neighborhoodId", owner.NeighborhoodId));
                            cmd.Parameters.Add(new SqlParameter("@phone", owner.Phone));
                            cmd.Parameters.Add(new SqlParameter("@id", id));

                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                return new StatusCodeResult(StatusCodes.Status204NoContent);
                            }
                            throw new Exception("No Rows Affected");
                        }
                    }
                }
            }
            catch (Exception)
            {
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        //Delete
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = @"DELETE FROM Owner WHERE Id = @id";
                        cmd.Parameters.Add(new SqlParameter("@id", id));

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            return new StatusCodeResult(StatusCodes.Status204NoContent);
                        }
                        throw new Exception("No rows affected");
                    }
                }
            }
            catch (Exception)
            {
                if (!OwnerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        //Method to check if Owner Exists
        private bool OwnerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        Select d.Id, d.Name, d.OwnerId, d.Breed, d.Notes, o.Name AS OwnerName, o.NeighborhoodId
                            FROM Owner d
                            Left Join Owner o
                            On d.OwnerId = o.Id
                            WHERE d.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }

        //get all Owners
        private List<Owner> GetOwners(string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, Address, NeighborhoodId, Phone FROM Owner";
                    if (q != null)
                    {
                        cmd.CommandText += " AND Name LIKE @name";
                        cmd.Parameters.Add(new SqlParameter("@name", "%" + q + "%"));
                    }

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Owner> owners = new List<Owner>();

                    while (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone"))
                        };
                        owners.Add(owner);
                    }
                    reader.Close();
                    return owners;
                }
            }
        }

        //get singular
        private Owner GetOwner(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone, n.Name NeighborhoodName, d.Id DogId ,d.Name DogName, d.Breed, d.Notes
                        FROM Owner o
                        Left Join  Neighborhood n
                        On o.NeighborhoodId = n.Id
                        Left Join Dog d
                        On o.Id = d.OwnerId
                        Where o.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    if (reader.Read())
                    {
                        owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Neighborhood = new Neighborhood()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Name = reader.GetString(reader.GetOrdinal("NeighborhoodName")),
                            },
                            Dogs = new List<Dog>()
                        };
                    }
                    owner.Dogs.Add(new Dog()
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("DogId")),
                        OwnerId = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("DogName")),
                        Breed = reader.GetString(reader.GetOrdinal("Breed")),
                        Notes = reader.GetString(reader.GetOrdinal("Notes"))
                    });
                    reader.Close();
                    return owner;
                }
            }
        }
        
        //get singular with neighborhood
        private List<Owner> GetOwnersWithNeighborhood(string q)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                       SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone, n.Id, n.Name AS Neighborhood
                       FROM Owner o
                       LEFT JOIN Neighborhood n ON o.NeighborhoodId = n.Id
                       WHERE 1=1";
                    if (q != null)
                    {
                        cmd.CommandText += " AND o.Name LIKE @name";
                        cmd.Parameters.Add(new SqlParameter("@name", "%" + q + "%"));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();

                    List<Owner> owners =  new List<Owner>();

                    while (reader.Read())
                    {
                        Owner owner = new Owner
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            Address = reader.GetString(reader.GetOrdinal("Address")),
                            Phone = reader.GetString(reader.GetOrdinal("Phone")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                            Neighborhood = new Neighborhood()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Name = reader.GetString(reader.GetOrdinal("Neighborhood"))
                            }
                        };
                        owners.Add(owner);
                    }
                    reader.Close();
                    return owners;
                }
            }
        }

        //get owners plural with the neighborhood left join
        private Owner GetOwnerWithNeighborhood(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT o.Id, o.Name, o.Address, o.NeighborhoodId, o.Phone, n.Id, n.Name AS Neighborhood 
                        FROM Owner o
                        LEFT JOIN Neighborhood n ON o.NeighborhoodID = n.Id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Owner owner = null;

                    while (reader.Read())
                    {
                        //if statement checks to see if owner is null, it does not create a new one each time it loops through while loop
                        //this keeps it from creating a new owner each time
                        if (owner == null)
                        {
                            owner = new Owner
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                Address = reader.GetString(reader.GetOrdinal("Address")),
                                Phone = reader.GetString(reader.GetOrdinal("Phone")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Neighborhood = new Neighborhood()
                                {
                                    Id = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                    Name = reader.GetString(reader.GetOrdinal("Neighborhood"))
                                }
                            };
                        }
                    }
                        reader.Close();
                        return owner;
                }
            }
        }    
    }
}