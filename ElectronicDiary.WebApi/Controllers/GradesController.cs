using AutoMapper;
using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.WebApi.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectronicDiary.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GradesController : ControllerBase
    {
        private readonly IGradeRepository _gradeRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GradesController> _logger;

        public GradesController(IGradeRepository gradeRepository, IMapper mapper, ILogger<GradesController> logger)
        {
            _gradeRepository = gradeRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

            _logger.LogInformation("User with ID {UserId} requested all grades", userId);

            var grades = await _gradeRepository.GetAllAsync();
            

            var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(grades);
            return Ok(gradeDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} requested grade with ID {Id}", userId, id);

            var grade = await _gradeRepository.GetByIdAsync(id);
            if (grade == null)
            {
                _logger.LogWarning("Grade with ID {Id} not found", id);
                return NotFound($"Grade with ID {id} not found");
            }

            var gradeDto = _mapper.Map<GradeDto>(grade);
            return Ok(gradeDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GradeDto gradeDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is creating a new grade", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error while creating grade");
                return BadRequest(ModelState);
            }

            var grade = _mapper.Map<Grade>(gradeDto);
            await _gradeRepository.AddAsync(grade);

            _logger.LogInformation("Grade with ID {Id} successfully created", grade.Id);
            return CreatedAtAction(nameof(GetById), new { id = grade.Id }, gradeDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GradeDto gradeDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is updating grade with ID {Id}", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error while updating grade with ID {Id}", id);
                return BadRequest(ModelState);
            }

            var existingGrade = await _gradeRepository.GetByIdAsync(id);
            if (existingGrade == null)
            {
                _logger.LogWarning("Grade with ID {Id} not found", id);
                return NotFound($"Grade with ID {id} not found");
            }

            var grade = _mapper.Map(gradeDto, existingGrade);
            await _gradeRepository.UpdateAsync(grade);

            _logger.LogInformation("Grade with ID {Id} successfully updated", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is deleting grade with ID {Id}", userId, id);

            var grade = await _gradeRepository.GetByIdAsync(id);
            if (grade == null)
            {
                _logger.LogWarning("Grade with ID {Id} not found", id);
                return NotFound($"Grade with ID {id} not found");
            }

            await _gradeRepository.DeleteAsync(id);
            _logger.LogInformation("Grade with ID {Id} successfully deleted", id);

            return NoContent();
        }

        //
        [Authorize(Roles = "Student")]
        [HttpGet("for-current-student")]
        public async Task<IActionResult> GetGradesByStudentId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var grades = await _gradeRepository.GetGradesByStudentIdAsync(userId);

            var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(grades);
            return Ok(gradeDtos);
        }

        [Authorize(Roles = "Teacher")]
        [HttpGet("assignment/{assignmentId}")]
        public async Task<IActionResult> GetGradesByAssignmentId(int assignmentId)
        {
            var grades = await _gradeRepository.GetGradesByAssignmentIdAsync(assignmentId);

            var gradeDtos = _mapper.Map<IEnumerable<GradeDto>>(grades);
            return Ok(gradeDtos);
        }
    }
}
