﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SafetyTourism.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;

namespace SafetyTourism.Controllers
{
    public class RecomendacoesController : Controller
    {
        private readonly IConfiguration _configure;
        private readonly string apiBaseUrl;

        // Construtor do controller
        public RecomendacoesController(IConfiguration configuration)
        {
            _configure = configuration;
            apiBaseUrl = _configure.GetValue<string>("WebAPIBaseUrl");
        }

        // GET RECOMENDAÇÕES NO INDEX A APARECER
        public async Task<IActionResult> Index(string sortOrder, string currentFilter, string searchString, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["ZonaSortParm"] = String.IsNullOrEmpty(sortOrder) ? "zona_desc" : "";
            ViewData["InformacaoSortParm"] = sortOrder == "inf" ? "inf_desc" : "inf";
            ViewData["DataSortParm"] = sortOrder == "data" ? "data_desc" : "data";
            ViewData["ValidadeSortParm"] = sortOrder == "val" ? "val_desc" : "val";
            if (searchString != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            ViewData["CurrentFilter"] = searchString;
            var listaRecomendacoes = new List<Recomendacao>();
            using (HttpClient client = new HttpClient())
            {
                UserInfo user = new UserInfo();
                StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                string endpoint = apiBaseUrl + "/recomendacoes";
                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                listaRecomendacoes = await response.Content.ReadAsAsync<List<Recomendacao>>();
            }
            IQueryable<Recomendacao> recomendacoes = (from r in listaRecomendacoes select r).AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                recomendacoes = recomendacoes.Where(r => r.Informacao.Contains(searchString) || r.Zona.Nome.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "zona_desc":
                    recomendacoes = recomendacoes.OrderByDescending(r => r.Zona.Nome);
                    break;
                case "inf":
                    recomendacoes = recomendacoes.OrderBy(r => r.Informacao);
                    break;
                case "inf_desc":
                    recomendacoes = recomendacoes.OrderByDescending(r => r.Informacao);
                    break;
                case "data":
                    recomendacoes = recomendacoes.OrderBy(r => r.Data);
                    break;
                case "data_desc":
                    recomendacoes = recomendacoes.OrderByDescending(r => r.Data);
                    break;
                case "val":
                    recomendacoes = recomendacoes.OrderBy(r => r.Validade);
                    break;
                case "val_desc":
                    recomendacoes = recomendacoes.OrderByDescending(r => r.Validade);
                    break;
                default:
                    recomendacoes = recomendacoes.OrderBy(r => r.Zona.Nome);
                    break;
            }
            int pageSize = 10;
            return View(await PaginatedList<Recomendacao>.CreateAsync(recomendacoes, pageNumber ?? 1, pageSize));
        }

        //DETAILS

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Recomendacao recomendacao;
            using (HttpClient client = new HttpClient())
            {
                UserInfo user = new UserInfo();
                StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                string endpoint = apiBaseUrl + "/recomendacoes/" + id;
                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                recomendacao = await response.Content.ReadAsAsync<Recomendacao>();
            }
            if (recomendacao == null)
            {
                return NotFound();
            }
            return View(recomendacao);
        }

        //CREATE GET
        [Authorize(Roles = "Funcionario,Administrador")]
        public async Task<IActionResult> Create()
        {
            var listaZonas = new List<Zona>();
            using (HttpClient client = new HttpClient())
            {
                UserInfo user = new UserInfo();
                StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                string endpoint = apiBaseUrl + "/zonas";
                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                listaZonas = await response.Content.ReadAsAsync<List<Zona>>();
            }
            PopulateZonasDropDownList(listaZonas);
            return View();
        }

        //CREATE POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Funcionario,Administrador")]
        public async Task<IActionResult> Create([Bind("Id,Validade,Informacao, Data, ZonaId")] Recomendacao recomendacao)
        {
            if (ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    UserInfo user = new UserInfo();
                    StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                    var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                    UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                    StringContent content = new StringContent(JsonConvert.SerializeObject(recomendacao), Encoding.UTF8, "application/json");
                    string endpoint = apiBaseUrl + "/recomendacoes";
                    var response = await client.PostAsync(endpoint, content);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recomendacao);
        }

        //DELETE GET
        [Authorize(Roles = "Funcionario,Administrador")]
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Recomendacao recomendacao;
            using (HttpClient client = new HttpClient())
            {
                UserInfo user = new UserInfo();
                StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                string endpoint = apiBaseUrl + "/recomendacoes/" + id;
                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                recomendacao = await response.Content.ReadAsAsync<Recomendacao>();
            }
            if (recomendacao == null)
            {
                return NotFound();
            }
            return View(recomendacao);
        }

        //DELETE POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Funcionario,Administrador")]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            using (HttpClient client = new HttpClient())
            {
                UserInfo user = new UserInfo();
                StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                string endpoint = apiBaseUrl + "/recomendacoes/" + id;
                var response = await client.DeleteAsync(endpoint);
            }
            return RedirectToAction(nameof(Index));
        }

        //EDIT GET
        [Authorize(Roles = "Funcionario,Administrador")]
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            Recomendacao recomendacao;
            using (HttpClient client = new HttpClient())
            {
                UserInfo user = new UserInfo();
                StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                string endpoint = apiBaseUrl + "/recomendacoes/" + id;
                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                recomendacao = await response.Content.ReadAsAsync<Recomendacao>();
            }
            if (recomendacao == null)
            {
                return NotFound();
            }
            var listaZonas = new List<Zona>();
            using (HttpClient client = new HttpClient())
            {
                UserInfo user = new UserInfo();
                StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                string endpoint = apiBaseUrl + "/zonas";
                var response = await client.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();
                listaZonas = await response.Content.ReadAsAsync<List<Zona>>();
            }
            PopulateZonasDropDownList(listaZonas, id);
            return View(recomendacao);
        }

        //EDIT POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Funcionario,Administrador")]
        public async Task<IActionResult> Edit(long id, [Bind("Id,Validade,Informacao,Data,ZonaId")] Recomendacao recomendacao)
        {
            if (id != recomendacao.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                using (HttpClient client = new HttpClient())
                {
                    UserInfo user = new UserInfo();
                    StringContent contentUser = new StringContent(JsonConvert.SerializeObject(user), Encoding.UTF8, "application/json");
                    var responseLogin = await client.PostAsync(apiBaseUrl + "/users/login", contentUser);
                    UserToken token = await responseLogin.Content.ReadAsAsync<UserToken>();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Token);
                    StringContent content = new StringContent(JsonConvert.SerializeObject(recomendacao), Encoding.UTF8, "application/json");
                    string endpoint = apiBaseUrl + "/recomendacoes/" + id;
                    var response = await client.PutAsync(endpoint, content);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(recomendacao);
        }

        private void PopulateZonasDropDownList(List<Zona> listaZonas, object selectedZona = null)
        {
            var zonasQuery = from z in listaZonas
                             orderby z.Nome
                             select z;
            ViewBag.Zona = new SelectList(zonasQuery, "Id", "Nome", selectedZona);
        }
    }
}
