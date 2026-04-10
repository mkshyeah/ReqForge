using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReqForge.KmgDemo.Data;
using ReqForge.KmgDemo.Models;

namespace ReqForge.KmgDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "operator,manager")]
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
        return Ok(_store.Fields);
    }

    [HttpGet("{id:int}")]
    public ActionResult<Field> GetById(int id)
    {
        var field = _store.GetField(id);
        if (field == null)
            return NotFound(new { error = $"Field {id} not found." });

        return Ok(field);
    }
}
