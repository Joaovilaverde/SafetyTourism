﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SafetyTourism.Data;
using SafetyTourism.Models;

namespace SafetyTourism.Controllers
{
    public class AfectadosPorController : Controller
    {
        private readonly SafetyContext _context;

        public AfectadosPorController(SafetyContext context)
        {
            _context = context;
        }

        // GET: AfectadosPor
        /*public async Task<IActionResult> Index()
        {
            var safetyContext = _context.Afectados.Include(a => a.Destino).Include(a => a.Doenca);
            return View(await safetyContext.ToListAsync());
        }*/

        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["DestinoSortParm"] = String.IsNullOrEmpty(sortOrder) ? "destinoId_desc" : "";
            ViewData["DoencaSortParm"] = sortOrder == "DoencaId" ? "doencaId_desc" : "DoencaId";
            ViewData["DataSortParm"] = sortOrder == "Data" ? "data_desc" : "Data";
            ViewData["InfectadosSortParm"] = sortOrder == "InfectadosPor100k" ? "infectados_desc" : "InfectadosPor100k";
            ViewData["GravidadeSortParm"] = sortOrder == "Gravidade" ? "gravidade_desc" : "Gravidade";

            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewData["CurrentFilter"] = searchString;
            var afectados = from a in _context.Afectados.Include(a => a.Destino).Include(a => a.Doenca)
                            select a;
            if (!String.IsNullOrEmpty(searchString))
            {
                afectados = afectados.Where(a => a.Destino.Nome.Contains(searchString)
                                       || a.Doenca.Nome.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "destinoId_desc":
                    afectados = afectados.OrderByDescending(a => a.Destino.Nome);
                    break;
                case "DoencaId":
                    afectados = afectados.OrderBy(a => a.Doenca.Nome);
                    break;
                case "doencaId_desc":
                    afectados = afectados.OrderByDescending(a => a.Doenca.Nome);
                    break;
                case "Data":
                    afectados = afectados.OrderBy(a => a.Data);
                    break;
                case "data_desc":
                    afectados = afectados.OrderByDescending(a => a.Data);
                    break;
                case "InfectadosPor100k":
                    afectados = afectados.OrderBy(a => a.InfectadosPor100k);
                    break;
                case "infectados_desc":
                    afectados = afectados.OrderByDescending(a => a.InfectadosPor100k);
                    break;
                case "Gravidade":
                    afectados = afectados.OrderBy(a => a.Gravidade);
                    break;
                case "gravidade_desc":
                    afectados = afectados.OrderByDescending(a => a.Gravidade);
                    break;
                default:
                    afectados = afectados.OrderBy(a => a.Destino.Nome);
                    break;
            }
            int pageSize = 10;
            return View(await PaginatedList<AfectadoPor>.CreateAsync(afectados.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        // GET: AfectadosPor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var afectadoPor = await _context.Afectados
                .Include(a => a.Destino)
                .Include(a => a.Doenca)
                .FirstOrDefaultAsync(m => m.AfectadoPorId == id);
            if (afectadoPor == null)
            {
                return NotFound();
            }

            return View(afectadoPor);
        }

        // GET: AfectadosPor/Create
        public IActionResult Create()
        {
            PopulateDestinosDropDownList();
            PopulateDoencasDropDownList();
            return View();
        }

        // POST: AfectadosPor/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AfectadoPorId,DestinoId,DoencaId,Data,InfectadosPor100k,Gravidade")] AfectadoPor afectadoPor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(afectadoPor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDestinosDropDownList(afectadoPor.DestinoId);
            PopulateDoencasDropDownList(afectadoPor.DoencaId);
            return View(afectadoPor);
        }

        // GET: AfectadosPor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var afectadoPor = await _context.Afectados.FindAsync(id);
            if (afectadoPor == null)
            {
                return NotFound();
            }
            PopulateDestinosDropDownList(afectadoPor.DestinoId);
            PopulateDoencasDropDownList(afectadoPor.DoencaId);
            return View(afectadoPor);
        }

        // POST: AfectadosPor/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AfectadoPorId,DestinoId,DoencaId,Data,InfectadosPor100k,Gravidade")] AfectadoPor afectadoPor)
        {
            if (id != afectadoPor.AfectadoPorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(afectadoPor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AfectadoPorExists(afectadoPor.AfectadoPorId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            PopulateDestinosDropDownList(afectadoPor.DestinoId);
            PopulateDoencasDropDownList(afectadoPor.DoencaId);
            return View(afectadoPor);
        }

        // GET: AfectadosPor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var afectadoPor = await _context.Afectados
                .Include(a => a.Destino)
                .Include(a => a.Doenca)
                .FirstOrDefaultAsync(m => m.AfectadoPorId == id);
            if (afectadoPor == null)
            {
                return NotFound();
            }

            return View(afectadoPor);
        }

        // POST: AfectadosPor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var afectadoPor = await _context.Afectados.FindAsync(id);
            _context.Afectados.Remove(afectadoPor);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AfectadoPorExists(int id)
        {
            return _context.Afectados.Any(e => e.AfectadoPorId == id);
        }

        private void PopulateDestinosDropDownList(object selectedDestino = null)
        {
            var destinosQuery = from d in _context.Destinos
                                orderby d.Nome
                                select d;
            ViewBag.DestinoId = new SelectList(destinosQuery.AsNoTracking(), "DestinoId", "Nome", selectedDestino);
        }
        private void PopulateDoencasDropDownList(object selectedDoenca = null)
        {
            var doencasQuery = from r in _context.Doencas
                               orderby r.Nome
                               select r;
            ViewBag.DoencaId = new SelectList(doencasQuery.AsNoTracking(), "DoencaId", "Nome", selectedDoenca);
        }
    }
}