using Microsoft.AspNetCore.Mvc;
using zands.Api.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace zands.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class KraftController : ControllerBase
{
    private static List<Kraft> krafts = new List<Kraft>
    {
        new Kraft{ Id = 1, Name = "Uju", Color = "Red", Fit = "Small", Size = 40}
    };
    // GET: api/<ZandController>
    [HttpGet]
    public ActionResult<IEnumerable<Kraft>> Get()
    {
        return Ok(krafts);
    }

    
    // GET api/<ZandController>/5
    [HttpGet("{id}")]
    public ActionResult<Kraft> Get(int id)
    {
       var kraft = krafts.FirstOrDefault(k => k.Id ==id);
        if (kraft == null)
        {
            return NotFound();
        }
        return Ok(kraft);
    }

    // POST api/<ZandController>
    [HttpPost]
    public ActionResult<Kraft> Post([FromBody] Kraft newKraft)
    {
        if(krafts.Any(z => z.Id == newKraft.Id))
        {
            return BadRequest("Item with this Id already exists");
        }

        krafts.Add(newKraft);
        return CreatedAtAction(nameof(Get), new { id = newKraft.Id }, newKraft);
    }

    // PUT api/<ZandController>/5
    [HttpPut("{id}")]
    public ActionResult Put(int id, [FromBody] Kraft UpdatedKraft)
    {
        var existingKraft = krafts.FirstOrDefault(z => z.Id == id);
        if(existingKraft == null)
        {
            return NotFound();
        }

        existingKraft.Id = UpdatedKraft.Id;
        existingKraft.Name = UpdatedKraft.Name;
        existingKraft.Size = UpdatedKraft.Size;
        existingKraft.Fit = UpdatedKraft.Fit;
        existingKraft.Color = UpdatedKraft.Color;   
        return NoContent();
            
    }

    // DELETE api/<ZandController>/5
    [HttpDelete("{id}")]
    public ActionResult Delete(int id)
    {
        var kraft = krafts.FirstOrDefault(z => z.Id == id);
        if (kraft == null)
        {
            return NotFound(new { message = "Item not found" });
        }
        krafts.Remove(kraft);
        return NoContent();
    }   } 