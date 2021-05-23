using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KmyKeiba.Models.Data
{
  class DatabaseConfigManager
  {
    public bool IsMigrated { get; private set; }

    public DatabaseConfig Config { get; private set; }

    public static DatabaseConfig GetCurrentConfigFile()
    {
      try
      {
        var lines = File.ReadAllLines(@"./database.txt");
        var data = lines
          .Select((l) => l.Split("="))
          .Where((l) => l.Length >= 2)
          .ToDictionary((l) => l[0].ToLower().Trim(), (l) => l[1].Trim());
        return new()
        {
          Host = data["host"],
          Database = data["database"],
          UserName = data["username"],
          Password = data["password"],
        };
      }
      catch
      {
        return new();
      }
    }

    public DatabaseConfigManager()
    {
      this.Config = GetCurrentConfigFile();
    }

    public async Task<bool> TryMigrateAsync()
    {
      if (this.IsMigrated)
      {
        return true;
      }

      try
      {
        // データベースがなければ自動的に作成する
        // 今後、DBがあるかないかで処理分けるかもしれないので、IF NOT EXISTSは使わない
        using (var connection = new MySqlConnection(this.Config.GetConnectionStringWithoutDatabase()))
        {
          await connection.OpenAsync();
          var isDatabaseExists = false;

          using (var cmd = connection.CreateCommand())
          {
            cmd.CommandText = "SHOW DATABASES;";
            using (var reader = await cmd.ExecuteReaderAsync())
            {
              while (await reader.ReadAsync() && !isDatabaseExists)
              {
                string row = string.Empty;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                  row += reader.GetValue(i).ToString();
                }
                if (row == this.Config.Database)
                {
                  isDatabaseExists = true;
                }
              }
            }
          }

          if (!isDatabaseExists)
          {
            using (var cmd = connection.CreateCommand())
            {
              cmd.CommandText = @$"CREATE DATABASE `{this.Config.Database}` CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;";
              await cmd.ExecuteNonQueryAsync();
            }
          }
        }

        // マイグレーション
        using var db = new MyContext();
        await db.Database.MigrateAsync();
        this.IsMigrated = true;
        return true;
      }
      catch (Exception ex)
      {
        return false;
      }
    }

    public async Task<bool> UpdateSettingAsync(DatabaseConfig config)
    {
      try
      {
        this.IsMigrated = false;
        this.Config = config;
        await File.WriteAllTextAsync(@"./database.txt", config.ToString());
        return await this.TryMigrateAsync();
      }
      catch
      {
        return false;
      }
    }
  }

  public class DatabaseConfig
  {
    public string Host { get; init; } = string.Empty;

    public string Database { get; init; } = string.Empty;

    public string UserName { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string GetConnectionString()
    {
      return $@"server={this.Host};database={this.Database};uid={this.UserName};pwd={this.Password};";
    }

    public string GetConnectionStringWithoutDatabase()
    {
      return $@"server={this.Host};uid={this.UserName};pwd={this.Password};";
    }

    public override string ToString()
    {
      return @$"host={this.Host}
database={this.Database}
username={this.UserName}
password={this.Password}";
    }
  }
}
