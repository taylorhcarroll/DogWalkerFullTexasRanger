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
    public class WalkerController : ControllerBase
    {
        private readonly IConfiguration _config;
        public WalkerController(IConfiguration config)
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
             [FromQuery] int? neighborhoodId )
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT Id, Name, NeighborhoodId FROM Walker WHERE 1 = 1";
                    if (neighborhoodId != null)
                    {
                        cmd.CommandText += " AND NeighborhoodId LIKE @neighborhoodId";
                        cmd.Parameters.Add(new SqlParameter("@neighborhoodId", "%" + neighborhoodId + "%"));
                    }
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Walker> walkers = new List<Walker>();

                    while (reader.Read())
                    {
                        Walker walker = new Walker
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId"))
                        };
                        walkers.Add(walker);
                    }
                    reader.Close();

                    return Ok(walkers);
                }
            }
        }

        //Get by ID
        [HttpGet("{id}", Name = "GetWalker")]
        public async Task<IActionResult> Get(
            [FromRoute] int id,
            [FromQuery] string include)
        {
            if (include != "walks") 
            {
                var walker = GetWalker(id);
                return Ok(walker);
            } else
            {
                var walker = GetWalkerWithWalks(id);
                return Ok(walker);
            }
        }
        //Post
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Walker walker)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT INTO Walker (Name, NeighborhoodId)
                                        OUTPUT INSERTED.Id
                                        VALUES (@name, @neighborhoodId)";
                    cmd.Parameters.Add(new SqlParameter("@name", walker.Name));
                    cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));
                    int newId = (int)cmd.ExecuteScalar();
                    walker.Id = newId;
                    return CreatedAtRoute("GetWalker", new { id = newId }, walker);
                }
            }
        }
        //Put
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] int id, [FromBody] Walker walker)
        {
            try
            {
                using (SqlConnection conn = Connection)
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        {
                            cmd.CommandText = @"UPDATE Walker
                                                SET Name = @name,
                                                NeighborhoodId = @neighborhoodId
                                                WHERE Id = @id";
                            cmd.Parameters.Add(new SqlParameter("@name", walker.Name));
                            cmd.Parameters.Add(new SqlParameter("@neighborhoodId", walker.NeighborhoodId));
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
                if (!WalkerExists(id))
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
                        cmd.CommandText = @"DELETE FROM Walker WHERE Id = @id";
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
                if (!WalkerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        //Method to check if Walkergo Exists
        private bool WalkerExists(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT Id, Name, NeighborhoodId
                        FROM Walker
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));

                    SqlDataReader reader = cmd.ExecuteReader();
                    return reader.Read();
                }
            }
        }
        private Walker GetWalker(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT
                        Id, Name, NeighborhoodId
                        FROM Walker
                        WHERE Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    if (reader.Read())
                    {
                        walker = new Walker
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Name = reader.GetString(reader.GetOrdinal("Name")),
                            NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")
                        )
                        };
                    }
                    reader.Close();
                    return walker;
                }
            }
        }
        private Walker GetWalkerWithWalks(int id)
        {
            using (SqlConnection conn = Connection)
            {
                conn.Open();
                using (SqlCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = @"
                        SELECT wr.Id, wr.Name, wr.NeighborhoodId, ws.Id AS WalksId, ws.Date, ws.Duration, ws.WalkerId, ws.DogId
                        FROM Walker wr
                        LEFT JOIN Walks ws ON wr.Id = ws.WalkerId
                        WHERE wr.Id = @id";
                    cmd.Parameters.Add(new SqlParameter("@id", id));
                    SqlDataReader reader = cmd.ExecuteReader();

                    Walker walker = null;

                    while (reader.Read())
                    {
                        //if statement checks to see if walker is null, it does not create a new one each time it loops through while loop
                        //this keeps it from creating a new walker each time
                        if (walker == null)
                        {
                            walker = new Walker
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                Name = reader.GetString(reader.GetOrdinal("Name")),
                                NeighborhoodId = reader.GetInt32(reader.GetOrdinal("NeighborhoodId")),
                                Walks = new List<Walks>()
                            };
                        }
                            walker.Walks.Add(new Walks()
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("WalksId")),
                                Date = reader.GetDateTime(reader.GetOrdinal("Date")),
                                Duration = reader.GetInt32(reader.GetOrdinal("Duration")),
                                WalkerId = reader.GetInt32(reader.GetOrdinal("WalkerId")),
                                DogId = reader.GetInt32(reader.GetOrdinal("dogId"))
                            });
                    }
                    reader.Close();
                    return walker;
                }
            }
        }
    }
}