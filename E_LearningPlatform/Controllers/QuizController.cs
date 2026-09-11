using Domain.DTO;
using E_LearningPlatform.Extensions;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Contract;

namespace E_LearningPlatform.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly IQuizService quizService;
        public QuizController(IQuizService _quizService)
        {
            quizService = _quizService;
        }

        [HttpGet("getall")]
        public async Task<IActionResult> GetAllQuizzes(CancellationToken cancellationToken)
        {
            var result = await quizService.GetAllQuizzesAsync(cancellationToken);
            if (result.Count > 0)
                return Ok(result);
            return NotFound("No quizzes found.");
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] QuizDto quiz, CancellationToken cancellationToken)
        {
            var result = await quizService.AddQuizAsync(quiz, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("getById/{id}")]
        public async Task<IActionResult> GetQuiz(int id, CancellationToken cancellationToken)
        {
            var result = await quizService.GetQuizByIdAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] QuizDto quiz, CancellationToken cancellationToken)
        {
            quiz.Id = id;
            var result = await quizService.UpdateQuizAsync(quiz, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await quizService.DeleteQuizAsync(id, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("bylesson/{lessonId}")]
        public async Task<IActionResult> GetQuizByLessonId(int lessonId, CancellationToken cancellationToken)
        {
            var result = await quizService.GetQuizByLessonIdAsync(lessonId, cancellationToken);
            return result.ToActionResult(this);
        }
    }

}
