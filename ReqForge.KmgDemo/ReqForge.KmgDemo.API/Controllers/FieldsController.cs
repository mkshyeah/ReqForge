using Microsoft.AspNetCore.Mvc;
using ReqForge.KmgDemo.Data;
using ReqForge.KmgDemo.DTOs;
using ReqForge.KmgDemo.Models;

namespace ReqForge.KmgDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FieldsController : ControllerBase
{
    private readonly InMemoryStore _store;

    public FieldsController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Field>> GetAll()
    {
        return Ok(_store.Fields.OrderBy(f => f.Id));
    }

    [HttpGet("{id:int}")]
    public ActionResult<Field> GetById(int id)
    {
        var field = _store.GetField(id);
        if (field == null)
            return NotFound(new { error = $"Field {id} not found." });

        return Ok(field);
    }

    [HttpPost]
    public ActionResult<Field> Create([FromBody] CreateFieldDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var created = _store.AddField(dto.Name.Trim(), dto.Region.Trim(), dto.IsActive);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public ActionResult<Field> Update(int id, [FromBody] UpdateFieldDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (!_store.TryUpdateField(id, dto.Name.Trim(), dto.Region.Trim(), dto.IsActive, out var field))
            return NotFound(new { error = $"Field {id} not found." });

        return Ok(field);
    }

    [HttpDelete("{id:int}")]
    public IActionResult Delete(int id)
    {
        if (!_store.TryDeleteField(id))
            return NotFound(new { error = $"Field {id} not found." });

        return NoContent();
    }
}
