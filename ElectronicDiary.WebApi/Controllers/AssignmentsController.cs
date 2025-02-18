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
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentRepository _assignmentRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<AssignmentsController> _logger;

        public AssignmentsController(IAssignmentRepository assignmentRepository, IMapper mapper, ILogger<AssignmentsController> logger)
        {
            _assignmentRepository = assignmentRepository;
            _mapper = mapper;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} requested all assignments", userId);

            var assignments = await _assignmentRepository.GetAllAsync();


            var assignmentDtos = _mapper.Map<IEnumerable<AssignmentDto>>(assignments);
            return Ok(assignmentDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} requested assignment with ID {Id}", userId, id);

            var assignment = await _assignmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                _logger.LogWarning("Assignment with ID {Id} not found", id);
                return NotFound();
            }

            var assignmentDto = _mapper.Map<AssignmentDto>(assignment);
            return Ok(assignmentDto);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] AssignmentDto assignmentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is creating a new assignment", userId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error while creating assignment");
                return BadRequest(ModelState);
            }

            var assignment = _mapper.Map<Assignment>(assignmentDto);
            await _assignmentRepository.AddAsync(assignment);

            _logger.LogInformation("Assignment with ID {Id} successfully created", assignment.Id);
            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignmentDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AssignmentDto assignmentDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is updating assignment with ID {Id}", userId, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation error while updating assignment with ID {Id}", id);
                return BadRequest(ModelState);
            }

            var existingAssignment = await _assignmentRepository.GetByIdAsync(id);
            if (existingAssignment == null)
            {
                _logger.LogWarning("Assignment with ID {Id} not found", id);
                return NotFound();
            }

            _mapper.Map(assignmentDto, existingAssignment);
            await _assignmentRepository.UpdateAsync(existingAssignment);

            _logger.LogInformation("Assignment with ID {Id} successfully updated", id);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            _logger.LogInformation("User with ID {UserId} is deleting assignment with ID {Id}", userId, id);

            var assignment = await _assignmentRepository.GetByIdAsync(id);
            if (assignment == null)
            {
                _logger.LogWarning("Assignment with ID {Id} not found", id);
                return NotFound();
            }

            await _assignmentRepository.DeleteAsync(id);
            _logger.LogInformation("Assignment with ID {Id} successfully deleted", id);

            return NoContent();
        }


        //
        [Authorize(Roles = "Teacher")]
        [HttpGet("for-current-teacher")]
        public async Task<IActionResult> GetAssignmentsByTeacherId()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "Unknown";
            var assignments = await _assignmentRepository.GetAssignmentsByTeacherIdAsync(userId);

            var assignmentDtos = _mapper.Map<IEnumerable<AssignmentDto>>(assignments);
            return Ok(assignmentDtos);
        }
    }

}
