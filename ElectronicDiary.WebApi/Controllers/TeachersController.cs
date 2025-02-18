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
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherRepository _teacherRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<TeachersController> _logger;

        public TeachersController(ITeacherRepository teacherRepository, IMapper mapper, ILogger<TeachersController> logger)
        {
            _teacherRepository = teacherRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} requested a list of all teachers", userId);

            var teachers = await _teacherRepository.GetAllAsync();
            

            var teacherDtos = _mapper.Map<IEnumerable<TeacherDto>>(teachers);
            return Ok(teacherDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} requested teacher with ID {Id}", userId, id);

            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher with ID {Id} not found", id);
                return NotFound($"Teacher with ID {id} not found.");
            }

            var teacherDto = _mapper.Map<TeacherDto>(teacher);
            return Ok(teacherDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TeacherDto teacherDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is creating a new teacher", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error occurred while creating a teacher");
                return BadRequest(ModelState);
            }

            var teacher = _mapper.Map<Teacher>(teacherDto);
            await _teacherRepository.AddAsync(teacher);

            _logger.LogInformation("Teacher with ID {Id} successfully created", teacher.Id);
            return CreatedAtAction(nameof(GetById), new { id = teacher.Id }, teacherDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TeacherDto teacherDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is updating teacher with ID {Id}", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error occurred while updating teacher with ID {Id}", id);
                return BadRequest(ModelState);
            }

            var existingTeacher = await _teacherRepository.GetByIdAsync(id);
            if (existingTeacher == null)
            {
                _logger.LogWarning("Teacher with ID {Id} not found", id);
                return NotFound($"Teacher with ID {id} not found.");
            }

            _mapper.Map(teacherDto, existingTeacher);
            await _teacherRepository.UpdateAsync(existingTeacher);

            _logger.LogInformation("Teacher with ID {Id} successfully updated", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is deleting teacher with ID {Id}", userId, id);

            var teacher = await _teacherRepository.GetByIdAsync(id);
            if (teacher == null)
            {
                _logger.LogWarning("Teacher with ID {Id} not found", id);
                return NotFound($"Teacher with ID {id} not found.");
            }

            await _teacherRepository.DeleteAsync(id);
            _logger.LogInformation("Teacher with ID {Id} successfully deleted", id);
            return NoContent();
        }
    }
}
