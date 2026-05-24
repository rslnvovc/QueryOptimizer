using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QueryOptimizer.Bll.Factories;
using QueryOptimizer.Bll.Factories.Abstractions;
using QueryOptimizer.Bll.Services;
using QueryOptimizer.Bll.Services.Abstractions;
using QueryOptimizer.DatabaseExecutor;
using QueryOptimizer.Optimization.Services;
using QueryOptimizer.Optimization.Services.Abstractions;
using QueryOptimizer.Providers.Oracle.Parsing;
using QueryOptimizer.Providers.PostgreSQL.Parsing;
using QueryOptimizer.Providers.SQLServer.Parsing;
using QueryOptimizer.Repositories;
using QueryOptimizer.Repositories.Abstractions;
using QueryOptimizer.Shared.Common.Exceptions.Database;
using QueryOptimizer.Shared.Infrastructure.Abstractions;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.ServiceProcess;
using System.Text;

namespace QueryOptimizer.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static void RegisterApplicationDependencies(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationConnectionString =
                configuration.GetConnectionString("QueryOptimizerDb")
                ?? throw new NotValidConnectionStringException();

            DatabaseExecutorFactory.InitDatabase(Shared.Common.Enums.DatabaseTypes.SqlServer, applicationConnectionString);

            services.AddScoped<IAnalyzingRepository>(_ =>
                new AnalyzingRepository(applicationConnectionString));

            services.AddScoped<IUserRepository>(_ =>
                new UserRepository(applicationConnectionString));

            services.AddScoped<ITargetDatabaseExecutorFactory, TargetDatabaseExecutorFactory>();
            services.AddScoped<IExecutionPlanParserFactory, ExecutionPlanParserFactory>();

            services.AddScoped<IQueryOptimizationWorkflowService, QueryOptimizationWorkflowService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped<IAdaptiveLearningService, AdaptiveLearningService>();

            services.AddScoped<IOptimizationCandidateGenerator, OptimizationCandidateGenerator>();

            services.AddScoped<ISqlQueryImprovementService, SqlQueryImprovementService>();

            //services.AddScoped<IExecutionPlanParser, SqlServerExecutionPlanParser>();
            //services.AddScoped<IExecutionPlanParser, PostgresExecutionPlanParser>();
            //services.AddScoped<IExecutionPlanParser, OracleExecutionPlanParser>();

            //services.AddScoped<IOptimizationRule, SelectStarRule>();
            //services.AddScoped<IOptimizationRule, MissingWhereRule>();
            //services.AddScoped<IOptimizationRule, LeadingWildcardLikeRule>();
            //services.AddScoped<IOptimizationRule, FunctionInWhereRule>();
            //services.AddScoped<IOptimizationRule, FullTableScanRule>();
            //services.AddScoped<IOptimizationRule, ExpensiveSortRule>();
            //services.AddScoped<IOptimizationRule, KeyLookupRule>();
            //services.AddScoped<IOptimizationRule, BadCardinalityEstimateRule>();
            //services.AddScoped<IOptimizationRule, ExpensiveNestedLoopRule>();
            //services.AddScoped<IOptimizationRule, HighLogicalReadsRule>();
            //services.AddScoped<IOptimizationRule, ImplicitJoinRule>();

            //services.AddScoped<ISqlRewriteRule, YearFunctionRewriteRule>();
            //services.AddScoped<ISqlRewriteRule, ImplicitJoinRewriteRule>();

        }
    }
}
