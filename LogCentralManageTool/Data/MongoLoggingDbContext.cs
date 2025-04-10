﻿using LogCentralManageTool.Data.Entities;

using Microsoft.EntityFrameworkCore;

using MongoDB.EntityFrameworkCore.Extensions;

namespace LogCentralManageTool.Data;
/// <summary>
/// EF Core Provider for MongoDB를 사용하는 로그 전용 DbContext 클래스입니다.
/// </summary>
public class MongoLoggingDbContext : DbContext, ILoggingDbContext
{
    public DbSet<LogMongo> MongoLogs { get; set; }

    // ILoggingDbContext 인터페이스 구현:
    // 필요에 따라 변환하거나 공통 인터페이스(Log)를 사용하도록 합니다.
    public IQueryable<ILog> Logs => MongoLogs;

    /// <summary>
    /// DbContextOptions를 받는 생성자입니다.
    /// </summary>
    /// <param name="options">DbContext 옵션</param>
    public MongoLoggingDbContext(DbContextOptions<MongoLoggingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // MongoDB EF Core Provider 전용 확장 메서드를 사용해 LogMongo 엔티티를 컬렉션 "Log"에 매핑합니다.
        modelBuilder.Entity<LogMongo>().ToCollection("Log");
    }
}