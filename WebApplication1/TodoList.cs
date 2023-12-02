using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.Data.Sqlite;

namespace WebApplication1
{
    public class TodoList
    {
        private readonly List<Task> tasks = new List<Task>();
        private int nextTaskId = 1;
        private const string JsonFilePath = "tasks.json";
        private const string XmlFilePath = "tasks.xml";
        private const string DbConnectionString = "Data Source=tasks.db";

        public void AddTask(Task task)
        {
            task.Id = nextTaskId++; // Устанавливаем уникальный идентификатор и увеличиваем счетчик
            tasks.Add(task);
        }

        public void RemoveTask(Task task)
        {
            tasks.Remove(task);
        }
        public void SaveChanges()
        {
            SaveTasksToJson();
            SaveTasksToXml();
            SaveTasksToSQLite();
        }


        public IEnumerable<Task> GetMostImportantTasks()
        {
            var uncompletedTasks = tasks.Where(t => !t.IsCompleted);

            if (uncompletedTasks.Any())
            {
                var highestPriority = uncompletedTasks.Max(t => t.Priority);
                return uncompletedTasks.Where(t => t.Priority == highestPriority).ToList();
            }
            else
            {
                return Enumerable.Empty<Task>();
            }
        }


        public IEnumerable<Task> GetNearestDeadlineTasks()
        {
            var uncompletedTasks = tasks.Where(t => !t.IsCompleted);

            if (uncompletedTasks.Any())
            {
                return uncompletedTasks.OrderBy(t => t.Deadline).ToList();
            }
            else
            {
                return Enumerable.Empty<Task>();
            }
        }


        public IEnumerable<Task> AllTasks()
        {
            return tasks;
        }

        public void SaveTasksToJson()
        {
            var json = JsonSerializer.Serialize(tasks);
            File.WriteAllText(JsonFilePath, json);
        }

        public void LoadTasksFromJson()
        {
            if (File.Exists(JsonFilePath))
            {
                var json = File.ReadAllText(JsonFilePath);

                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var deserializedTasks = JsonSerializer.Deserialize<List<Task>>(json);

                        if (deserializedTasks != null)
                        {
                            tasks.Clear();
                            tasks.AddRange(deserializedTasks);
                        }
                        else
                        {
                            // Обработка ситуации, когда десериализация вернула null
                            Console.WriteLine("Ошибка десериализации: результат равен null");
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Обработка ошибки десериализации JSON
                        Console.WriteLine($"Ошибка десериализации JSON: {ex.Message}");
                    }
                }
                else
                {
                    // Обработка ситуации, когда JSON пуст
                    Console.WriteLine("JSON-файл пуст");
                }
            }
            else
            {
                // Обработка ситуации, когда файл не существует
                Console.WriteLine($"Файл {JsonFilePath} не существует");
            }
        }


        public void SaveTasksToXml()
        {
            var serializer = new XmlSerializer(typeof(List<Task>));
            using (var stream = new StreamWriter(XmlFilePath))
            {
                serializer.Serialize(stream, tasks);
            }
        }

        public void LoadTasksFromXml()
        {
            if (File.Exists(XmlFilePath))
            {
                var serializer = new XmlSerializer(typeof(List<Task>));
                using (var stream = new StreamReader(XmlFilePath))
                {
                    try
                    {
                        var deserializedTasks = (List<Task>)serializer.Deserialize(stream)!;

                        tasks.Clear();
                        tasks.AddRange(deserializedTasks);
                    }
                    catch (InvalidOperationException ex)
                    {
                        // Обработка ошибки десериализации XML
                        Console.WriteLine($"Ошибка десериализации XML: {ex.Message}");
                    }
                }
            }
            else
            {
                // Обработка ситуации, когда файл не существует
                Console.WriteLine($"Файл {XmlFilePath} не существует");
            }
        }



        public void SaveTasksToSQLite()
        {
            using (var connection = new SqliteConnection(DbConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Tasks (
                    Title TEXT,
                    Priority INTEGER,
                    Deadline TEXT,
                    IsCompleted INTEGER
                )";

                    command.ExecuteNonQuery();

                    foreach (var task in tasks)
                    {
                        command.CommandText = $@"
                    INSERT INTO Tasks (Title, Priority, Deadline, IsCompleted)
                    VALUES ('{task.Title}', {task.Priority}, '{task.Deadline:yyyy-MM-dd}', {(task.IsCompleted ? 1 : 0)})";

                        command.ExecuteNonQuery();
                    }
                }
            }
        }


        public void LoadTasksFromSQLite()
        {
            using (var connection = new SqliteConnection(DbConnectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM Tasks";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(new Task
                            {
                                Title = reader.GetString(0),
                                Priority = reader.GetInt32(1),
                                Deadline = DateTime.Parse(reader.GetString(2)),
                                IsCompleted = reader.GetInt32(3) == 1
                            });
                        }
                    }
                }
            }
        }
        public Task? GetTaskById(int id)
        {
            return tasks.FirstOrDefault(t => t.Id == id);
        }

    }
}
