using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReqForge.KmgDemo.Data;
using ReqForge.KmgDemo.DTOs;
using ReqForge.KmgDemo.Models;

namespace ReqForge.KmgDemo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "operator,manager")]
public class ProductionReportsController : ControllerBase
{
    private readonly InMemoryStore _store;

    public ProductionReportsController(InMemoryStore store)
    {
        _store = store;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductionReport>> GetAll()
    {
        return Ok(_store.ProductionReports.OrderByDescending(r => r.Date));
    }

    [HttpGet("{id:int}")]
    public ActionResult<ProductionReport> GetById(int id)
    {
        var report = _store.GetProductionReport(id);
        if (report == null)
            return NotFound(new { error = $"Production report {id} not found." });

        return Ok(report);
    }

    [HttpPost]
    public ActionResult<ProductionReport> Create([FromBody] CreateProductionReportDto dto)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (dto.Date.Year < 2000)
            return BadRequest(new { error = "Date must be a realistic production date." });

        if (_store.GetField(dto.FieldId) == null)
            return NotFound(new { error = $"Field {dto.FieldId} not found." });

        var report = new ProductionReport
        {
            FieldId = dto.FieldId,
            Date = DateTime.SpecifyKind(dto.Date, DateTimeKind.Utc),
            OilTons = dto.OilTons,
            GasThousandM3 = dto.GasThousandM3,
            Comment = dto.Comment.Trim()
        };

        var created = _store.AddProductionReport(report);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }
}
