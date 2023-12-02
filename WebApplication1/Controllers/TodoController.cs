using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("api/todo")]
    public class TodoController : ControllerBase
    {
        private readonly TodoList todoList = new TodoList();
        public TodoController(TodoList todoList)
        {
            this.todoList = todoList;
        }

        [HttpGet]
        public IEnumerable<Task> GetAllTasks()
        {
            return todoList.AllTasks();
        }

        [HttpPost]
        public IActionResult AddTask([FromBody] Task task)
        {
            todoList.AddTask(task);
            todoList.SaveChanges(); // Сохраняем изменения
            return Ok();
        }


        [HttpGet("{id}")]
        public IActionResult GetTaskById(int id)
        {
            var task = todoList.GetTaskById(id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }

        [HttpGet("mostimportant")]
        public IEnumerable<Task> GetMostImportantTasks()
        {
            var uncompletedTasks = todoList.GetMostImportantTasks();

            if (uncompletedTasks.Any())
            {
                return uncompletedTasks.OrderBy(t => t.Priority).ToList();
            }
            else
            {
                return Enumerable.Empty<Task>();
            }
        }


        [HttpGet("nearestdeadline")]
        public IEnumerable<Task> GetNearestDeadlineTasks()
        {
            var uncompletedTasks = todoList.GetNearestDeadlineTasks();

            if (uncompletedTasks.Any())
            {
                return uncompletedTasks.OrderBy(t => t.Deadline).ToList();
            }
            else
            {
                return Enumerable.Empty<Task>();
            }
        }



        private static async Task<Task?> ReadTask(HttpContext context)
        {
            var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();

            try
            {
                var task = JsonSerializer.Deserialize<Task>(requestBody);
                return task;
            }
            catch (JsonException)
            {
                return null;
            }
        }
        public List<Task> GetTodoList()
        {
            return todoList.AllTasks().ToList();
        }
    }
}
