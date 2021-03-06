﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OMS_API.Models;

namespace OMS_API.Data
{
    public class OMSContext : IdentityDbContext<ApplicationUser>
    {
        public OMSContext(DbContextOptions<OMSContext> options)
            : base(options)
        {
        }

        public DbSet<OMS_API.Models.Surto> Surtos { get; set; }
        public DbSet<OMS_API.Models.Virus> Virus { get; set; }
        public DbSet<OMS_API.Models.Zona> Zonas { get; set; }
        public DbSet<OMS_API.Models.Pais> Paises { get; set; }
        public DbSet<OMS_API.Models.Recomendacao> Recomendacoes { get; set; }
    }
}
