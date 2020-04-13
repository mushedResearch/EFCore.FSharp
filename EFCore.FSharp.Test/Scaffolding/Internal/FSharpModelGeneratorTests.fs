﻿namespace Bricelam.EntityFrameworkCore.FSharp.Test.Scaffolding.Internal

open System.IO
open Microsoft.EntityFrameworkCore.Design
open Microsoft.Extensions.DependencyInjection
open Microsoft.EntityFrameworkCore.Scaffolding
open Microsoft.EntityFrameworkCore.Metadata.Internal
open Bricelam.EntityFrameworkCore.FSharp
open Bricelam.EntityFrameworkCore.FSharp.Test.TestUtilities
open Xunit;
open FsUnit.Xunit

module FSharpModelGeneratorTests =

    let private createGenerator () =
        let services = 
            ServiceCollection()
                .AddEntityFrameworkSqlServer()
                .AddEntityFrameworkDesignTimeServices()                
                .AddSingleton<IAnnotationCodeGenerator, AnnotationCodeGenerator>()
                .AddSingleton<ProviderCodeGenerator, TestProviderCodeGenerator>()
                .AddSingleton<IProviderConfigurationCodeGenerator, TestProviderCodeGenerator>()

        (EFCoreFSharpServices() :> IDesignTimeServices).ConfigureDesignTimeServices(services)

        services
            .BuildServiceProvider()
            .GetRequiredService<IModelCodeGenerator>()            

    [<Fact>]
    let ``Language works`` () =
        let generator = createGenerator()

        let result = generator.Language

        result |> should equal "F#"

    [<Fact>]
    let ``Write code works``() = 
        let generator = createGenerator()
        let modelBuilder = RelationalTestHelpers.Instance.CreateConventionBuilder()

        modelBuilder
            .Entity("TestEntity")
            .Property<int>("Id")
            .HasAnnotation(ScaffoldingAnnotationNames.ColumnOrdinal, 0) |> ignore

        let modelBuilderOptions = 
            ModelCodeGenerationOptions(
                ModelNamespace = "TestNamespace",
                ContextNamespace = "ContextNameSpace",
                ContextDir = Path.Combine("..", (sprintf "%s%c" "TestContextDir" Path.DirectorySeparatorChar)),
                ContextName = "TestContext",
                ConnectionString = "Data Source=Test")

        let result = 
            generator.GenerateModel(
                modelBuilder.Model, 
                modelBuilderOptions)

        result.ContextFile.Path |> should equal (Path.Combine("..", "TestContextDir", "TestContext.fs"))
        Assert.NotEmpty(result.ContextFile.Code)

        Assert.Equal(1, result.AdditionalFiles.Count)
        Assert.Equal("TestDomain.fs", result.AdditionalFiles.[0].Path)
        Assert.NotEmpty(result.AdditionalFiles.[0].Code)


