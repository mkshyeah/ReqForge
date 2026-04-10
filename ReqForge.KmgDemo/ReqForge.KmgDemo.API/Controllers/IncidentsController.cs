using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReqForge.KmgDemo.Data;
using ReqForge.KmgDemo.DTOs;
using ReqForge.KmgDemo.Models;

namespace ReqForge.KmgDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "operator,manager")]
public class IncidentsController : ControllerBase
{
    private readonly InMemoryStore _store;

    public IncidentsController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Incident>> GetAll()
    {
        return Ok(_store.Incidents.OrderByDescending(i => i.CreatedAt));
    }

    [HttpGet("{id:int}")]
    public ActionResult<Incident> GetById(int id)
    {
        var incident = _store.GetIncident(id);
        if (incident == null)
            return NotFound(new { error = $"Incident {id} not found." });

        return Ok(incident);
    }

    [HttpPost]
    public ActionResult<Incident> Create([FromBody] CreateIncidentDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (_store.GetField(dto.FieldId) == null)
            return NotFound(new { error = $"Field {dto.FieldId} not found." });

        var incident = new Incident
        {
            FieldId = dto.FieldId,
            Title = dto.Title.Trim(),
            Severity = dto.Severity,
            Status = "open",
            CreatedAt = DateTime.UtcNow
        };

        var created = _store.AddIncident(incident);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Только manager может менять статус (демо разграничения прав).</summary>
    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "manager")]
    public ActionResult<Incident> UpdateStatus(int id, [FromBody] UpdateIncidentStatusDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!_store.TryUpdateIncidentStatus(id, dto.Status, out var incident))
            return NotFound(new { error = $"Incident {id} not found." });

        return Ok(incident);
    }
}
