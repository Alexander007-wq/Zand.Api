using Microsoft.AspNetCore.Mvc;
using zands.Api.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace zands.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ZandController : ControllerBase
{
    private static List<Zand> zands = new List<Zand>
    {
        new Zand{ Id = 1, Name = "Uju", Ratings = 4.8}
    };
    // GET: api/<ZandController>
    [HttpGet]
    public ActionResult<IEnumerable<Zand>> Get()
    {
        return Ok(zands);
    }


    // GET api/<ZandController>/5
    [HttpGet("{id}")]
    public ActionResult<Zand> Get(int id)
    {
        var zand = zands.FirstOrDefault(z => z.Id == id);
        if (zand == null)
        {
            return NotFound();
        }
        return Ok(zand);
    }

    // POST api/<ZandController>
    [HttpPost]
    public ActionResult<Zand> Post([FromBody] Zand newZand)
    {
        if (zands.Any(z => z.Id == newZand.Id))
        {
            return BadRequest("Item with this Id already exists");
        }

        zands.Add(newZand);
        return CreatedAtAction(nameof(Get), new { id = newZand.Id }, newZand);
    }

    // PUT api/<ZandController>/5
    [HttpPut("{id}")]
    public ActionResult Put(int id, [FromBody] Zand UpdatedZand)
    {
        var existingZand = zands.FirstOrDefault(z => z.Id == id);
        if (existingZand == null)
        {
            return NotFound();
        }

        existingZand.Id = UpdatedZand.Id;
        existingZand.Name = UpdatedZand.Name;
        existingZand.Ratings = UpdatedZand.Ratings;

        return NoContent();

    }

    // DELETE api/<ZandController>/5
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var zand = zands.FirstOrDefault(z => z.Id == id);
        if (zand == null)
        {
            return NotFound(new { message = "Item not found" });
        }
        zands.Remove(zand);
        return NoContent();
    }
} 