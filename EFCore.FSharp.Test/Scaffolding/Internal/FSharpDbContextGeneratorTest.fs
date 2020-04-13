﻿namespace Bricelam.EntityFrameworkCore.FSharp.Test.Scaffolding.Internal

open Microsoft.EntityFrameworkCore.Scaffolding
open Xunit
open FsUnit.Xunit

type FSharpDbContextGeneratorTest() =
    inherit ModelCodeGeneratorTestBase()
    
    [<Fact>]
    member this.``Empty model`` () =
        let emptyModelDbContext = """namespace TestNamespace

open System
open System.Collections.Generic
open Microsoft.EntityFrameworkCore
open Microsoft.EntityFrameworkCore.Metadata
open Bricelam.EntityFrameworkCore.FSharp.Extensions

    open TestDbDomain

    type TestDbContext =
        inherit DbContext

        new() = { inherit DbContext() }
        new(options : DbContextOptions<TestDbContext>) =
            { inherit DbContext(options) }

        override this.OnConfiguring(optionsBuilder: DbContextOptionsBuilder) =
            if not optionsBuilder.IsConfigured then
                optionsBuilder.UseSqlServer("Initial Catalog=TestDatabase") |> ignore
                ()

        override this.OnModelCreating(modelBuilder: ModelBuilder) =
            base.OnModelCreating(modelBuilder)

            modelBuilder.RegisterOptionTypes()
"""

        base.Test(
            (fun m -> ()),
            (ModelCodeGenerationOptions()),
            (fun code -> emptyModelDbContext |> should equal code.ContextFile.Code),
            (fun model -> Assert.Empty(model.GetEntityTypes()))
        )
