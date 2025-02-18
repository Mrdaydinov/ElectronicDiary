using AutoMapper;
using ElectronicDiary.Application.Interfaces;
using ElectronicDiary.Domain.Entities;
using ElectronicDiary.WebAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ElectronicDiary.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentRepository _studentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<StudentsController> _logger;

        public StudentsController(IStudentRepository studentRepository, IMapper mapper, ILogger<StudentsController> logger)
        {
            _studentRepository = studentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

            _logger.LogInformation("User with ID {UserId} requested the list of all students", userId);

            var students = await _studentRepository.GetAllAsync();


            var studentsDtos = _mapper.Map<IEnumerable<StudentDto>>(students);
            return Ok(studentsDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} requested student with ID: {Id}", userId, id);

            var student = await _studentRepository.GetByIdAsync(id);
            if (student == null)
            {
                _logger.LogWarning("Student with ID {Id} not found", id);
                return NotFound("Student not found");
            }

            var studentDto = _mapper.Map<StudentDto>(student);
            return Ok(studentDto);
        }

        [HttpGet("for-current-teacher")]
        public async Task<IActionResult> GetStudentsForCurrentTeacher()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";

            var students = await _studentRepository.GetStudentsByTeacherIdAsync(userId);

            var studentsDtos = _mapper.Map<IEnumerable<StudentDto>>(students);
            return Ok(studentsDtos);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StudentDto studentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is attempting to create a new student", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error while creating student");
                return BadRequest(ModelState);
            }

            var student = _mapper.Map<Student>(studentDto);
            await _studentRepository.AddAsync(student);

            _logger.LogInformation("Student with ID {Id} successfully created", student.Id);
            return CreatedAtAction(nameof(GetById), new { id = student.Id }, studentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentDto studentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is attempting to update student with ID: {Id}", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error while updating student with ID {Id}", id);
                return BadRequest(ModelState);
            }

            var existingStudent = await _studentRepository.GetByIdAsync(id);
            if (existingStudent == null)
            {
                _logger.LogWarning("Student with ID {Id} not found for update", id);
                return NotFound("Student not found");
            }

            _mapper.Map(studentDto, existingStudent);
            await _studentRepository.UpdateAsync(existingStudent);

            _logger.LogInformation("Student with ID {Id} successfully updated", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is attempting to delete student with ID: {Id}", userId, id);

            var existingStudent = await _studentRepository.GetByIdAsync(id);
            if (existingStudent == null)
            {
                _logger.LogWarning("Student with ID {Id} not found for deletion", id);
                return NotFound("Student not found");
            }

            await _studentRepository.DeleteAsync(id);

            _logger.LogInformation("Student with ID {Id} successfully deleted", id);
            return NoContent();
        }
    }
}
