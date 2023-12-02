using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.Data.Sqlite;

namespace WebApplication1
{
    public class Task
    {
        public Task()
        {
            Title = string.Empty; // или другое значение по умолчанию
        }

        public int Id { get; set; }
        public string? Title { get; set; }
        public int Priority { get; set; }
        public DateTime Deadline { get; set; }
        public bool IsCompleted { get; set; }

        internal static T FromResult<T>(object value)
        {
            throw new NotImplementedException();
        }
    }
}
