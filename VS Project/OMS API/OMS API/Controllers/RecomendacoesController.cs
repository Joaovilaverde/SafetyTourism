﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OMS_API.Data;
using OMS_API.Models;

namespace OMS_API.Controllers
{
    [Route("api/recomendacoes")]
    [ApiController]
    public class RecomendacoesController : ControllerBase
    {
        private readonly OMSContext _context;

        public RecomendacoesController(OMSContext context)
        {
            _context = context;
        }

        // GET: api/Recomendacoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Recomendacao>>> GetRecomendacao()
        {
            return await _context.Recomendacao.ToListAsync();
        }

        // GET: api/Recomendacoes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Recomendacao>> GetRecomendacao(long id)
        {
            var recomendacao = await _context.Recomendacao.FindAsync(id);

            if (recomendacao == null)
            {
                return NotFound();
            }

            return recomendacao;
        }

        // PUT: api/Recomendacoes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRecomendacao(long id, Recomendacao recomendacao)
        {
            if (id != recomendacao.Id)
            {
                return BadRequest();
            }

            _context.Entry(recomendacao).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RecomendacaoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Recomendacoes
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Recomendacao>> PostRecomendacao(Recomendacao recomendacao)
        {
            _context.Recomendacao.Add(recomendacao);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRecomendacao", new { id = recomendacao.Id }, recomendacao);
        }

        // DELETE: api/Recomendacoes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecomendacao(long id)
        {
            var recomendacao = await _context.Recomendacao.FindAsync(id);
            if (recomendacao == null)
            {
                return NotFound();
            }

            _context.Recomendacao.Remove(recomendacao);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RecomendacaoExists(long id)
        {
            return _context.Recomendacao.Any(e => e.Id == id);
        }
    }
}
