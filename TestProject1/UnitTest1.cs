
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApplication1;
using WebApplication1.Controllers;
using Xunit;

namespace TestProject1
{
    using System.Collections.Generic;

    public class TaskEqualityComparer : IEqualityComparer<WebApplication1.Task>
    {
        public static TaskEqualityComparer Instance { get; } = new TaskEqualityComparer();

        public bool Equals(WebApplication1.Task x, WebApplication1.Task y)
        {
            // Реализуйте сравнение свойств задачи
            return x.Id == y.Id &&
                   x.Title == y.Title &&
                   x.Priority == y.Priority &&
                   x.Deadline == y.Deadline &&
                   x.IsCompleted == y.IsCompleted;
        }

        public int GetHashCode(WebApplication1.Task obj)
        {
            // Реализуйте хэширование свойств задачи
            return obj.Id.GetHashCode() ^
                   (obj.Title?.GetHashCode() ?? 0) ^
                   obj.Priority.GetHashCode() ^
                   obj.Deadline.GetHashCode() ^
                   obj.IsCompleted.GetHashCode();
        }
    }


    public class TodoControllerTests
    {
        private readonly TodoController _controller;

        public TodoControllerTests()
        {
            var services = new ServiceCollection();
            var todoList = new TodoList(); // Создаем экземпляр TodoList
            services.AddSingleton(todoList);
            services.AddLogging();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddControllers();

            var serviceProvider = services.BuildServiceProvider();
            _controller = new TodoController(todoList);
        }


        [Fact]
        public void AddTask_ReturnsOk()
        {
            // Arrange
            var newTask = new WebApplication1.Task { Id = 4, Title = "New Task", Priority = 1, Deadline = DateTime.Now.AddDays(4), IsCompleted = false };

            // Act
            var result = _controller.AddTask(newTask) as OkResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

            // Check if the task was added
            var addedTask = _controller.GetAllTasks().FirstOrDefault(t => t.Id == newTask.Id);
            Assert.NotNull(addedTask);
            Assert.Equal(newTask, addedTask);
        }

        [Fact]
        public void GetTaskById_ReturnsCorrectTask()
        {
            // Arrange
            var taskId = 1;
            var expectedTask = new WebApplication1.Task { Id = taskId, Title = "Task 1", Priority = 1, Deadline = DateTime.Now.AddDays(1), IsCompleted = false };
            _controller.GetTodoList().Clear();
            _controller.GetTodoList().Add(expectedTask);

            // Act
            var result = _controller.GetTaskById(taskId);

            // Assert
            Assert.NotNull(result);

            if (result is OkObjectResult okResult)
            {
                Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

                var actualTask = okResult.Value as WebApplication1.Task;
                Assert.NotNull(actualTask);

                Assert.Equal(expectedTask, actualTask, TaskEqualityComparer.Instance);
            }
            else if (result is NotFoundResult notFoundResult)
            {
                Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
            }
            else
            {
                Assert.True(false, $"Unexpected result type: {result.GetType()}");
            }
        }





        // Add more tests as needed
    }
}