using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using LetsDoc.Core.Models;

namespace LetsDoc.Core.VectorStore;

public class SqliteVectorStore : IVectorStore
{
    private readonly string _dbPath;
    private readonly string _extensionPath;

    public SqliteVectorStore(string dbPath, string extensionPath)
    {
        _dbPath = dbPath;
        _extensionPath = extensionPath;
        Initialize();
    }

    private void Initialize()
    {
        // We use a single connection for initialization to ensure the tables exist
        using var conn = GetConnection();
        
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            CREATE TABLE IF NOT EXISTS chunks (
                id TEXT PRIMARY KEY,
                docId TEXT,
                text TEXT,
                pageNumber INTEGER
            );
            
            -- sqlite-vec syntax for a 384-dimension vector table
            CREATE VIRTUAL TABLE IF NOT EXISTS vec_chunks USING vec0(
                id TEXT PRIMARY KEY,
                embedding float[384]
            );
        ";
        cmd.ExecuteNonQuery();
    }

    private SqliteConnection GetConnection()
    {
        var conn = new SqliteConnection($"Data Source={_dbPath}");
        conn.Open();

        // Required to run the .dylib on macOS
        conn.EnableExtensions(true);
        
        try 
        {
            conn.LoadExtension(_extensionPath);
        }
        catch (SqliteException ex)
        {
            throw new Exception($"Failed to load sqlite-vec extension from {_extensionPath}. Ensure the .dylib is in the root folder.", ex);
        }

        return conn;
    }

    public void AddVector(EmbeddingVector vec) 
    {
        // Supporting the interface method by wrapping it in the enumerable logic
        AddVectors(new[] { vec });
    }

    public void AddVectors(IEnumerable<EmbeddingVector> vectors)
    {
        using var conn = GetConnection();
        using var transaction = conn.BeginTransaction();

        // Optimized: Reuse commands within the loop
        var metaCmd = conn.CreateCommand();
        metaCmd.CommandText = "INSERT OR REPLACE INTO chunks (id, docId, text, pageNumber) VALUES ($id, $docId, $text, $pageNumber)";
        
        var vecCmd = conn.CreateCommand();
        vecCmd.CommandText = "INSERT OR REPLACE INTO vec_chunks (id, embedding) VALUES ($id, $embedding)";

        try
        {
            foreach (var vec in vectors)
            {
                // 1. Metadata
                metaCmd.Parameters.Clear();
                metaCmd.Parameters.AddWithValue("$id", vec.Id);
                metaCmd.Parameters.AddWithValue("$docId", vec.DocumentId);
                metaCmd.Parameters.AddWithValue("$text", vec.Text);
                metaCmd.Parameters.AddWithValue("$pageNumber", vec.PageNumber);
                metaCmd.ExecuteNonQuery();

                // 2. Vector (float[] -> byte[])
                vecCmd.Parameters.Clear();
                vecCmd.Parameters.AddWithValue("$id", vec.Id);
                
                var bytes = new byte[vec.Vector.Length * sizeof(float)];
                Buffer.BlockCopy(vec.Vector, 0, bytes, 0, bytes.Length);
                vecCmd.Parameters.AddWithValue("$embedding", bytes);
                vecCmd.ExecuteNonQuery();
            }
            transaction.Commit();
        }
        catch { transaction.Rollback(); throw; }
    }

    public List<EmbeddingVector> Search(float[] queryEmbedding, int topK = 5)
    {
        var results = new List<EmbeddingVector>();
        using var conn = GetConnection();

        var queryBytes = new byte[queryEmbedding.Length * sizeof(float)];
        Buffer.BlockCopy(queryEmbedding, 0, queryBytes, 0, queryBytes.Length);

        var cmd = conn.CreateCommand();
        // The 'k = $topK' is the specific ANN parameter for sqlite-vec
        cmd.CommandText = @"
            SELECT v.id, c.text, c.pageNumber, v.distance
            FROM vec_chunks v
            JOIN chunks c ON v.id = c.id
            WHERE v.embedding MATCH $query AND k = $topK
            ORDER BY distance;
        ";
        cmd.Parameters.AddWithValue("$query", queryBytes);
        cmd.Parameters.AddWithValue("$topK", topK);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new EmbeddingVector
            {
                Id = reader.GetString(0),
                Text = reader.GetString(1),
                PageNumber = reader.GetInt32(2),
                Distance = reader.GetDouble(3)
            });
        }
        return results;
    }
}